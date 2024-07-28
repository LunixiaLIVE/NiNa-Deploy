using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Management;
using System.Windows.Forms;
using NiNa_Deploy.Properties;
using System.Globalization;
using System.Security.Cryptography;


namespace NiNa_Deploy
{
    public partial class AppForm : Form
    {
        public AppForm()
        {
            InitializeComponent();
            toolStripStatusLabel_1.Text = Utility.listStatus[0];
            if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName.ToUpper().Contains("USAF.MIL"))
            {
                this.Text += $" {Utility.strVersion} | UNCLASSIFIED/CUI | NIPRNET | " + Environment.UserDomainName + " | " + Environment.UserName;
            }
            else if (Environment.UserDomainName.ToUpper().Contains("SMIL.MIL"))
            {
                this.Text += $" {Utility.strVersion} | CLASSIFIED/SECRET | SIPRNET | " + Environment.UserDomainName + " | " + Environment.UserName;
            }
            else
            {
                this.Text += $" {Utility.strVersion} | UNCLASSIFIED | PUBLIC | " + Environment.UserDomainName + " | " + Environment.UserName;
            }
            cb_FileExtensions.Items.Clear();
            foreach (String ext in Properties.Settings.Default.FileExtensions)
            {
                cb_FileExtensions.Items.Add(ext.Split("|")[0]);
            }
            tb_RemoteTempFolder.Text = Properties.Settings.Default.TempLocation;
            lb_TotalComputers.Text = String.Empty;
            toolStripStatusLabel_1.Text = Utility.listStatus[0];
            num_PingTimeout.Value = Properties.Settings.Default.PingTimeout;
            tb_PsExecLocation.Text = Properties.Settings.Default.PsExecLocation;
            num_MaxThreads.Value = Properties.Settings.Default.MaxPsExecProcs;
            num_TimeBetweenThreads.Value = Properties.Settings.Default.PsExecInterval;
            chk_PsExec_System.Checked = Properties.Settings.Default.PsExecAsSystem;

            lbl_MonitoringThreads.Text = Utility.MonitoringThreadList.Count.ToString();

            ToolTip tip = new() { ShowAlways = true };

            tip.SetToolTip(this.chk_PsExec_System, "Option for PsExec to execute in the context of the 'NT Authority\\SYSTEM account on the remote system.");
            tip.SetToolTip(this.chk_PsExecRemoteInteractive, "Option for PsExec to execute interactively with the current logged-on user. Useful for popups.");
            
            tip.SetToolTip(this.num_TimeBetweenThreads, "How long to wait (in milliseconds) between starting each remote thread.");
            tip.SetToolTip(this.num_MaxThreads, "Number of maximum remote threads.");

            Thread t = new(ContinuousDoEvents) { Name = "ContinuousDoEvents" };
            t.Start();
            Utility.MonitoringThreadList.Add(t);
            Application.DoEvents();
        }

 

        void ContinuousDoEvents()
        {
            while (true)
            {
                Thread.Sleep(100);
                lbl_MonitoringThreads.Invoke(UpdateLabel_MonitoringThreads);
            }
        }

