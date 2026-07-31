#Requires -Version 5.1
<#
    NiNa Deploy  (Ninja Nation - Deploy)  --  PowerShell / WinForms port
    ----------------------------------------------------------------------
    A faithful rewrite of the C# WinForms tool of the same name.

    Deploys a package/script to a list of remote Windows computers. For each
    target it runs the pipeline:
        Ping  ->  C$ admin-share check  ->  WMI test  ->  copy package to
        remote temp folder  ->  execute remotely with PsExec  ->  capture
        exit code  ->  clean up the copied file.

    Concurrency is bounded (Max Remote Threads) with a configurable delay
    between launches. Results land in a grid that can be exported to CSV.

    Original by Andrew J Torsky, TSgt, USAF / 633d Communications Squadron.
    Requires PsExec64.exe (Sysinternals). Drop it in .\Resources next to
    this script, or point at it in the Config section.
#>

# ----------------------------------------------------------------------------
# Ensure single-threaded apartment (required for WinForms). Re-launch if not.
# ----------------------------------------------------------------------------
if ([System.Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    $exe = (Get-Process -Id $PID).Path
    Start-Process -FilePath $exe -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-Sta','-File',"`"$PSCommandPath`"")
    return
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Management       # WMI ManagementScope / ConnectionOptions
[System.Windows.Forms.Application]::EnableVisualStyles()

$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }

# ----------------------------------------------------------------------------
# Constants (mirrors Utility.cs)
# ----------------------------------------------------------------------------
$Version   = '1.2-ps'
$AuthorInfo = @'
Ninja Nation - Deploy

Developed by:
Andrew J Torsky, TSgt, USAF
633d Communications Squadron
JBLE-Langley AFB Virginia

PowerShell port
'@

$StatusReady = 'Ready...'

# ----------------------------------------------------------------------------
# Settings (replaces .NET user settings / App.config). Persisted as JSON in
# %APPDATA%\NiNa-Deploy\settings.json so tweaks survive between runs.
# ----------------------------------------------------------------------------
$SettingsDir  = Join-Path $env:APPDATA 'NiNa-Deploy'
$SettingsPath = Join-Path $SettingsDir 'settings.json'

$DefaultSettings = [ordered]@{
    # Extension | Parent Process | Argument template (#PACKAGE is the placeholder)
    FileExtensions = @(
        '.bat|cmd.exe|'
        '.cab|dism.exe|/online /add-package /packagepath:#PACKAGE'
        '.cmd|cmd.exe|/C #PACKAGE'
        '.exe||'
        '.msi|msiexec.exe|/i #PACKAGE /quiet /norestart'
        '.msp|msiexec.exe|/p #PACKAGE /quiet /norestart'
        '.msu|wusa.exe|#PACKAGE /quiet /norestart'
        '.ps1|powershell.exe|-file #PACKAGE -verb runas -ExecutionPolicy Bypass'
        '.reg|reg32.exe|/s #PACKAGE'
        '.vbs|cscript.exe|#PACKAGE'
        'Custom'
    ) -join "`r`n"
    TempLocation   = 'C:\Windows\Temp\NiNa'
    PingTimeout    = 1000
    PsExecLocation = ''
    MaxThreads     = 1
    ThreadInterval = 500
    PsExecAsSystem = $true
    Domains        = @(
        'USAF.MIL|UNCLASSIFIED/CUI|NIPRNET'
        'SMIL.MIL|CLASSIFIED/SECRET/SIPRNET'
    ) -join "`r`n"
}

function Load-Settings {
    if (Test-Path $SettingsPath) {
        try {
            $loaded = Get-Content -Raw -LiteralPath $SettingsPath | ConvertFrom-Json
            $s = [ordered]@{}
            foreach ($k in $DefaultSettings.Keys) {
                $s[$k] = if ($null -ne $loaded.$k) { $loaded.$k } else { $DefaultSettings[$k] }
            }
            return $s
        } catch {
            return ([ordered]@{} + $DefaultSettings)
        }
    }
    return ([ordered]@{} + $DefaultSettings)
}

function Save-Settings {
    param($Settings)
    if (-not (Test-Path $SettingsDir)) { New-Item -ItemType Directory -Path $SettingsDir -Force | Out-Null }
    ($Settings | ConvertTo-Json -Depth 5) | Set-Content -LiteralPath $SettingsPath -Encoding UTF8
}

$Settings = Load-Settings

# ----------------------------------------------------------------------------
# Shared, thread-safe state consumed by worker runspaces and the UI timer.
# ----------------------------------------------------------------------------
$App = [hashtable]::Synchronized(@{})
$App.Results             = [System.Collections.ArrayList]::Synchronized([System.Collections.ArrayList]::new())
$App.Abort              = $false
$App.AbortOnWMIFail     = $false
$App.TotalThreadsCreated = 0
$App.RemoteActive        = 0
$App.Done                = $true

# ----------------------------------------------------------------------------
# Per-host worker. Runs inside a runspace-pool runspace. Uses .NET directly so
# it needs no imported functions -- everything arrives via arguments.
#   $App    - shared synchronized state
#   $row    - synchronized hashtable representing this host's result row
#   $target - hostname / IP
#   $cfg    - snapshot of execution parameters
# ----------------------------------------------------------------------------
$WorkerScript = {
    param($App, $row, $target, $cfg)

    function Set-Fields([hashtable]$r, [hashtable]$vals) {
        [System.Threading.Monitor]::Enter($App.SyncRoot)
        try { foreach ($k in $vals.Keys) { $r[$k] = $vals[$k] } }
        finally { [System.Threading.Monitor]::Exit($App.SyncRoot) }
    }

    [System.Threading.Monitor]::Enter($App.SyncRoot)
    try { $App.RemoteActive++ } finally { [System.Threading.Monitor]::Exit($App.SyncRoot) }

    try {
        Start-Sleep -Milliseconds 500

        # ---- DIAGNOSTIC: PING --------------------------------------------
        $errorCode = ''
        try {
            $ping = New-Object System.Net.NetworkInformation.Ping
            $opts = New-Object System.Net.NetworkInformation.PingOptions(64, $true)
            $buffer = [System.Text.Encoding]::ASCII.GetBytes('00000000000000000000000000000000')
            $reply = $ping.Send($target, [int]$cfg.PingTimeout, $buffer, $opts)
            $errorCode = $reply.Status.ToString()
        } catch {
            $errorCode = $_.Exception.Message
        }
        if ($errorCode -ne 'Success') {
            Set-Fields $row @{ PingCode=$errorCode; ShareCode='Fail:Ping'; WMICode='Fail:Ping'; CMD='Fail:Ping'; PID='Fail:Ping'; ExitCode='Fail:Ping' }
            return
        }
        Set-Fields $row @{ PingCode=$errorCode }
        Start-Sleep -Milliseconds 500

        # ---- DIAGNOSTIC: C$ SHARE ----------------------------------------
        $shareOk = Test-Path -LiteralPath "\\$target\C$"
        if (-not $shareOk) {
            Set-Fields $row @{ ShareCode='Fail:Share'; WMICode='Fail:Share'; CMD='Fail:Share'; PID='Fail:Share'; ExitCode='Fail:Share' }
            return
        }
        Set-Fields $row @{ ShareCode='Success' }
        Start-Sleep -Milliseconds 500

        # ---- DIAGNOSTIC: WMI ---------------------------------------------
        $wmi = ''
        try {
            $co = New-Object System.Management.ConnectionOptions
            $co.Impersonation   = [System.Management.ImpersonationLevel]::Impersonate
            $co.EnablePrivileges = $true
            $co.Timeout          = [TimeSpan]::FromMilliseconds([int]$cfg.PingTimeout)
            $scope = New-Object System.Management.ManagementScope("\\$target\root\cimv2", $co)
            $scope.Connect()
            $wmi = if ($scope.IsConnected) { 'Success' } else { 'Failed' }
        } catch [UnauthorizedAccessException] {
            $wmi = "Access Denied: $($_.Exception.Message)"
        } catch {
            $wmi = "Error: $($_.Exception.Message)"
        }
        if ($wmi -ne 'Success') {
            Set-Fields $row @{ WMICode=$wmi; CMD='Fail:WMI'; PID='Fail:WMI'; ExitCode='Fail:WMI' }
            if ($App.AbortOnWMIFail) { return }
        } else {
            Set-Fields $row @{ WMICode='Success' }
        }
        Start-Sleep -Milliseconds 500

        # ---- PREP: COMMAND -----------------------------------------------
        Set-Fields $row @{ CMD=$cfg.CmdLine }

        # ---- PREP: FILE COPY OVER ADMIN SHARE ----------------------------
        $remoteDir = "\\$target\" + ($cfg.TempFolder -replace '^C:', 'C$')
        $fileName  = Split-Path -Leaf $cfg.PackagePath
        $remoteFile = Join-Path $remoteDir $fileName

        if (-not (Test-Path -LiteralPath $remoteDir)) {
            try { New-Item -ItemType Directory -Path $remoteDir -Force -ErrorAction Stop | Out-Null }
            catch { Set-Fields $row @{ PID='Fail:RemoteDirectoryCreation'; ExitCode=$_.Exception.Message }; return }
        }
        if (-not (Test-Path -LiteralPath $remoteFile)) {
            try { Copy-Item -LiteralPath $cfg.PackagePath -Destination $remoteFile -Force -ErrorAction Stop }
            catch { Set-Fields $row @{ PID='Fail:RemoteFileCopy'; ExitCode=$_.Exception.Message }; return }
        }

        # ---- EXECUTE: PsExec ---------------------------------------------
        $arguments = "\\$target -accepteula -s -i $($cfg.CmdLine)"
        if (-not $cfg.AsSystem)    { $arguments = $arguments -replace ' -s', '' }
        if (-not $cfg.Interactive) { $arguments = $arguments -replace ' -i', '' }

        try {
            $psi = New-Object System.Diagnostics.ProcessStartInfo
            $psi.FileName  = $cfg.PsExecPath
            $psi.Arguments = $arguments
            $psi.UseShellExecute        = $false
            $psi.WindowStyle            = [System.Diagnostics.ProcessWindowStyle]::Hidden
            $psi.RedirectStandardOutput = $true
            $psi.RedirectStandardError  = $true
            $psi.RedirectStandardInput  = $true
            $proc = [System.Diagnostics.Process]::Start($psi)
            Set-Fields $row @{ PID=$proc.Id.ToString() }
            $proc.WaitForExit()
            Set-Fields $row @{ ExitCode=$proc.ExitCode.ToString() }
        } catch {
            Set-Fields $row @{ PID='Fail:Execute'; ExitCode=$_.Exception.Message }
            return
        }

        # ---- CLEANUP -----------------------------------------------------
        if (Test-Path -LiteralPath $remoteFile) {
            try { Remove-Item -LiteralPath $remoteFile -Force -ErrorAction Stop } catch { }
        }
    }
    finally {
        [System.Threading.Monitor]::Enter($App.SyncRoot)
        try { $App.RemoteActive-- } finally { [System.Threading.Monitor]::Exit($App.SyncRoot) }
    }
}

# ----------------------------------------------------------------------------
# Coordinator. Owns the runspace pool, dispatches one worker per host honoring
# max concurrency + inter-thread interval, then waits for completion. Runs in
# its own runspace so the UI never blocks.
# ----------------------------------------------------------------------------
$CoordinatorScript = {
    param($App, $hosts, $cfg, $worker)

    $pool = [runspacefactory]::CreateRunspacePool(1, [Math]::Max(1, [int]$cfg.MaxThreads))
    $pool.ApartmentState = 'MTA'
    $pool.Open()
    $handles = New-Object System.Collections.ArrayList

    foreach ($target in $hosts) {
        if ($App.Abort) { break }

        $row = [hashtable]::Synchronized(@{
            Date       = (Get-Date).ToLongDateString()
            Time       = (Get-Date).ToLongTimeString()
            RemoteHost = $target
            PingCode   = ''
            ShareCode  = ''
            WMICode    = ''
            CMD        = ''
            PID        = ''
            ExitCode   = ''
        })
        [void]$App.Results.Add($row)

        $ps = [powershell]::Create()
        $ps.RunspacePool = $pool
        [void]$ps.AddScript($worker).AddArgument($App).AddArgument($row).AddArgument($target).AddArgument($cfg)
        $async = $ps.BeginInvoke()
        [void]$handles.Add([pscustomobject]@{ PS=$ps; Async=$async })

        [System.Threading.Monitor]::Enter($App.SyncRoot)
        try { $App.TotalThreadsCreated++ } finally { [System.Threading.Monitor]::Exit($App.SyncRoot) }

        Start-Sleep -Milliseconds ([int]$cfg.ThreadInterval)
    }

    # Wait for outstanding workers (unless aborted).
    foreach ($h in $handles) {
        if ($App.Abort) { break }
        try { $h.PS.EndInvoke($h.Async) } catch { }
        $h.PS.Dispose()
    }
    try { $pool.Close(); $pool.Dispose() } catch { }
    $App.Done = $true
}

# ----------------------------------------------------------------------------
# GUI  (mirrors MainForm.Designer.cs layout & control names)
# ----------------------------------------------------------------------------
$form = New-Object System.Windows.Forms.Form
$form.Text          = 'NiNa Deploy'
$form.ClientSize    = New-Object System.Drawing.Size(868, 649)
$form.MinimumSize   = New-Object System.Drawing.Size(884, 688)
$form.BackColor     = [System.Drawing.Color]::Silver
$form.StartPosition = 'CenterScreen'
$icoPath = Join-Path $ScriptDir 'Resources\Ninja.ico'
if (Test-Path $icoPath) { try { $form.Icon = New-Object System.Drawing.Icon($icoPath) } catch { } }

$tip = New-Object System.Windows.Forms.ToolTip
$tip.ShowAlways = $true

# ---- Menu ------------------------------------------------------------------
$menu = New-Object System.Windows.Forms.MenuStrip
$miFile  = New-Object System.Windows.Forms.ToolStripMenuItem('File')
$miExit  = New-Object System.Windows.Forms.ToolStripMenuItem('Exit')
$miExit.Add_Click({ $form.Close() })
[void]$miFile.DropDownItems.Add($miExit)

$miData    = New-Object System.Windows.Forms.ToolStripMenuItem('Data')
$miExport  = New-Object System.Windows.Forms.ToolStripMenuItem('Export')
[void]$miData.DropDownItems.Add($miExport)

$miHelp    = New-Object System.Windows.Forms.ToolStripMenuItem('Help')
$miArgs    = New-Object System.Windows.Forms.ToolStripMenuItem('Arguments')
$miAbout   = New-Object System.Windows.Forms.ToolStripMenuItem('About')
[void]$miHelp.DropDownItems.AddRange(@($miArgs, $miAbout))
[void]$menu.Items.AddRange(@($miFile, $miData, $miHelp))
$form.MainMenuStrip = $menu

# ---- Left tab control (Target / Monitoring) --------------------------------
$tabControl2 = New-Object System.Windows.Forms.TabControl
$tabControl2.Location = New-Object System.Drawing.Point(0, 27)
$tabControl2.Size     = New-Object System.Drawing.Size(222, 597)
$tabControl2.Anchor   = 'Top,Bottom,Left'
$tabTarget     = New-Object System.Windows.Forms.TabPage('Target')
$tabMonitoring = New-Object System.Windows.Forms.TabPage('Monitoring')
$tabTarget.BackColor     = [System.Drawing.Color]::Silver
$tabMonitoring.BackColor = [System.Drawing.Color]::Silver
[void]$tabControl2.TabPages.AddRange(@($tabTarget, $tabMonitoring))

# Target Computer group
$grpTargetComputer = New-Object System.Windows.Forms.GroupBox
$grpTargetComputer.Text     = 'Target Computer'
$grpTargetComputer.Location = New-Object System.Drawing.Point(6, 6)
$grpTargetComputer.Size     = New-Object System.Drawing.Size(200, 54)
$txtTargetComputer = New-Object System.Windows.Forms.TextBox
$txtTargetComputer.Location = New-Object System.Drawing.Point(6, 22)
$txtTargetComputer.Size     = New-Object System.Drawing.Size(188, 23)
$txtTargetComputer.Anchor   = 'Top,Left,Right'
[void]$grpTargetComputer.Controls.Add($txtTargetComputer)

# Computer List group
$grpComputerList = New-Object System.Windows.Forms.GroupBox
$grpComputerList.Text     = 'Computer List'
$grpComputerList.Location = New-Object System.Drawing.Point(6, 66)
$grpComputerList.Size     = New-Object System.Drawing.Size(200, 497)

$lstTargets = New-Object System.Windows.Forms.ListBox
$lstTargets.Location = New-Object System.Drawing.Point(6, 22)
$lstTargets.Size     = New-Object System.Drawing.Size(188, 409)
$lstTargets.Anchor   = 'Top,Bottom,Left,Right'
$lstTargets.ScrollAlwaysVisible = $true
$lstTargets.SelectionMode = 'MultiExtended'

$btnImport = New-Object System.Windows.Forms.Button
$btnImport.Text = 'Import'; $btnImport.Location = New-Object System.Drawing.Point(129, 437); $btnImport.Size = New-Object System.Drawing.Size(65, 23); $btnImport.Anchor = 'Bottom,Left'
$btnRemoveSelected = New-Object System.Windows.Forms.Button
$btnRemoveSelected.Text = 'Remove Selected'; $btnRemoveSelected.Location = New-Object System.Drawing.Point(6, 466); $btnRemoveSelected.Size = New-Object System.Drawing.Size(117, 23); $btnRemoveSelected.Anchor = 'Bottom,Left'
$btnClearAll = New-Object System.Windows.Forms.Button
$btnClearAll.Text = 'Clear All'; $btnClearAll.Location = New-Object System.Drawing.Point(129, 466); $btnClearAll.Size = New-Object System.Drawing.Size(65, 23); $btnClearAll.Anchor = 'Bottom,Left'
$btnAddTarget = New-Object System.Windows.Forms.Button
$btnAddTarget.Text = 'Add Target'; $btnAddTarget.Location = New-Object System.Drawing.Point(6, 437); $btnAddTarget.Size = New-Object System.Drawing.Size(115, 23); $btnAddTarget.Anchor = 'Bottom,Left'
[void]$grpComputerList.Controls.AddRange(@($btnAddTarget, $btnClearAll, $btnRemoveSelected, $btnImport, $lstTargets))

[void]$tabTarget.Controls.AddRange(@($grpComputerList, $grpTargetComputer))

# Monitoring group (Runtime Info)
$grpRuntime = New-Object System.Windows.Forms.GroupBox
$grpRuntime.Text     = 'Runtime Info'
$grpRuntime.Location = New-Object System.Drawing.Point(6, 6)
$grpRuntime.Size     = New-Object System.Drawing.Size(200, 148)
$lblRuntimeCaptions = New-Object System.Windows.Forms.Label
$lblRuntimeCaptions.AutoSize = $true
$lblRuntimeCaptions.Location = New-Object System.Drawing.Point(6, 19)
$lblRuntimeCaptions.Text = "OVERALL`r`nTotal Active Threads:`r`nMonitoring Threads:`r`nMemory Usage:`r`n`r`nREMOTE THREADS`r`nActive:`r`nTotal Created: "
$lblMonitoring = New-Object System.Windows.Forms.Label
$lblMonitoring.AutoSize = $true
$lblMonitoring.Location = New-Object System.Drawing.Point(129, 19)
$lblMonitoring.Text = '...'
[void]$grpRuntime.Controls.AddRange(@($lblMonitoring, $lblRuntimeCaptions))
[void]$tabMonitoring.Controls.Add($grpRuntime)

# ---- Right tab control (Package / Results) ---------------------------------
$tabControl1 = New-Object System.Windows.Forms.TabControl
$tabControl1.Location = New-Object System.Drawing.Point(224, 27)
$tabControl1.Size     = New-Object System.Drawing.Size(633, 597)
$tabControl1.Anchor   = 'Top,Bottom,Left,Right'
$tabPackage = New-Object System.Windows.Forms.TabPage('Package')
$tabResults = New-Object System.Windows.Forms.TabPage('Results')
$tabPackage.BackColor = [System.Drawing.Color]::Silver
[void]$tabControl1.TabPages.AddRange(@($tabPackage, $tabResults))

# Package group (steps 1-6)
$grpPackage = New-Object System.Windows.Forms.GroupBox
$grpPackage.Text = 'Package'; $grpPackage.Location = New-Object System.Drawing.Point(3, 6); $grpPackage.Size = New-Object System.Drawing.Size(616, 266)

$grpExt = New-Object System.Windows.Forms.GroupBox
$grpExt.Text = '1) Extension'; $grpExt.Location = New-Object System.Drawing.Point(3, 22); $grpExt.Size = New-Object System.Drawing.Size(90, 56)
$cbExtensions = New-Object System.Windows.Forms.ComboBox
$cbExtensions.Location = New-Object System.Drawing.Point(6, 22); $cbExtensions.Size = New-Object System.Drawing.Size(75, 23)
[void]$grpExt.Controls.Add($cbExtensions)

$grpParent = New-Object System.Windows.Forms.GroupBox
$grpParent.Text = '2) Parent Process'; $grpParent.Location = New-Object System.Drawing.Point(99, 22); $grpParent.Size = New-Object System.Drawing.Size(179, 56)
$cbApplications = New-Object System.Windows.Forms.ComboBox
$cbApplications.Location = New-Object System.Drawing.Point(6, 22); $cbApplications.Size = New-Object System.Drawing.Size(158, 23)
[void]$grpParent.Controls.Add($cbApplications)

$grpArgs = New-Object System.Windows.Forms.GroupBox
$grpArgs.Text = '3) Arguments'; $grpArgs.Location = New-Object System.Drawing.Point(284, 22); $grpArgs.Size = New-Object System.Drawing.Size(326, 56)
$tbArguments = New-Object System.Windows.Forms.TextBox
$tbArguments.Location = New-Object System.Drawing.Point(6, 22); $tbArguments.Size = New-Object System.Drawing.Size(314, 23)
[void]$grpArgs.Controls.Add($tbArguments)

$grpTemp = New-Object System.Windows.Forms.GroupBox
$grpTemp.Text = '4) Remote Temp Folder'; $grpTemp.Location = New-Object System.Drawing.Point(3, 84); $grpTemp.Size = New-Object System.Drawing.Size(607, 56)
$tbRemoteTempFolder = New-Object System.Windows.Forms.TextBox
$tbRemoteTempFolder.Location = New-Object System.Drawing.Point(6, 22); $tbRemoteTempFolder.Size = New-Object System.Drawing.Size(557, 23)
$btnSetTempFolder = New-Object System.Windows.Forms.Button
$btnSetTempFolder.Text = '...'; $btnSetTempFolder.Location = New-Object System.Drawing.Point(569, 22); $btnSetTempFolder.Size = New-Object System.Drawing.Size(32, 23)
[void]$grpTemp.Controls.AddRange(@($btnSetTempFolder, $tbRemoteTempFolder))

$grpPkgLoc = New-Object System.Windows.Forms.GroupBox
$grpPkgLoc.Text = '5) Package Location'; $grpPkgLoc.Location = New-Object System.Drawing.Point(3, 146); $grpPkgLoc.Size = New-Object System.Drawing.Size(607, 53)
$tbPackageLocation = New-Object System.Windows.Forms.TextBox
$tbPackageLocation.Location = New-Object System.Drawing.Point(6, 22); $tbPackageLocation.Size = New-Object System.Drawing.Size(557, 23)
$btnLocatePackage = New-Object System.Windows.Forms.Button
$btnLocatePackage.Text = '...'; $btnLocatePackage.Location = New-Object System.Drawing.Point(569, 22); $btnLocatePackage.Size = New-Object System.Drawing.Size(32, 23)
[void]$grpPkgLoc.Controls.AddRange(@($btnLocatePackage, $tbPackageLocation))

$grpCmdLine = New-Object System.Windows.Forms.GroupBox
$grpCmdLine.Text = '6) Command Line'; $grpCmdLine.Location = New-Object System.Drawing.Point(3, 205); $grpCmdLine.Size = New-Object System.Drawing.Size(607, 54)
$tbCommandLine = New-Object System.Windows.Forms.TextBox
$tbCommandLine.Location = New-Object System.Drawing.Point(6, 22); $tbCommandLine.Size = New-Object System.Drawing.Size(557, 23)
$btnRefreshCmdLine = New-Object System.Windows.Forms.Button
$btnRefreshCmdLine.Text = '...'; $btnRefreshCmdLine.Location = New-Object System.Drawing.Point(569, 21); $btnRefreshCmdLine.Size = New-Object System.Drawing.Size(32, 23)
[void]$grpCmdLine.Controls.AddRange(@($btnRefreshCmdLine, $tbCommandLine))

[void]$grpPackage.Controls.AddRange(@($grpArgs, $grpParent, $grpCmdLine, $grpExt, $grpTemp, $grpPkgLoc))

# Configs group
$grpConfigs = New-Object System.Windows.Forms.GroupBox
$grpConfigs.Text = 'Configs'; $grpConfigs.Location = New-Object System.Drawing.Point(3, 278); $grpConfigs.Size = New-Object System.Drawing.Size(616, 148)

$grpPingTimeout = New-Object System.Windows.Forms.GroupBox
$grpPingTimeout.Text = 'Ping Timeout (ms)'; $grpPingTimeout.Location = New-Object System.Drawing.Point(6, 22); $grpPingTimeout.Size = New-Object System.Drawing.Size(133, 57)
$numPingTimeout = New-Object System.Windows.Forms.NumericUpDown
$numPingTimeout.Location = New-Object System.Drawing.Point(6, 22); $numPingTimeout.Size = New-Object System.Drawing.Size(121, 23)
$numPingTimeout.Maximum = 10000; $numPingTimeout.Minimum = 0
[void]$grpPingTimeout.Controls.Add($numPingTimeout)

$grpPsExecPath = New-Object System.Windows.Forms.GroupBox
$grpPsExecPath.Text = 'PsExec64.exe Path'; $grpPsExecPath.Location = New-Object System.Drawing.Point(145, 22); $grpPsExecPath.Size = New-Object System.Drawing.Size(465, 57)
$tbPsExecLocation = New-Object System.Windows.Forms.TextBox
$tbPsExecLocation.Location = New-Object System.Drawing.Point(6, 22); $tbPsExecLocation.Size = New-Object System.Drawing.Size(415, 23); $tbPsExecLocation.Enabled = $false
$btnLocatePsExec = New-Object System.Windows.Forms.Button
$btnLocatePsExec.Text = '...'; $btnLocatePsExec.Location = New-Object System.Drawing.Point(427, 22); $btnLocatePsExec.Size = New-Object System.Drawing.Size(32, 23)
[void]$grpPsExecPath.Controls.AddRange(@($btnLocatePsExec, $tbPsExecLocation))

$grpMaxThreads = New-Object System.Windows.Forms.GroupBox
$grpMaxThreads.Text = 'Max Remote Threads'; $grpMaxThreads.Location = New-Object System.Drawing.Point(6, 85); $grpMaxThreads.Size = New-Object System.Drawing.Size(133, 56)
$numMaxThreads = New-Object System.Windows.Forms.NumericUpDown
$numMaxThreads.Location = New-Object System.Drawing.Point(6, 22); $numMaxThreads.Size = New-Object System.Drawing.Size(121, 23)
$numMaxThreads.Minimum = 1; $numMaxThreads.Maximum = 1000
[void]$grpMaxThreads.Controls.Add($numMaxThreads)

$grpInterval = New-Object System.Windows.Forms.GroupBox
$grpInterval.Text = 'Thread Interval (ms)'; $grpInterval.Location = New-Object System.Drawing.Point(145, 85); $grpInterval.Size = New-Object System.Drawing.Size(133, 56)
$numInterval = New-Object System.Windows.Forms.NumericUpDown
$numInterval.Location = New-Object System.Drawing.Point(6, 22); $numInterval.Size = New-Object System.Drawing.Size(121, 23)
$numInterval.Minimum = 500; $numInterval.Maximum = 5000
[void]$grpInterval.Controls.Add($numInterval)

$grpOptions = New-Object System.Windows.Forms.GroupBox
$grpOptions.Text = 'Options'; $grpOptions.Location = New-Object System.Drawing.Point(284, 85); $grpOptions.Size = New-Object System.Drawing.Size(326, 56)
$chkSystem = New-Object System.Windows.Forms.CheckBox
$chkSystem.Text = 'Use SYSTEM Account'; $chkSystem.AutoSize = $true; $chkSystem.Location = New-Object System.Drawing.Point(6, 17)
$chkInteractive = New-Object System.Windows.Forms.CheckBox
$chkInteractive.Text = 'Remote Interactive'; $chkInteractive.AutoSize = $true; $chkInteractive.Location = New-Object System.Drawing.Point(6, 33)
$chkFailOnWMI = New-Object System.Windows.Forms.CheckBox
$chkFailOnWMI.Text = 'Terminate on WMI Error'; $chkFailOnWMI.AutoSize = $true; $chkFailOnWMI.Location = New-Object System.Drawing.Point(150, 17)
[void]$grpOptions.Controls.AddRange(@($chkFailOnWMI, $chkInteractive, $chkSystem))

[void]$grpConfigs.Controls.AddRange(@($grpInterval, $grpOptions, $grpMaxThreads, $grpPsExecPath, $grpPingTimeout))

# Execute / Abort group
$grpExec = New-Object System.Windows.Forms.GroupBox
$grpExec.Location = New-Object System.Drawing.Point(3, 425); $grpExec.Size = New-Object System.Drawing.Size(616, 73)
$btnExecute = New-Object System.Windows.Forms.Button
$btnExecute.Text = 'Execute'; $btnExecute.Location = New-Object System.Drawing.Point(312, 15); $btnExecute.Size = New-Object System.Drawing.Size(298, 50)
$btnExecute.BackColor = [System.Drawing.Color]::Green; $btnExecute.ForeColor = [System.Drawing.SystemColors]::Control; $btnExecute.UseVisualStyleBackColor = $false
$btnAbort = New-Object System.Windows.Forms.Button
$btnAbort.Text = 'Abort'; $btnAbort.Location = New-Object System.Drawing.Point(6, 15); $btnAbort.Size = New-Object System.Drawing.Size(292, 50)
$btnAbort.BackColor = [System.Drawing.Color]::Red; $btnAbort.ForeColor = [System.Drawing.SystemColors]::Control; $btnAbort.UseVisualStyleBackColor = $false
[void]$grpExec.Controls.AddRange(@($btnExecute, $btnAbort))

[void]$tabPackage.Controls.AddRange(@($grpExec, $grpConfigs, $grpPackage))

# Results grid
$grid = New-Object System.Windows.Forms.DataGridView
$grid.Dock = 'Fill'
$grid.AllowUserToAddRows = $false
$grid.AllowUserToDeleteRows = $false
$grid.AllowUserToOrderColumns = $true
$grid.AutoSizeColumnsMode = 'AllCells'
$grid.ReadOnly = $true
$grid.RowHeadersVisible = $false
[void]$tabResults.Controls.Add($grid)

# ---- Status strip ----------------------------------------------------------
$statusStrip = New-Object System.Windows.Forms.StatusStrip
$progress = New-Object System.Windows.Forms.ToolStripProgressBar
$progress.Size = New-Object System.Drawing.Size(100, 16)
$statusLabel = New-Object System.Windows.Forms.ToolStripStatusLabel
$statusLabel.Text = $StatusReady
[void]$statusStrip.Items.AddRange(@($progress, $statusLabel))

$form.Controls.AddRange(@($tabControl2, $tabControl1, $statusStrip, $menu))

# ----------------------------------------------------------------------------
# Results DataTable (bound to grid, mutated on UI thread by the timer)
# ----------------------------------------------------------------------------
$resultTable = New-Object System.Data.DataTable
foreach ($col in 'Date','Time','RemoteHost','PingCode','ShareCode','WMICode','CMD','PID','ExitCode') {
    [void]$resultTable.Columns.Add($col, [string])
}
$grid.DataSource = $resultTable

# ----------------------------------------------------------------------------
# Helpers
# ----------------------------------------------------------------------------
function Update-ComputerListCaption {
    $grpComputerList.Text = "Computer List $($lstTargets.Items.Count)"
}

function Update-CommandLine {
    # Mirrors MakeCommandLine(): "<parent> <args with #PACKAGE -> remote temp path>"
    $leaf = if ($tbPackageLocation.Text) { Split-Path -Leaf $tbPackageLocation.Text } else { '' }
    $tmp = "$($tbRemoteTempFolder.Text)\$leaf"
    $tbCommandLine.Text = "$($cbApplications.Text) $($tbArguments.Text.Replace('#PACKAGE', $tmp))"
}

# ----------------------------------------------------------------------------
# Wire up settings into controls
# ----------------------------------------------------------------------------
$cbExtensions.Items.Clear()
foreach ($ext in ($Settings.FileExtensions -split "`r`n")) {
    if ($ext) { [void]$cbExtensions.Items.Add(($ext -split '\|')[0]) }
}
$tbRemoteTempFolder.Text = $Settings.TempLocation
$numPingTimeout.Value    = [Math]::Min([Math]::Max([int]$Settings.PingTimeout, $numPingTimeout.Minimum), $numPingTimeout.Maximum)
$numMaxThreads.Value     = [Math]::Min([Math]::Max([int]$Settings.MaxThreads, $numMaxThreads.Minimum), $numMaxThreads.Maximum)
$numInterval.Value       = [Math]::Min([Math]::Max([int]$Settings.ThreadInterval, $numInterval.Minimum), $numInterval.Maximum)
$chkSystem.Checked       = [bool]$Settings.PsExecAsSystem

# PsExec auto-detect (Resources\PsExec64.exe wins, like the original)
$bundledPsExec = Join-Path $ScriptDir 'Resources\PsExec64.exe'
if (Test-Path $bundledPsExec) { $tbPsExecLocation.Text = $bundledPsExec }
else { $tbPsExecLocation.Text = $Settings.PsExecLocation }

# Classification banner from AD domain (mirrors AppForm constructor)
$domainName = ''
try { $domainName = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties().DomainName } catch { }
$banner = $null
foreach ($d in ($Settings.Domains -split "`r`n")) {
    if (-not $d) { continue }
    $parts = $d -split '\|'
    if ($domainName.ToUpper().Contains($parts[0].ToUpper())) {
        $banner = " $Version | $($parts[1]) | $($parts[2]) | $env:USERDOMAIN | $env:USERNAME"
        break
    }
}
if (-not $banner) { $banner = " $Version | UNCLASSIFIED | PUBLIC | $env:USERDOMAIN | $env:USERNAME" }
$form.Text += $banner

Update-ComputerListCaption

# Tooltips (mirrors AppForm constructor)
$tip.SetToolTip($chkSystem, "Option for PsExec to execute in the context of the 'NT Authority\SYSTEM account on the remote system.")
$tip.SetToolTip($chkInteractive, 'Option for PsExec to execute interactively with the current logged-on user. Useful for popups.')
$tip.SetToolTip($numInterval, 'How long to wait (in milliseconds) between starting each remote thread.')
$tip.SetToolTip($numMaxThreads, 'Number of maximum remote threads.')
$tip.SetToolTip($btnImport, 'Import a list of hostnames/IP addresses from a text file (1 item per line)')
$tip.SetToolTip($btnRemoveSelected, 'Removes selected items from the Computer List.')
$tip.SetToolTip($btnClearAll, 'Clears Computer List')
$tip.SetToolTip($btnSetTempFolder, 'Set a temp folder. The same folder path will be used on the remote system.')
$tip.SetToolTip($btnLocatePackage, 'Locate a file/script to deploy to remote systems.')
$tip.SetToolTip($btnAddTarget, 'Adds target hostname/IP from Target Computer to the Computer List')
$tip.SetToolTip($btnRefreshCmdLine, '[Re]Generates commandline based on items 1-5')
$tip.SetToolTip($numPingTimeout, 'How long to wait before ping will time out.')
$tip.SetToolTip($btnAbort, 'Cancels operation and kills all running threads.')
$tip.SetToolTip($chkFailOnWMI, 'Terminates thread if WMI fails to connect.')

# ----------------------------------------------------------------------------
# Event handlers
# ----------------------------------------------------------------------------
$cbExtensions.Add_SelectedValueChanged({
    foreach ($ext in ($Settings.FileExtensions -split "`r`n")) {
        $parts = $ext -split '\|'
        if ($cbExtensions.Text -eq $parts[0]) {
            $cbApplications.Text = if ($parts.Count -gt 1) { $parts[1] } else { '' }
            $tbArguments.Text    = if ($parts.Count -gt 2) { $parts[2] } else { '' }
        }
    }
})

$btnImport.Add_Click({
    $ofd = New-Object System.Windows.Forms.OpenFileDialog
    $ofd.Filter = 'All Files (*.*)|*.*'; $ofd.CheckFileExists = $true; $ofd.CheckPathExists = $true; $ofd.Title = 'Select File for Deployment'
    if ($ofd.ShowDialog() -eq 'OK') {
        foreach ($item in ((Get-Content -Raw -LiteralPath $ofd.FileName) -split "`r`n")) {
            if ($item.Length -gt 0) { [void]$lstTargets.Items.Add($item) }
        }
    }
    Update-ComputerListCaption
})

$btnRemoveSelected.Add_Click({
    for ($i = $lstTargets.SelectedItems.Count - 1; $i -ge 0; $i--) {
        $lstTargets.Items.Remove($lstTargets.SelectedItems[$i])
    }
    Update-ComputerListCaption
})

$btnClearAll.Add_Click({ $lstTargets.Items.Clear(); Update-ComputerListCaption })

$btnAddTarget.Add_Click({
    if ($txtTargetComputer.Text) {
        [void]$lstTargets.Items.Add($txtTargetComputer.Text)
        Update-ComputerListCaption
        $txtTargetComputer.Clear()
    }
})

$btnLocatePackage.Add_Click({
    $ofd = New-Object System.Windows.Forms.OpenFileDialog
    $ofd.Filter = 'All Files (*.*)|*.*'; $ofd.CheckFileExists = $true; $ofd.CheckPathExists = $true; $ofd.Title = 'Select File for Deployment'
    if ($ofd.ShowDialog() -eq 'OK') { $tbPackageLocation.Text = $ofd.FileName }
})

$btnSetTempFolder.Add_Click({
    $fbd = New-Object System.Windows.Forms.FolderBrowserDialog
    if ($fbd.ShowDialog() -eq 'OK' -and $fbd.SelectedPath) {
        $tbRemoteTempFolder.Text = $fbd.SelectedPath
        $Settings.TempLocation = $fbd.SelectedPath
        Save-Settings $Settings
    }
})

$btnLocatePsExec.Add_Click({
    $ofd = New-Object System.Windows.Forms.OpenFileDialog
    $ofd.Filter = 'All Files (*.exe)|*.exe'; $ofd.CheckFileExists = $true; $ofd.CheckPathExists = $true; $ofd.Title = 'PsExec Location'
    if ($ofd.ShowDialog() -eq 'OK') {
        $tbPsExecLocation.Text = $ofd.FileName
        $Settings.PsExecLocation = $ofd.FileName
        Save-Settings $Settings
    }
})

$tbPackageLocation.Add_TextChanged({ Update-CommandLine })
$btnRefreshCmdLine.Add_Click({ Update-CommandLine })

$numPingTimeout.Add_ValueChanged({ $Settings.PingTimeout = [int]$numPingTimeout.Value; Save-Settings $Settings })
$numMaxThreads.Add_ValueChanged({ $Settings.MaxThreads = [int]$numMaxThreads.Value; Save-Settings $Settings })
$numInterval.Add_ValueChanged({ $Settings.ThreadInterval = [int]$numInterval.Value; Save-Settings $Settings })
$chkSystem.Add_CheckedChanged({ $Settings.PsExecAsSystem = [bool]$chkSystem.Checked; Save-Settings $Settings })
$chkFailOnWMI.Add_CheckedChanged({ $App.AbortOnWMIFail = [bool]$chkFailOnWMI.Checked })

$miArgs.Add_Click({
    [System.Windows.Forms.MessageBox]::Show(
@"
Examples of arguments both Prepended and Appended

1)
/I #PACKAGE /quiet /norestart

2)
#PACKAGE /quiet /norestart

3)
Formatting the Argument Box

#PACKAGE is a placeholder and must be present in the Arguments Box as shown above.
"@, 'How-To Arguments', 'OK', 'Information') | Out-Null
})

