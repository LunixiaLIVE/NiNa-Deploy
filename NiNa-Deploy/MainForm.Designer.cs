namespace NiNa_Deploy
{
    partial class AppForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            menuMain = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            dataToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            argumentsToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            groupBox2 = new GroupBox();
            btn_ClearAll = new Button();
            btn_RemoveSelected = new Button();
            btn_ImportList = new Button();
            lb_TotalComputers = new Label();
            list_TargetComputers = new ListBox();
            btn_Execute = new Button();
            groupBox1 = new GroupBox();
            txt_TargetComputer = new TextBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox12 = new GroupBox();
            groupBox17 = new GroupBox();
            num_TimeBetweenThreads = new NumericUpDown();
            groupBox16 = new GroupBox();
            chk_PsExecRemoteInteractive = new CheckBox();
            chk_PsExec_System = new CheckBox();
            groupBox15 = new GroupBox();
            num_MaxThreads = new NumericUpDown();
            groupBox14 = new GroupBox();
            btn_LocatePsExec = new Button();
            tb_PsExecLocation = new TextBox();
            groupBox13 = new GroupBox();
            num_PingTimeout = new NumericUpDown();
            groupBox4 = new GroupBox();
            groupBox6 = new GroupBox();
            btn_RefeshCommandLine = new Button();
            tb_CommandLine = new TextBox();
            groupBox10 = new GroupBox();
            tb_RemoteTempFolder = new TextBox();
            groupBox7 = new GroupBox();
            tb_Arguments = new TextBox();
            groupBox8 = new GroupBox();
            groupBox11 = new GroupBox();
            cb_Applications = new ComboBox();
            groupBox3 = new GroupBox();
            cb_FileExtensions = new ComboBox();
            groupBox5 = new GroupBox();
            btn_LocatePackage = new Button();
            tb_PackageLocation = new TextBox();
            tabPage2 = new TabPage();
            dataGridView_Table = new DataGridView();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar_1 = new ToolStripProgressBar();
            toolStripStatusLabel_1 = new ToolStripStatusLabel();
            toolStripStatusLabel_threads = new ToolStripStatusLabel();
            groupBox9 = new GroupBox();
            groupBox18 = new GroupBox();
            lbl_MonitoringThreads = new Label();
            label1 = new Label();
            menuMain.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox12.SuspendLayout();
            groupBox17.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_TimeBetweenThreads).BeginInit();
            groupBox16.SuspendLayout();
            groupBox15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_MaxThreads).BeginInit();
            groupBox14.SuspendLayout();
            groupBox13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_PingTimeout).BeginInit();
            groupBox4.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox10.SuspendLayout();
            groupBox7.SuspendLayout();
            groupBox8.SuspendLayout();
            groupBox11.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox5.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_Table).BeginInit();
            statusStrip1.SuspendLayout();
            groupBox9.SuspendLayout();
            groupBox18.SuspendLayout();
            SuspendLayout();
            // 
            // menuMain
            // 
            menuMain.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, dataToolStripMenuItem, helpToolStripMenuItem });
            menuMain.Location = new Point(0, 0);
            menuMain.Name = "menuMain";
            menuMain.Size = new Size(866, 24);
            menuMain.TabIndex = 0;
            menuMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(93, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // dataToolStripMenuItem
            // 
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(43, 20);
            dataToolStripMenuItem.Text = "Data";
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(108, 22);
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { argumentsToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // argumentsToolStripMenuItem
            // 
            argumentsToolStripMenuItem.Name = "argumentsToolStripMenuItem";
            argumentsToolStripMenuItem.Size = new Size(133, 22);
            argumentsToolStripMenuItem.Text = "Arguments";
            argumentsToolStripMenuItem.Click += argumentsToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(133, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btn_ClearAll);
            groupBox2.Controls.Add(btn_RemoveSelected);
            groupBox2.Controls.Add(btn_ImportList);
            groupBox2.Controls.Add(lb_TotalComputers);
            groupBox2.Controls.Add(list_TargetComputers);
            groupBox2.Location = new Point(6, 71);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 365);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Computer List";
            // 
            // btn_ClearAll
            // 
            btn_ClearAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_ClearAll.Location = new Point(129, 334);
            btn_ClearAll.Name = "btn_ClearAll";
            btn_ClearAll.Size = new Size(65, 23);
            btn_ClearAll.TabIndex = 5;
            btn_ClearAll.Text = "Clear All";
            btn_ClearAll.UseVisualStyleBackColor = true;
            btn_ClearAll.Click += btn_ClearAll_Click;
            // 
            // btn_RemoveSelected
            // 
            btn_RemoveSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_RemoveSelected.Location = new Point(6, 334);
            btn_RemoveSelected.Name = "btn_RemoveSelected";
            btn_RemoveSelected.Size = new Size(117, 23);
            btn_RemoveSelected.TabIndex = 4;
            btn_RemoveSelected.Text = "Remove Selected";
            btn_RemoveSelected.UseVisualStyleBackColor = true;
            btn_RemoveSelected.Click += btn_RemoveSelected_Click;
            // 
            // btn_ImportList
            // 
            btn_ImportList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_ImportList.Location = new Point(6, 305);
            btn_ImportList.Name = "btn_ImportList";
            btn_ImportList.Size = new Size(188, 23);
            btn_ImportList.TabIndex = 3;
            btn_ImportList.Text = "Import List";
            btn_ImportList.UseVisualStyleBackColor = true;
            btn_ImportList.Click += btn_ImportList_Click;
            // 
            // lb_TotalComputers
            // 
            lb_TotalComputers.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lb_TotalComputers.AutoSize = true;
            lb_TotalComputers.Location = new Point(6, 283);
            lb_TotalComputers.Name = "lb_TotalComputers";
            lb_TotalComputers.Size = new Size(38, 15);
            lb_TotalComputers.TabIndex = 2;
            lb_TotalComputers.Text = "label1";
            // 
            // list_TargetComputers
            // 
            list_TargetComputers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            list_TargetComputers.FormattingEnabled = true;
            list_TargetComputers.ItemHeight = 15;
            list_TargetComputers.Location = new Point(6, 22);
            list_TargetComputers.Name = "list_TargetComputers";
            list_TargetComputers.SelectionMode = SelectionMode.MultiExtended;
            list_TargetComputers.Size = new Size(188, 259);
            list_TargetComputers.TabIndex = 1;
            // 
            // btn_Execute
            // 
            btn_Execute.BackColor = Color.Green;
            btn_Execute.ForeColor = SystemColors.Control;
            btn_Execute.Location = new Point(525, 153);
            btn_Execute.Name = "btn_Execute";
            btn_Execute.Size = new Size(85, 51);
            btn_Execute.TabIndex = 6;
            btn_Execute.Text = "Execute";
            btn_Execute.UseVisualStyleBackColor = false;
            btn_Execute.Click += btn_Execute_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txt_TargetComputer);
            groupBox1.ForeColor = SystemColors.ControlText;
            groupBox1.Location = new Point(6, 11);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(200, 54);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target Computer";
            // 
            // txt_TargetComputer
            // 
            txt_TargetComputer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_TargetComputer.Location = new Point(6, 22);
            txt_TargetComputer.Name = "txt_TargetComputer";
            txt_TargetComputer.Size = new Size(188, 23);
            txt_TargetComputer.TabIndex = 0;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(224, 27);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(631, 661);
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.Silver;
            tabPage1.Controls.Add(groupBox12);
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(623, 633);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Package";
            // 
            // groupBox12
            // 
            groupBox12.Controls.Add(btn_Execute);
            groupBox12.Controls.Add(groupBox17);
            groupBox12.Controls.Add(groupBox16);
            groupBox12.Controls.Add(groupBox15);
            groupBox12.Controls.Add(groupBox14);
            groupBox12.Controls.Add(groupBox13);
            groupBox12.Location = new Point(3, 418);
            groupBox12.Name = "groupBox12";
            groupBox12.Size = new Size(616, 211);
            groupBox12.TabIndex = 2;
            groupBox12.TabStop = false;
            groupBox12.Text = "Configs";
            // 
            // groupBox17
            // 
            groupBox17.Controls.Add(num_TimeBetweenThreads);
            groupBox17.Location = new Point(6, 147);
            groupBox17.Name = "groupBox17";
            groupBox17.Size = new Size(133, 56);
            groupBox17.TabIndex = 3;
            groupBox17.TabStop = false;
            groupBox17.Text = "Thread Interval";
            // 
            // num_TimeBetweenThreads
            // 
            num_TimeBetweenThreads.Location = new Point(6, 22);
            num_TimeBetweenThreads.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            num_TimeBetweenThreads.Minimum = new decimal(new int[] { 500, 0, 0, 0 });
            num_TimeBetweenThreads.Name = "num_TimeBetweenThreads";
            num_TimeBetweenThreads.Size = new Size(121, 23);
            num_TimeBetweenThreads.TabIndex = 0;
            num_TimeBetweenThreads.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            num_TimeBetweenThreads.ValueChanged += num_TimeBetweenProcs_ValueChanged;
            // 
            // groupBox16
            // 
            groupBox16.Controls.Add(chk_PsExecRemoteInteractive);
            groupBox16.Controls.Add(chk_PsExec_System);
            groupBox16.Location = new Point(145, 85);
            groupBox16.Name = "groupBox16";
            groupBox16.Size = new Size(151, 118);
            groupBox16.TabIndex = 3;
            groupBox16.TabStop = false;
            groupBox16.Text = "PsExec Options";
            // 
            // chk_PsExecRemoteInteractive
            // 
            chk_PsExecRemoteInteractive.AutoSize = true;
            chk_PsExecRemoteInteractive.Location = new Point(6, 51);
            chk_PsExecRemoteInteractive.Name = "chk_PsExecRemoteInteractive";
            chk_PsExecRemoteInteractive.Size = new Size(125, 19);
            chk_PsExecRemoteInteractive.TabIndex = 1;
            chk_PsExecRemoteInteractive.Text = "Remote Interactive";
            chk_PsExecRemoteInteractive.UseVisualStyleBackColor = true;
            // 
            // chk_PsExec_System
            // 
            chk_PsExec_System.AutoSize = true;
            chk_PsExec_System.Location = new Point(6, 26);
            chk_PsExec_System.Name = "chk_PsExec_System";
            chk_PsExec_System.Size = new Size(138, 19);
            chk_PsExec_System.TabIndex = 0;
            chk_PsExec_System.Text = "Use SYSTEM Account";
            chk_PsExec_System.UseVisualStyleBackColor = true;
            chk_PsExec_System.CheckedChanged += chk_PsExec_System_CheckedChanged;
            // 
            // groupBox15
            // 
            groupBox15.Controls.Add(num_MaxThreads);
            groupBox15.Location = new Point(6, 85);
            groupBox15.Name = "groupBox15";
            groupBox15.Size = new Size(133, 56);
            groupBox15.TabIndex = 2;
            groupBox15.TabStop = false;
            groupBox15.Text = "Max Remote Threads";
            // 
            // num_MaxThreads
            // 
            num_MaxThreads.Location = new Point(6, 22);
            num_MaxThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            num_MaxThreads.Name = "num_MaxThreads";
            num_MaxThreads.Size = new Size(121, 23);
            num_MaxThreads.TabIndex = 0;
            num_MaxThreads.Value = new decimal(new int[] { 5, 0, 0, 0 });
            num_MaxThreads.ValueChanged += num_MaxPsExecProcs_ValueChanged;
            // 
            // groupBox14
            // 
            groupBox14.Controls.Add(btn_LocatePsExec);
            groupBox14.Controls.Add(tb_PsExecLocation);
            groupBox14.Location = new Point(145, 22);
            groupBox14.Name = "groupBox14";
            groupBox14.Size = new Size(465, 57);
            groupBox14.TabIndex = 1;
            groupBox14.TabStop = false;
            groupBox14.Text = "PsExec Path";
            // 
            // btn_LocatePsExec
            // 
            btn_LocatePsExec.Location = new Point(401, 22);
            btn_LocatePsExec.Name = "btn_LocatePsExec";
            btn_LocatePsExec.Size = new Size(58, 23);
            btn_LocatePsExec.TabIndex = 1;
            btn_LocatePsExec.Text = "Locate";
            btn_LocatePsExec.UseVisualStyleBackColor = true;
            btn_LocatePsExec.Click += btn_LocatePsExec_Click;
            // 
            // tb_PsExecLocation
            // 
            tb_PsExecLocation.Enabled = false;
            tb_PsExecLocation.Location = new Point(6, 22);
            tb_PsExecLocation.Name = "tb_PsExecLocation";
            tb_PsExecLocation.Size = new Size(389, 23);
            tb_PsExecLocation.TabIndex = 0;
            // 
            // groupBox13
            // 
            groupBox13.Controls.Add(num_PingTimeout);
            groupBox13.Location = new Point(6, 22);
            groupBox13.Name = "groupBox13";
            groupBox13.Size = new Size(133, 57);
            groupBox13.TabIndex = 0;
            groupBox13.TabStop = false;
            groupBox13.Text = "Ping Timeout (ms)";
            // 
            // num_PingTimeout
            // 
            num_PingTimeout.Location = new Point(6, 22);
            num_PingTimeout.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            num_PingTimeout.Name = "num_PingTimeout";
            num_PingTimeout.Size = new Size(121, 23);
            num_PingTimeout.TabIndex = 0;
            num_PingTimeout.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            num_PingTimeout.ValueChanged += num_PingTimeout_ValueChanged;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(groupBox6);
            groupBox4.Controls.Add(groupBox10);
            groupBox4.Controls.Add(groupBox7);
            groupBox4.Controls.Add(groupBox8);
            groupBox4.Controls.Add(groupBox5);
            groupBox4.Location = new Point(3, 6);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(616, 406);
            groupBox4.TabIndex = 1;
            groupBox4.TabStop = false;
            groupBox4.Text = "Package";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(btn_RefeshCommandLine);
            groupBox6.Controls.Add(tb_CommandLine);
            groupBox6.Location = new Point(6, 314);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(604, 78);
            groupBox6.TabIndex = 3;
            groupBox6.TabStop = false;
            groupBox6.Text = "6) Command Line";
            // 
            // btn_RefeshCommandLine
            // 
            btn_RefeshCommandLine.Location = new Point(6, 49);
            btn_RefeshCommandLine.Name = "btn_RefeshCommandLine";
            btn_RefeshCommandLine.Size = new Size(586, 23);
            btn_RefeshCommandLine.TabIndex = 2;
            btn_RefeshCommandLine.Text = "[Re]Generate Command Line";
            btn_RefeshCommandLine.UseVisualStyleBackColor = true;
            btn_RefeshCommandLine.Click += btn_RefeshCommandLine_Click;
            // 
            // tb_CommandLine
            // 
            tb_CommandLine.Location = new Point(6, 22);
            tb_CommandLine.Name = "tb_CommandLine";
            tb_CommandLine.Size = new Size(586, 23);
            tb_CommandLine.TabIndex = 0;
            // 
            // groupBox10
            // 
            groupBox10.Controls.Add(tb_RemoteTempFolder);
            groupBox10.Location = new Point(6, 164);
            groupBox10.Name = "groupBox10";
            groupBox10.Size = new Size(604, 56);
            groupBox10.TabIndex = 2;
            groupBox10.TabStop = false;
            groupBox10.Text = "4) Remote Temp Folder";
            // 
            // tb_RemoteTempFolder
            // 
            tb_RemoteTempFolder.Location = new Point(6, 22);
            tb_RemoteTempFolder.Name = "tb_RemoteTempFolder";
            tb_RemoteTempFolder.Size = new Size(586, 23);
            tb_RemoteTempFolder.TabIndex = 0;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(tb_Arguments);
            groupBox7.Location = new Point(6, 102);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(604, 56);
            groupBox7.TabIndex = 2;
            groupBox7.TabStop = false;
            groupBox7.Text = "3) Arguments";
            // 
            // tb_Arguments
            // 
            tb_Arguments.Location = new Point(10, 22);
            tb_Arguments.Name = "tb_Arguments";
            tb_Arguments.Size = new Size(582, 23);
            tb_Arguments.TabIndex = 0;
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(groupBox11);
            groupBox8.Controls.Add(groupBox3);
            groupBox8.Location = new Point(6, 22);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(604, 74);
            groupBox8.TabIndex = 1;
            groupBox8.TabStop = false;
            // 
            // groupBox11
            // 
            groupBox11.Controls.Add(cb_Applications);
            groupBox11.Location = new Point(102, 10);
            groupBox11.Name = "groupBox11";
            groupBox11.Size = new Size(496, 56);
            groupBox11.TabIndex = 1;
            groupBox11.TabStop = false;
            groupBox11.Text = "2) Parent Process";
            // 
            // cb_Applications
            // 
            cb_Applications.FormattingEnabled = true;
            cb_Applications.Location = new Point(6, 22);
            cb_Applications.Name = "cb_Applications";
            cb_Applications.Size = new Size(484, 23);
            cb_Applications.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cb_FileExtensions);
            groupBox3.Location = new Point(6, 10);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(90, 56);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Text = "1) Extension";
            // 
            // cb_FileExtensions
            // 
            cb_FileExtensions.FormattingEnabled = true;
            cb_FileExtensions.Location = new Point(6, 22);
            cb_FileExtensions.Name = "cb_FileExtensions";
            cb_FileExtensions.Size = new Size(75, 23);
            cb_FileExtensions.TabIndex = 0;
            cb_FileExtensions.SelectedValueChanged += cb_FileExtensions_SelectedValueChanged;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(btn_LocatePackage);
            groupBox5.Controls.Add(tb_PackageLocation);
            groupBox5.Location = new Point(6, 226);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(604, 82);
            groupBox5.TabIndex = 0;
            groupBox5.TabStop = false;
            groupBox5.Text = "5) Package Location";
            // 
            // btn_LocatePackage
            // 
            btn_LocatePackage.Location = new Point(6, 51);
            btn_LocatePackage.Name = "btn_LocatePackage";
            btn_LocatePackage.Size = new Size(586, 23);
            btn_LocatePackage.TabIndex = 1;
            btn_LocatePackage.Text = "Locate Pacakge";
            btn_LocatePackage.UseVisualStyleBackColor = true;
            btn_LocatePackage.Click += btn_LocatePackage_Click;
            // 
            // tb_PackageLocation
            // 
            tb_PackageLocation.Location = new Point(6, 22);
            tb_PackageLocation.Name = "tb_PackageLocation";
            tb_PackageLocation.Size = new Size(586, 23);
            tb_PackageLocation.TabIndex = 0;
            tb_PackageLocation.TextChanged += tb_PackageLocation_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridView_Table);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(623, 633);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Results";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView_Table
            // 
            dataGridView_Table.AllowUserToAddRows = false;
            dataGridView_Table.AllowUserToDeleteRows = false;
            dataGridView_Table.AllowUserToOrderColumns = true;
            dataGridView_Table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView_Table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView_Table.DefaultCellStyle = dataGridViewCellStyle1;
            dataGridView_Table.Dock = DockStyle.Fill;
            dataGridView_Table.Location = new Point(3, 3);
            dataGridView_Table.Name = "dataGridView_Table";
            dataGridView_Table.RowHeadersVisible = false;
            dataGridView_Table.Size = new Size(617, 627);
            dataGridView_Table.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar_1, toolStripStatusLabel_1, toolStripStatusLabel_threads });
            statusStrip1.Location = new Point(0, 691);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(866, 22);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar_1
            // 
            toolStripProgressBar_1.Name = "toolStripProgressBar_1";
            toolStripProgressBar_1.Size = new Size(100, 16);
            // 
            // toolStripStatusLabel_1
            // 
            toolStripStatusLabel_1.Name = "toolStripStatusLabel_1";
            toolStripStatusLabel_1.Size = new Size(118, 17);
            toolStripStatusLabel_1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel_threads
            // 
            toolStripStatusLabel_threads.Name = "toolStripStatusLabel_threads";
            toolStripStatusLabel_threads.Size = new Size(0, 17);
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(groupBox18);
            groupBox9.Controls.Add(groupBox1);
            groupBox9.Controls.Add(groupBox2);
            groupBox9.Location = new Point(5, 27);
            groupBox9.Name = "groupBox9";
            groupBox9.Size = new Size(213, 653);
            groupBox9.TabIndex = 4;
            groupBox9.TabStop = false;
            // 
            // groupBox18
            // 
            groupBox18.Controls.Add(lbl_MonitoringThreads);
            groupBox18.Controls.Add(label1);
            groupBox18.Location = new Point(6, 442);
            groupBox18.Name = "groupBox18";
            groupBox18.Size = new Size(200, 203);
            groupBox18.TabIndex = 3;
            groupBox18.TabStop = false;
            groupBox18.Text = "Runtime Info";
            // 
            // lbl_MonitoringThreads
            // 
            lbl_MonitoringThreads.AutoSize = true;
            lbl_MonitoringThreads.Location = new Point(129, 19);
            lbl_MonitoringThreads.Name = "lbl_MonitoringThreads";
            lbl_MonitoringThreads.Size = new Size(16, 15);
            lbl_MonitoringThreads.TabIndex = 1;
            lbl_MonitoringThreads.Text = "...";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(115, 120);
            label1.TabIndex = 0;
            label1.Text = "OVERALL\r\nTotal Active Threads:\r\nMonitoring Threads:\r\nMemory Usage:\r\n\r\nREMOTE THREADS\r\nActive:\r\nTotal Created: ";
            // 
            // AppForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Silver;
            ClientSize = new Size(866, 713);
            Controls.Add(groupBox9);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Controls.Add(menuMain);
            MainMenuStrip = menuMain;
            MinimumSize = new Size(882, 752);
            Name = "AppForm";
            Text = "NiNa Deploy";
            FormClosing += AppForm_FormClosing;
            menuMain.ResumeLayout(false);
            menuMain.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            groupBox12.ResumeLayout(false);
            groupBox17.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)num_TimeBetweenThreads).EndInit();
            groupBox16.ResumeLayout(false);
            groupBox16.PerformLayout();
            groupBox15.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)num_MaxThreads).EndInit();
            groupBox14.ResumeLayout(false);
            groupBox14.PerformLayout();
            groupBox13.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)num_PingTimeout).EndInit();
            groupBox4.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBox10.ResumeLayout(false);
            groupBox10.PerformLayout();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            groupBox8.ResumeLayout(false);
            groupBox11.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView_Table).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBox9.ResumeLayout(false);
            groupBox18.ResumeLayout(false);
            groupBox18.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuMain;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar_1;
        private ToolStripStatusLabel toolStripStatusLabel_1;
        private GroupBox groupBox1;
        private TextBox txt_TargetComputer;
        private GroupBox groupBox2;
        private ListBox list_TargetComputers;
        private GroupBox groupBox4;
        private GroupBox groupBox8;
        private GroupBox groupBox7;
        private GroupBox groupBox5;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ToolStripMenuItem dataToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private DataGridView dataGridView_Table;
        private GroupBox groupBox10;
        private TextBox tb_RemoteTempFolder;
        private ComboBox cb_FileExtensions;
        private Button btn_ClearAll;
        private Button btn_RemoveSelected;
        private Button btn_ImportList;
        private Label lb_TotalComputers;
        private GroupBox groupBox9;
        private Button btn_Execute;
        private GroupBox groupBox11;
        private ComboBox cb_Applications;
        private GroupBox groupBox3;
        private GroupBox groupBox6;
        private TextBox tb_Arguments;
        private Button btn_LocatePackage;
        private TextBox tb_PackageLocation;
        private TextBox tb_CommandLine;
        private Button btn_RefeshCommandLine;
        private ToolStripMenuItem argumentsToolStripMenuItem;
        private GroupBox groupBox12;
        private GroupBox groupBox13;
        private NumericUpDown num_PingTimeout;
        private GroupBox groupBox14;
        private Button btn_LocatePsExec;
        private TextBox tb_PsExecLocation;
        private GroupBox groupBox15;
        private NumericUpDown num_MaxThreads;
        private ToolStripStatusLabel toolStripStatusLabel_threads;
        private GroupBox groupBox16;
        private CheckBox chk_PsExec_System;
        private CheckBox chk_PsExecRemoteInteractive;
        private GroupBox groupBox17;
        private NumericUpDown num_TimeBetweenThreads;
        private GroupBox groupBox18;
        private Label label1;
        private Label lbl_MonitoringThreads;
    }
}
