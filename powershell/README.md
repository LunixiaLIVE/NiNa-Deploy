# NiNa Deploy — PowerShell port

A single-file PowerShell/WinForms rewrite of the C# **NiNa Deploy** (Ninja Nation - Deploy)
remote-deployment tool. Same UI, same workflow, no compiler required.

## What it does

Deploys a package/script to a list of remote Windows computers. For each target it runs:

1. **Ping** (ICMP, TTL 64, don't-fragment)
2. **C$ admin-share** reachability check
3. **WMI** connectivity test (`root\cimv2` via `ManagementScope`)
4. **Copy** the package to the remote temp folder over the admin share
5. **Execute** it remotely with **PsExec** (`-accepteula`, optional `-s` SYSTEM / `-i` interactive)
6. **Capture** the exit code, then **delete** the copied file

Work runs concurrently, bounded by *Max Remote Threads*, with a configurable delay
between launches. Results populate the **Results** grid and can be exported to CSV
from **Data → Export**.

## Requirements

- Windows PowerShell 5.1 (or PowerShell 7 on Windows). The script re-launches itself
  in STA mode automatically if needed.
- **PsExec64.exe** (Sysinternals) — not redistributed here. Either:
  - drop it at `.\Resources\PsExec64.exe` (auto-detected, next to the script), or
  - point at it via the **PsExec64.exe Path** `...` button in the Config section.
- Rights to reach the targets' `C$` share and WMI (typically a local admin on the targets).

## Run

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\NiNa-Deploy.ps1
```

Or right-click → **Run with PowerShell**.

## Notes on the port

- **Threading** — the C# raw-`Thread` model is replaced by a background *coordinator*
  runspace that dispatches per-host workers through a `RunspacePool` sized to
  *Max Remote Threads*. A `System.Windows.Forms.Timer` refreshes the grid and the
  Runtime Info panel on the UI thread (replacing the `ContinuousDoEvents` loop).
- **Settings** — .NET user settings / `App.config` are replaced by
  `%APPDATA%\NiNa-Deploy\settings.json`, seeded with the original defaults
  (file-extension → parent-process → argument map, temp location, ping timeout,
  max threads, thread interval, SYSTEM flag, classification domains).
- **Classification banner** — the title bar is set from the AD domain exactly as the
  original (`USAF.MIL → UNCLASSIFIED/CUI | NIPRNET`, etc.), falling back to
  `UNCLASSIFIED | PUBLIC`.
- **Abort** restarts the application, matching the original's behavior.
- The `#PACKAGE` placeholder in the Arguments box is substituted with the remote
  temp path when the command line is generated (button `...` in step 6, or on
  package-location change).