$miAbout.Add_Click({
    [System.Windows.Forms.MessageBox]::Show("$AuthorInfo`r`nVersion: $Version", 'About', 'OK', 'Information') | Out-Null
})

$miExport.Add_Click({
    $sfd = New-Object System.Windows.Forms.SaveFileDialog
    $sfd.Filter = 'Comma-Separated Files (*.csv)|*.csv|All Files (*.*)|*.*'
    if ($sfd.ShowDialog() -eq 'OK') {
        $rows = foreach ($r in $resultTable.Rows) {
            [pscustomobject][ordered]@{
                Date=$r.Date; Time=$r.Time; RemoteHost=$r.RemoteHost; PingCode=$r.PingCode
                ShareCode=$r.ShareCode; WMICode=$r.WMICode; CMD=$r.CMD; PID=$r.PID; ExitCode=$r.ExitCode
            }
        }
        $rows | Export-Csv -LiteralPath $sfd.FileName -NoTypeInformation -Encoding UTF8
    }
})

# ----------------------------------------------------------------------------
# UI refresh timer (replaces the ContinuousDoEvents monitoring thread)
# ----------------------------------------------------------------------------
$uiTimer = New-Object System.Windows.Forms.Timer
$uiTimer.Interval = 250
$uiTimer.Add_Tick({
    # Sync worker results into the bound DataTable.
    $snapshot = @($App.Results.ToArray())
    if ($snapshot.Count -ne $resultTable.Rows.Count) {
        for ($i = $resultTable.Rows.Count; $i -lt $snapshot.Count; $i++) {
            [void]$resultTable.Rows.Add(($resultTable.NewRow()))
        }
    }
    for ($i = 0; $i -lt $snapshot.Count; $i++) {
        $src = $snapshot[$i]; $dst = $resultTable.Rows[$i]
        foreach ($c in 'Date','Time','RemoteHost','PingCode','ShareCode','WMICode','CMD','PID','ExitCode') {
            $v = [string]$src[$c]
            if ($dst[$c] -ne $v) { $dst[$c] = $v }
        }
    }

    # Runtime Info panel.
    $memMB = [int]([System.Diagnostics.Process]::GetCurrentProcess().PrivateMemorySize64 / 1MB)
    $active = [int]$App.RemoteActive
    $lblMonitoring.Text = "`r`n$($active + 1)`r`n1`r`n$memMB MB`r`n`r`n`r`n$active`r`n$($App.TotalThreadsCreated)"

    if (-not $App.Done) {
        $total = $lstTargets.Items.Count
        $created = [int]$App.TotalThreadsCreated
        $statusLabel.Text = "Running... $created of $total launched, $active active"
        if ($total -gt 0) {
            $progress.Maximum = $total
            $progress.Value   = [Math]::Min($created, $total)
        }
    } elseif ($progress.Value -ne 0 -or $statusLabel.Text -ne $StatusReady) {
        # Deployment just finished -> reset UI, re-enable Execute.
        $progress.Value = 0
        $statusLabel.Text = $StatusReady
        $btnExecute.Enabled = $true
    }
})
$uiTimer.Start()

