namespace Caps2CtrlSpace
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiShow = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSepExit = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.lblImeLayout = new System.Windows.Forms.Label();
            this.lblWindowText = new System.Windows.Forms.Label();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.edTest = new System.Windows.Forms.TextBox();
            this.chkOnTop = new System.Windows.Forms.CheckBox();
            this.chkAutoCheckImeMode = new System.Windows.Forms.CheckBox();
            this.chkCapsState = new System.Windows.Forms.CheckBox();
            this.chkAutoRun = new System.Windows.Forms.CheckBox();
            this.picImeMode = new System.Windows.Forms.PictureBox();
            this.cmsImeMode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiImeModeEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiImeModeLocale = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiImeModeDisabled = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiImeModeClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSepInputIndicator = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiInputIndicator = new System.Windows.Forms.ToolStripMenuItem();
            this.picInputIndicator = new System.Windows.Forms.PictureBox();
            this.cmsNotifyIcon.SuspendLayout();
            this.grpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImeMode)).BeginInit();
            this.cmsImeMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picInputIndicator)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("宋体", 14F);
            this.lblTitle.Location = new System.Drawing.Point(8, 8);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(4);
            this.lblTitle.Size = new System.Drawing.Size(398, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Convert CapsLock key to Ctrl+Space";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.cmsNotifyIcon;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Caps2CtrlSpace";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // cmsNotifyIcon
            // 
            this.cmsNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShow,
            this.tsmiSepExit,
            this.tsmiExit});
            this.cmsNotifyIcon.Name = "cmsNotifyIcon";
            this.cmsNotifyIcon.ShowImageMargin = false;
            this.cmsNotifyIcon.Size = new System.Drawing.Size(134, 54);
            // 
            // tsmiShow
            // 
            this.tsmiShow.Name = "tsmiShow";
            this.tsmiShow.Size = new System.Drawing.Size(133, 22);
            this.tsmiShow.Text = "Show Window";
            this.tsmiShow.Click += new System.EventHandler(this.tsmiShow_Click);
            // 
            // tsmiSepExit
            // 
            this.tsmiSepExit.Name = "tsmiSepExit";
            this.tsmiSepExit.Size = new System.Drawing.Size(130, 6);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(133, 22);
            this.tsmiExit.Text = "Exit";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // lblImeLayout
            // 
            this.lblImeLayout.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblImeLayout.Location = new System.Drawing.Point(8, 231);
            this.lblImeLayout.Name = "lblImeLayout";
            this.lblImeLayout.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblImeLayout.Size = new System.Drawing.Size(398, 31);
            this.lblImeLayout.TabIndex = 5;
            this.lblImeLayout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWindowText
            // 
            this.lblWindowText.AutoEllipsis = true;
            this.lblWindowText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblWindowText.Location = new System.Drawing.Point(8, 200);
            this.lblWindowText.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
            this.lblWindowText.Name = "lblWindowText";
            this.lblWindowText.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblWindowText.Size = new System.Drawing.Size(398, 31);
            this.lblWindowText.TabIndex = 6;
            this.lblWindowText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.edTest);
            this.grpOptions.Controls.Add(this.chkOnTop);
            this.grpOptions.Controls.Add(this.chkAutoCheckImeMode);
            this.grpOptions.Controls.Add(this.chkCapsState);
            this.grpOptions.Controls.Add(this.chkAutoRun);
            this.grpOptions.Location = new System.Drawing.Point(46, 65);
            this.grpOptions.Margin = new System.Windows.Forms.Padding(16);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Padding = new System.Windows.Forms.Padding(8);
            this.grpOptions.Size = new System.Drawing.Size(323, 131);
            this.grpOptions.TabIndex = 7;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Options";
            // 
            // edTest
            // 
            this.edTest.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.edTest.Location = new System.Drawing.Point(239, 96);
            this.edTest.Name = "edTest";
            this.edTest.Size = new System.Drawing.Size(76, 21);
            this.edTest.TabIndex = 9;
            this.edTest.DoubleClick += new System.EventHandler(this.edTest_DoubleClick);
            // 
            // chkOnTop
            // 
            this.chkOnTop.AutoSize = true;
            this.chkOnTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkOnTop.Location = new System.Drawing.Point(8, 94);
            this.chkOnTop.Name = "chkOnTop";
            this.chkOnTop.Padding = new System.Windows.Forms.Padding(4);
            this.chkOnTop.Size = new System.Drawing.Size(307, 24);
            this.chkOnTop.TabIndex = 8;
            this.chkOnTop.Text = "Always On Top";
            this.chkOnTop.UseVisualStyleBackColor = true;
            this.chkOnTop.CheckedChanged += new System.EventHandler(this.chkOnTop_CheckedChanged);
            // 
            // chkAutoCheckImeMode
            // 
            this.chkAutoCheckImeMode.AutoSize = true;
            this.chkAutoCheckImeMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAutoCheckImeMode.Location = new System.Drawing.Point(8, 70);
            this.chkAutoCheckImeMode.Name = "chkAutoCheckImeMode";
            this.chkAutoCheckImeMode.Padding = new System.Windows.Forms.Padding(4);
            this.chkAutoCheckImeMode.Size = new System.Drawing.Size(307, 24);
            this.chkAutoCheckImeMode.TabIndex = 7;
            this.chkAutoCheckImeMode.Text = "Auto Check IME Mode (Will increase CPU usage)";
            this.chkAutoCheckImeMode.UseVisualStyleBackColor = true;
            this.chkAutoCheckImeMode.CheckedChanged += new System.EventHandler(this.chkAutoCheckImeMode_CheckedChanged);
            // 
            // chkCapsState
            // 
            this.chkCapsState.AutoSize = true;
            this.chkCapsState.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkCapsState.Location = new System.Drawing.Point(8, 46);
            this.chkCapsState.Name = "chkCapsState";
            this.chkCapsState.Padding = new System.Windows.Forms.Padding(4);
            this.chkCapsState.Size = new System.Drawing.Size(307, 24);
            this.chkCapsState.TabIndex = 6;
            this.chkCapsState.Text = "Enabled CapsLock Indicator Light (If Available)";
            this.chkCapsState.UseVisualStyleBackColor = true;
            this.chkCapsState.CheckedChanged += new System.EventHandler(this.chkCapsState_CheckedChanged);
            // 
            // chkAutoRun
            // 
            this.chkAutoRun.AutoSize = true;
            this.chkAutoRun.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAutoRun.Location = new System.Drawing.Point(8, 22);
            this.chkAutoRun.Margin = new System.Windows.Forms.Padding(2);
            this.chkAutoRun.Name = "chkAutoRun";
            this.chkAutoRun.Padding = new System.Windows.Forms.Padding(4);
            this.chkAutoRun.Size = new System.Drawing.Size(307, 24);
            this.chkAutoRun.TabIndex = 5;
            this.chkAutoRun.Text = "Auto Run Application When Windows Startup";
            this.chkAutoRun.UseVisualStyleBackColor = true;
            this.chkAutoRun.CheckedChanged += new System.EventHandler(this.chkAutoRun_CheckedChanged);
            // 
            // picImeMode
            // 
            this.picImeMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picImeMode.ContextMenuStrip = this.cmsImeMode;
            this.picImeMode.Location = new System.Drawing.Point(10, 233);
            this.picImeMode.Name = "picImeMode";
            this.picImeMode.Size = new System.Drawing.Size(25, 26);
            this.picImeMode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picImeMode.TabIndex = 8;
            this.picImeMode.TabStop = false;
            // 
            // cmsImeMode
            // 
            this.cmsImeMode.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmsImeMode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiImeModeEnglish,
            this.tsmiImeModeLocale,
            this.tsmiImeModeDisabled,
            this.tsmiImeModeClose,
            this.tsmiSepInputIndicator,
            this.tsmiInputIndicator});
            this.cmsImeMode.Name = "cmsImeMode";
            this.cmsImeMode.Size = new System.Drawing.Size(249, 182);
            this.cmsImeMode.Opening += new System.ComponentModel.CancelEventHandler(this.cmsImeMode_Opening);
            // 
            // tsmiImeModeEnglish
            // 
            this.tsmiImeModeEnglish.Image = global::Caps2CtrlSpace.Properties.Resources._1028_1_;
            this.tsmiImeModeEnglish.Name = "tsmiImeModeEnglish";
            this.tsmiImeModeEnglish.Size = new System.Drawing.Size(248, 30);
            this.tsmiImeModeEnglish.Text = "Save As English Icon";
            this.tsmiImeModeEnglish.Click += new System.EventHandler(this.tsmiImeState_Click);
            // 
            // tsmiImeModeLocale
            // 
            this.tsmiImeModeLocale.Image = global::Caps2CtrlSpace.Properties.Resources._1028_2_;
            this.tsmiImeModeLocale.Name = "tsmiImeModeLocale";
            this.tsmiImeModeLocale.Size = new System.Drawing.Size(248, 30);
            this.tsmiImeModeLocale.Text = "Save As Locale Icon";
            this.tsmiImeModeLocale.Click += new System.EventHandler(this.tsmiImeState_Click);
            // 
            // tsmiImeModeDisabled
            // 
            this.tsmiImeModeDisabled.Image = global::Caps2CtrlSpace.Properties.Resources._1028_3_;
            this.tsmiImeModeDisabled.Name = "tsmiImeModeDisabled";
            this.tsmiImeModeDisabled.Size = new System.Drawing.Size(248, 30);
            this.tsmiImeModeDisabled.Text = "Save As Disabled Icon";
            this.tsmiImeModeDisabled.Click += new System.EventHandler(this.tsmiImeState_Click);
            // 
            // tsmiImeModeClose
            // 
            this.tsmiImeModeClose.Image = global::Caps2CtrlSpace.Properties.Resources._1028_4_;
            this.tsmiImeModeClose.Name = "tsmiImeModeClose";
            this.tsmiImeModeClose.Size = new System.Drawing.Size(248, 30);
            this.tsmiImeModeClose.Text = "Save As Close Icon";
            this.tsmiImeModeClose.Click += new System.EventHandler(this.tsmiImeState_Click);
            // 
            // tsmiSepInputIndicator
            // 
            this.tsmiSepInputIndicator.Name = "tsmiSepInputIndicator";
            this.tsmiSepInputIndicator.Size = new System.Drawing.Size(245, 6);
            // 
            // tsmiInputIndicator
            // 
            this.tsmiInputIndicator.Image = global::Caps2CtrlSpace.Properties.Resources._2052_0_;
            this.tsmiInputIndicator.Name = "tsmiInputIndicator";
            this.tsmiInputIndicator.Size = new System.Drawing.Size(248, 30);
            this.tsmiInputIndicator.Text = "Save As Input Indicator Icon";
            this.tsmiInputIndicator.Click += new System.EventHandler(this.tsmiImeState_Click);
            // 
            // picInputIndicator
            // 
            this.picInputIndicator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picInputIndicator.ContextMenuStrip = this.cmsImeMode;
            this.picInputIndicator.Location = new System.Drawing.Point(378, 233);
            this.picInputIndicator.Name = "picInputIndicator";
            this.picInputIndicator.Size = new System.Drawing.Size(25, 26);
            this.picInputIndicator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picInputIndicator.TabIndex = 9;
            this.picInputIndicator.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(414, 270);
            this.Controls.Add(this.picInputIndicator);
            this.Controls.Add(this.picImeMode);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.lblWindowText);
            this.Controls.Add(this.lblImeLayout);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CapsLock To Ctrl+Space";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.cmsNotifyIcon.ResumeLayout(false);
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImeMode)).EndInit();
            this.cmsImeMode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picInputIndicator)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip cmsNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem tsmiShow;
        private System.Windows.Forms.ToolStripSeparator tsmiSepExit;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label lblImeLayout;
        private System.Windows.Forms.Label lblWindowText;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.CheckBox chkOnTop;
        private System.Windows.Forms.CheckBox chkCapsState;
        private System.Windows.Forms.CheckBox chkAutoCheckImeMode;
        private System.Windows.Forms.CheckBox chkAutoRun;
        private System.Windows.Forms.PictureBox picImeMode;
        private System.Windows.Forms.PictureBox picInputIndicator;
        private System.Windows.Forms.ContextMenuStrip cmsImeMode;
        private System.Windows.Forms.ToolStripMenuItem tsmiImeModeClose;
        private System.Windows.Forms.ToolStripMenuItem tsmiImeModeEnglish;
        private System.Windows.Forms.ToolStripMenuItem tsmiImeModeLocale;
        private System.Windows.Forms.ToolStripMenuItem tsmiImeModeDisabled;
        private System.Windows.Forms.ToolStripSeparator tsmiSepInputIndicator;
        private System.Windows.Forms.ToolStripMenuItem tsmiInputIndicator;
        private System.Windows.Forms.TextBox edTest;
    }
}

