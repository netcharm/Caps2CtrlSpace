using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Caps2CtrlSpace
{
    public partial class MainForm : Form
    {
        private const string AppName = "Caps2CtrlSpace";

        private void SetHide(bool hide = true)
        {
            this.ShowInTaskbar = true;
            if (hide)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
            notifyIcon.Visible = hide;
        }

        /// <summary>
        /// 是否自动启动
        /// </summary>
        /// <returns></returns>
        private bool IsAutoStart()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var currValue = rk.GetValue(AppName);
#if DEBUG
            Console.WriteLine(currValue);
#endif
            return currValue != null && currValue.ToString() == Application.ExecutablePath.ToString();
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Icon = Icon;
            chkAutoRun.Checked = IsAutoStart();

            chkOnTop.Checked = true;
            chkCapsState.Checked = true;

            //SetHide();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                SetHide(true);
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                SetHide(false);
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetHide(false);
        }

        private void chkAutoRun_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (chkAutoRun.Checked)
                {
                    rk.SetValue(AppName, Application.ExecutablePath.ToString());
                }
                else
                {
                    rk.DeleteValue(AppName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed," + exception.Message);
            }
        }

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            SetHide(false);
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkCapsState_CheckedChanged(object sender, EventArgs e)
        {
            KeyMapper.CapsLockLight = chkCapsState.Checked;
            timer.Enabled = chkCapsState.Checked;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (chkCapsState.Checked)
            {
                //InputLanguageCollection ilc = InputLanguage.InstalledInputLanguages;//获取所有安装的输入法
                InputLanguage il = InputLanguage.CurrentInputLanguage;//获取当前UI线程的输入法以及状态
                lblImeLayout.Text = $"{il.Handle}:{il.Culture.KeyboardLayoutId}, {il.LayoutName}";
            }
        }

        private void chkOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkOnTop.Checked;
        }
    }
}