# ----------------------------------------------------------------------------
# Execute
# ----------------------------------------------------------------------------
$script:coordPS = $null
$btnExecute.Add_Click({
    if ($tbPsExecLocation.Text.ToUpper() -notlike '*PSEXEC*') {
        [System.Windows.Forms.MessageBox]::Show("You are missing the PsExec File (required). Configure the location for PsExec in the 'Config' section.", 'PsExec Missing', 'OK', 'Information') | Out-Null
        return
    }
    if ([string]::IsNullOrEmpty($tbCommandLine.Text)) { return }
    if ($lstTargets.Items.Count -eq 0 -and [string]::IsNullOrEmpty($txtTargetComputer.Text)) { return }
    if ($lstTargets.Items.Count -eq 0 -and $txtTargetComputer.Text) { [void]$lstTargets.Items.Add($txtTargetComputer.Text) }

    $confirm = [System.Windows.Forms.MessageBox]::Show(
        "Are you sure you want to execute`r`n`r`n$($tbCommandLine.Text)`r`n`r`non`r`n`r`n$($lstTargets.Items.Count) remote computers?`r`n`r`nYes/No",
        'Verify', 'YesNo', 'Information')
    if ($confirm -ne 'Yes') { return }

    # Reset shared state.
    $App.Results.Clear()
    $App.Abort = $false
    $App.TotalThreadsCreated = 0
    $App.RemoteActive = 0
    $App.Done = $false
    $App.AbortOnWMIFail = [bool]$chkFailOnWMI.Checked

    $resultTable.Rows.Clear()
    $tabControl1.SelectedTab = $tabResults
    $tabControl2.SelectedTab = $tabMonitoring
    $btnExecute.Enabled = $false
    $progress.Style = 'Continuous'

    $cfg = @{
        CmdLine     = $tbCommandLine.Text
        TempFolder  = $tbRemoteTempFolder.Text
        PackagePath = $tbPackageLocation.Text
        PsExecPath  = $tbPsExecLocation.Text
        AsSystem    = [bool]$chkSystem.Checked
        Interactive = [bool]$chkInteractive.Checked
        PingTimeout = [int]$numPingTimeout.Value
        MaxThreads  = [int]$numMaxThreads.Value
        ThreadInterval = [int]$numInterval.Value
    }
    $hosts = @($lstTargets.Items | ForEach-Object { [string]$_ })

    # Launch the coordinator in its own runspace so the UI stays responsive.
    $script:coordPS = [powershell]::Create()
    [void]$script:coordPS.AddScript($CoordinatorScript).AddArgument($App).AddArgument($hosts).AddArgument($cfg).AddArgument($WorkerScript)
    [void]$script:coordPS.BeginInvoke()
})

$btnAbort.Add_Click({
    $result = [System.Windows.Forms.MessageBox]::Show('Abort all threads? Doing so will restart the application.', 'Confirmation', 'OKCancel', 'Information')
    if ($result -eq 'OK') {
        $App.Abort = $true
        $App.Done = $true
        # Mirror the original's "restart the application" behavior.
        $exe = (Get-Process -Id $PID).Path
        Start-Process -FilePath $exe -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-Sta','-File',"`"$PSCommandPath`"")
        $form.Close()
    }
})

$form.Add_FormClosing({
    $App.Abort = $true
    $uiTimer.Stop()
    if ($script:coordPS) { try { $script:coordPS.Dispose() } catch { } }
})

# ----------------------------------------------------------------------------
# Run
# ----------------------------------------------------------------------------
[void]$form.ShowDialog()
$form.Dispose()
