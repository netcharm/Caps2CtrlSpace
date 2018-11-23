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
using System.Runtime.InteropServices;
using System.Threading;

namespace Caps2CtrlSpace
{
    public partial class MainForm : Form
    {
        private const string AppName = "Caps2CtrlSpace";

        #region get current input window keyboard layout functions
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr AttachThreadInput(IntPtr idAttach,
                             IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetKeyboardLayout(uint thread);
        #endregion

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

        private Dictionary<int, string> InstalledKeyboardLayout = new Dictionary<int, string>();
        private int lastKeyboardLayout = 1033;
        private void timer_Tick(object sender, EventArgs e)
        {
            if (chkCapsState.Checked)
            {
                IntPtr activeWindowHandle = GetForegroundWindow();
                IntPtr activeWindowThread = GetWindowThreadProcessId(activeWindowHandle, IntPtr.Zero);
                IntPtr thisWindowThread = GetWindowThreadProcessId(this.Handle, IntPtr.Zero);

                AttachThreadInput(activeWindowThread, thisWindowThread, true);
                IntPtr focusedControlHandle = GetFocus();
                var kl = GetKeyboardLayout((uint)activeWindowThread.ToInt32()).ToInt32() & 0xFFFF;
                AttachThreadInput(activeWindowThread, thisWindowThread, false);

                if (InstalledKeyboardLayout.Count <= 0 || !InstalledKeyboardLayout.ContainsKey(kl))
                {
                    InputLanguageCollection ilc = InputLanguage.InstalledInputLanguages;//获取所有安装的输入法
                    foreach(InputLanguage il in ilc)
                    {
                        InstalledKeyboardLayout.Add(il.Culture.KeyboardLayoutId, il.LayoutName);
                    }
                }
                //InputLanguage cil = InputLanguage.CurrentInputLanguage;//获取当前UI线程的输入法以及状态
                if (InstalledKeyboardLayout.ContainsKey(kl))
                {
                    if (kl != lastKeyboardLayout)
                    {
                        if (IsKeyLocked(Keys.CapsLock))
                        {
                            KeyMapper.ToggleCapsLock();
                            Thread.Sleep(100);
                        }
                        lastKeyboardLayout = kl;
                    }
                    lblImeLayout.Text = $"{focusedControlHandle}:{kl}, {InstalledKeyboardLayout[kl]}";
                }
                KeyMapper.CurrentKeyboardLayout = (uint)kl;
            }
        }

        private void chkOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkOnTop.Checked;
        }
    }
}