        private void UpdateLabel_MonitoringThreads()
        {
            
            lbl_MonitoringThreads.Text = $"\r\n" +
                                         $"{Utility.RemoteThreadList.Count + Utility.MonitoringThreadList.Count}\r\n" +
                                         $"{Utility.MonitoringThreadList.Count}\r\n" +
                                         $"{(Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024)} MB\r\n" +
                                         $"\r\n" +
                                         $"\r\n" +
                                         $"{Utility.RemoteThreadList.Count}\r\n" +
                                         $"{Utility.TotalThreadsCreated}";
            lbl_MonitoringThreads.Update();
            GC.Collect();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btn_ImportList_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "All Files (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Select File for Deployment"
            };
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                foreach (String item in File.ReadAllText(ofd.FileName).ToString().Split("\r\n").ToArray())
                {
                    if(item.Length > 0)
                    {
                        list_TargetComputers.Items.Add(item);
                    }
                }
                Application.DoEvents();
            }
            lb_TotalComputers.Text = list_TargetComputers.Items.Count.ToString();
            Application.DoEvents();
        }

        private void btn_RemoveSelected_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = list_TargetComputers.SelectedItems;
            if (list_TargetComputers.SelectedIndex != 1 && list_TargetComputers.Items.Count > 0)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    list_TargetComputers.Items.Remove(selectedItems[i]);
                }
            }
            lb_TotalComputers.Text = list_TargetComputers.Items.Count.ToString();
            Application.DoEvents();
        }

        private void btn_ClearAll_Click(object sender, EventArgs e)
        {
            list_TargetComputers.Items.Clear();
            lb_TotalComputers.Text = "0";
            Application.DoEvents();
        }

        private void btn_LocatePackage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Title = "Select File for Deployment";
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                tb_PackageLocation.Text = ofd.FileName;
            }
        }

        private void cb_FileExtensions_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (String ext in Properties.Settings.Default.FileExtensions)
            {
                if (cb_FileExtensions.Text == ext.Split("|")[0])
                {
                    cb_Applications.Text = ext.Split("|")[1];
                    tb_Arguments.Text = ext.Split("|")[2];
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new();
            foreach (String s in File.ReadAllLines($@"{Directory.GetCurrentDirectory()}\Resources\AuthorInfo.txt"))
            {
                sb.Append($"{s}\r\n");
            }
            MessageBox.Show($"{sb}" +
                            $"Version: {Utility.strVersion}", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoThreadThings(String strComputer)
        {
            Console.WriteLine($"Remote Thread started for: {strComputer}");

            Utility.RemoteProcessesResults.Rows.Add(
                DateTime.Now.ToLongDateString(),
                DateTime.Now.ToLongTimeString(),
                strComputer,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty
            );

            Thread.Sleep(500);

            Console.WriteLine($"Diagnostic Ping: {strComputer}");
            ///
            ///DIAGNOSTIC - PING
            ///
            String ErrorCode = String.Empty;
            Ping ping = new();
            PingOptions pingOptions = new(64, true);
            byte[] buffer = Encoding.ASCII.GetBytes("00000000000000000000000000000000");

            try 
            { 
                PingReply pingReply = ping.Send(strComputer, (int)Math.Floor(num_PingTimeout.Value), buffer, pingOptions);
                ErrorCode = pingReply.Status.ToString();
            }
            catch(PingException e)
            {
                ErrorCode = e.Message.ToString();
            }

            for(int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    if (ErrorCode != "Success")
                    {
                        Utility.RemoteProcessesResults.Rows[i]["PingCode"] = ErrorCode;
                        Utility.RemoteProcessesResults.Rows[i]["ShareCode"] = "Fail:Ping";
                        Utility.RemoteProcessesResults.Rows[i]["WMICode"] = "Fail:Ping";
                        Utility.RemoteProcessesResults.Rows[i]["CMD"] = "Fail:Ping";
                        Utility.RemoteProcessesResults.Rows[i]["PID"] = "Fail:Ping";
                        Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = "Fail:Ping";
                        return;
                    }
                    Utility.RemoteProcessesResults.Rows[i]["PingCode"] = ErrorCode;
                }
            }
            Thread.Sleep(500);

            Console.WriteLine($"Diagnostic CShare: {strComputer}");
            ///
            ///DIAGNOSTIC - CSHARE
            ///
            String ShareCode = String.Empty;
            if (Directory.Exists($"\\\\{strComputer}\\C$"))
            {
                ShareCode = "Success";
            }
            for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    if (ShareCode != "Success")
                    {
                        Utility.RemoteProcessesResults.Rows[i]["ShareCode"] = "Fail:Share";
                        Utility.RemoteProcessesResults.Rows[i]["WMICode"] = "Fail:Share";
                        Utility.RemoteProcessesResults.Rows[i]["CMD"] = "Fail:Share";
                        Utility.RemoteProcessesResults.Rows[i]["PID"] = "Fail:Share";
                        Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = "Fail:Share";
                        return;
                    }
                    Utility.RemoteProcessesResults.Rows[i]["ShareCode"] = "Success";
                }
            }
            Thread.Sleep(500);

            Console.WriteLine($"Diagnostic WMI: {strComputer}");
            ///
            ///DIAGNOSTIC - WMI
            ///
            WMI WMIConnect = new();
            String wmi = WMIConnect.TestConnection(strComputer);
            WMIConnect.Dispose();
            for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    if (wmi != "Success")
                    {
                        Utility.RemoteProcessesResults.Rows[i]["WMICode"] = wmi;
                        Utility.RemoteProcessesResults.Rows[i]["CMD"] = "Fail:WMI";
                        Utility.RemoteProcessesResults.Rows[i]["PID"] = "Fail:WMI";
                        Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = "Fail:WMI";
                        return;
                    }
                    Utility.RemoteProcessesResults.Rows[i]["WMICode"] = "Success";
                }
            }
            Thread.Sleep(500);

            ///
            ///PREP - COMMAND
            ///
            for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    Utility.RemoteProcessesResults.Rows[i]["CMD"] = tb_CommandLine.Text;
                }
            }

            Console.WriteLine($"File Prep: {strComputer}");
            ///
            ///PREP - FILE PREP
            ///
            String rD = $@"\\{strComputer}\{tb_RemoteTempFolder.Text.Replace("C:", "C$")}";
            String fN = tb_PackageLocation.Text.Split(@"\").Last();
            if (!Directory.Exists(rD))
            {
                try
                {
                    Directory.CreateDirectory(rD);
                }
                catch (Exception e)
                {
                    for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
                    {
                        if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                        {
                            Utility.RemoteProcessesResults.Rows[i]["PID"] = "Fail:RemoteDirectoryCreation";
                            Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = e.Message.ToString();
                        }
                    }
                    return;
                }
            }
            if (!File.Exists($@"{rD}\{fN}"))
            {
                try
                {
                    File.Copy(tb_PackageLocation.Text, $@"{rD}\{fN}", true);
                }
                catch (Exception e)
                {
                    for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
                    {
                        if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                        {
                            Utility.RemoteProcessesResults.Rows[i]["PID"] = "Fail:RemoteFileCopy";
                            Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = e.Message.ToString();
                        }
                    }
                    return;
                }
            }

            Console.WriteLine($"Execution of Remote Process: {strComputer}");
            ///
            ///EXECUTE - PROCESS
            ///
            String Arguments = $@"\\{strComputer} -accepteula -s -i {tb_CommandLine.Text}";
            if (!chk_PsExec_System.Checked) { Arguments = Arguments.Replace(" -s", ""); }
            if (!chk_PsExecRemoteInteractive.Checked) { Arguments = Arguments.Replace(" -i", ""); }

            Process p = new();
            ProcessStartInfo pinfo = new()
            {
                FileName = $@"{tb_PsExecLocation.Text}",
                Arguments = Arguments,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };
            p.StartInfo = pinfo;
            p.Start();

            Thread.Sleep(1000);

            Console.WriteLine($"Waiting for prcocess to return result: {strComputer}");
            for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    Utility.RemoteProcessesResults.Rows[i]["PID"] = p.Id.ToString();
                }
            }
            while (!p.HasExited)
            {
                Thread.Sleep(500);
            }
            for (int i = 0; i < Utility.RemoteProcessesResults.Rows.Count; i++)
            {
                if (Utility.RemoteProcessesResults.Rows[i]["RemoteHost"].ToString() == strComputer)
                {
                    Utility.RemoteProcessesResults.Rows[i]["ExitCode"] = p.ExitCode.ToString();
                }
            }
            Console.WriteLine($"Thread Exiting: {strComputer}");

            ///
            ///CLEANUP REMOTE FILES
            ///
            if (File.Exists($@"{rD}\{fN}"))
            {
                try
                {
                    File.Delete($@"{rD}\{fN}");
                }
                catch
                {
                }
            }
            return;
        }

        private void btn_Execute_Click(object sender, EventArgs e)
        {
            if (!tb_PsExecLocation.Text.ToUpper().Contains("PSEXEC"))
            {
                MessageBox.Show("You are missing the PsExec File (required). Configure the location for PsExec in the 'Config' section.", "PsExec Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if(tb_CommandLine.Text == String.Empty || tb_CommandLine.Text == null || tb_CommandLine.Text.Length == 0)
            {
                return;
            }

            if (list_TargetComputers.Items.Count == 0 && txt_TargetComputer.Text == String.Empty)
            {
                return;
            }

            if (list_TargetComputers.Items.Count == 0 && txt_TargetComputer.Text != String.Empty)
            {
                list_TargetComputers.Items.Add(txt_TargetComputer.Text);
            }

            lb_TotalComputers.Text = list_TargetComputers.Items.Count.ToString();

            DialogResult dr = MessageBox.Show($"Are you sure you want to execute\r\n\r\n" +
                                              $"{tb_CommandLine.Text}\r\n\r\n" +
                                              $"on\r\n\r\n" +
                                              $"{list_TargetComputers.Items.Count} remote computers?\r\n\r\n" +
                                              $"Yes/No", "Verify", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dr == DialogResult.No) { return; }

            tabControl1.SelectTab(1);
            Application.DoEvents();

            Utility.RemoteProcessesResults = Utility.GenerateRemoteProcessesTable();
            Utility.SleepTimeBetweenThreads = (int)num_TimeBetweenThreads.Value;
            Utility.RemoteThreadList.Clear();
            Utility.MaxThreads = (int)num_MaxThreads.Value;
            Utility.TotalThreadsCreated = 0;
            dataGridView_Table.DataSource = Utility.RemoteProcessesResults;

            toolStripProgressBar_1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar_1.Enabled = true;
            toolStripProgressBar_1.Visible = true;
            toolStripProgressBar_1.Maximum = list_TargetComputers.Items.Count;
            toolStripProgressBar_1.Minimum = 0;

            foreach (String strComputer in list_TargetComputers.Items)
            {
                while (Utility.RemoteThreadList.Count >= Utility.MaxThreads)
                {
                    Utility.RemoteThreadList.RemoveAll(t => t.IsAlive == false);
                    Thread.Sleep(100);
                    Application.DoEvents();
                }
                Thread thread = new(() => DoThreadThings(strComputer));
                thread.Start();
                Utility.RemoteThreadList.Add(thread);
                Utility.TotalThreadsCreated++;
                Thread.Sleep(Utility.SleepTimeBetweenThreads);

                toolStripStatusLabel_1.Text = $"Creating Thread for {strComputer} : {Utility.TotalThreadsCreated} of {list_TargetComputers.Items.Count}";
                toolStripProgressBar_1.Value = Utility.TotalThreadsCreated;

                Application.DoEvents();
            }
            Application.DoEvents();
            toolStripProgressBar_1.Style = ProgressBarStyle.Marquee;
            toolStripProgressBar_1.MarqueeAnimationSpeed = 100;
            toolStripStatusLabel_1.Text = $"Waiting for remaining threads to complete.";
            Application.DoEvents();
            while (Utility.RemoteThreadList.Count > 0)
            {
                Utility.RemoteThreadList.RemoveAll(t => t.IsAlive == false);
                Thread.Sleep(100);
                Application.DoEvents();
            }
            toolStripProgressBar_1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar_1.Value = 0;
            Application.DoEvents();
            toolStripStatusLabel_1.Text = $"{Utility.listStatus[0]}";
        }

        private void argumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"Examples of arguments both Prepended and Appended\r\n\r\n" +

                $"1)\r\n" +
                $"/I #PACKAGE /quiet /norestart\r\n\r\n" +

                $"2){Environment.NewLine}" +
                $"#PACKAGE /quiet /norestart\r\n\r\n" +

                $"3)\r\n" +
                $"Formatting the Argument Box\r\n\r\n" +

                $"#PACKAGE is a placeholder and must be present in the Arguments Box as shown above.",

                "How-To Arugments", MessageBoxButtons.OK, MessageBoxIcon.Information
            );
            GC.Collect();
        }

        private void tb_PackageLocation_TextChanged(object sender, EventArgs e)
        {
            MakeCommandLine();
        }

        private void btn_RefeshCommandLine_Click(object sender, EventArgs e)
        {
            MakeCommandLine();
        }

        private void MakeCommandLine()
        {
            String tmp = $"{tb_RemoteTempFolder.Text}\\{tb_PackageLocation.Text.Split('\\').Last()}";
            tb_CommandLine.Text = $"{cb_Applications.Text} {tb_Arguments.Text.Replace("#PACKAGE", tmp)}";
            Application.DoEvents();
            GC.Collect();
        }

        private void num_PingTimeout_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PingTimeout = (int)num_PingTimeout.Value;
            Properties.Settings.Default.Save();
            GC.Collect();
        }

        private void btn_LocatePsExec_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files (*.exe)|*.exe";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Title = "PsExec Location";
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                tb_PsExecLocation.Text = ofd.FileName;
                Properties.Settings.Default.PsExecLocation = ofd.FileName;
                Properties.Settings.Default.Save();
            }
            GC.Collect();
        }

        private void num_MaxPsExecProcs_ValueChanged(object sender, EventArgs e)
        {
            Utility.MaxThreads = (int)num_MaxThreads.Value;
            Properties.Settings.Default.MaxPsExecProcs = (int)num_MaxThreads.Value;
            Properties.Settings.Default.Save();
            GC.Collect();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma-Separated Files (*.csv)|*.csv| All Files (*.*)|*.*";
            sfd.FilterIndex = 0;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                dataGridView_Table.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                dataGridView_Table.SelectAll();
                DataObject dataObject = dataGridView_Table.GetClipboardContent();
                File.WriteAllText(sfd.FileName, dataObject.GetText(TextDataFormat.CommaSeparatedValue));
            }
            GC.Collect();
        }

        private void num_TimeBetweenProcs_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PsExecInterval = (int)num_TimeBetweenThreads.Value;
            Properties.Settings.Default.Save();
            GC.Collect();
        }

        private void chk_PsExec_System_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PsExecAsSystem = chk_PsExec_System.Checked;
            Properties.Settings.Default.Save();
            GC.Collect();
        }

        private void AppForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
