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
using System.Diagnostics;
using System.Resources;
using Caps2CtrlSpace.Properties;
using System.IO;
using System.Configuration;
using System.Reflection;

namespace Caps2CtrlSpace
{
    public partial class MainForm : Form
    {
        private string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        private string AppFile = Path.GetFileName(Assembly.GetEntryAssembly().EscapedCodeBase);

        //private static Configuration config = ConfigurationManager.OpenExeConfiguration( Application.ExecutablePath );
        private static Configuration config;
        private AppSettingsSection appSection;

        private bool updateConfig = false;

        private const string AppName = "Caps2CtrlSpace";

        private void LoadConfig()
        {
            try
            {
                chkAutoRun.Checked = bool.Parse(appSection.Settings["AutoRun"].Value);
            }
            catch
            {
                appSection.Settings.Add("AutoRun", chkAutoRun.Checked.ToString());
                updateConfig = true;
            }
            try
            {
                chkCapsState.Checked = bool.Parse(appSection.Settings["CapsState"].Value);
            }
            catch
            {
                appSection.Settings.Add("CapsState", chkCapsState.Checked.ToString());
                updateConfig = true;
            }
            try
            {
                chkAutoCheckImeMode.Checked = bool.Parse(appSection.Settings["AutoCheckImeMode"].Value);
            }
            catch
            {
                appSection.Settings.Add("AutoCheckImeMode", chkAutoCheckImeMode.Checked.ToString());
                updateConfig = true;
            }
            try
            {
                chkOnTop.Checked = bool.Parse(appSection.Settings["AlwaysOnTop"].Value);
            }
            catch
            {
                appSection.Settings.Add("AlwaysOnTop", chkOnTop.Checked.ToString());
                updateConfig = true;
            }

            if (updateConfig) config.Save();
            updateConfig = false;
        }

        private void SaveConfig()
        {
            try
            {
                if (updateConfig)
                {
                    appSection.Settings["AutoRun"].Value = chkAutoRun.Checked.ToString();
                    appSection.Settings["CapsState"].Value = chkCapsState.Checked.ToString();
                    appSection.Settings["AutoCheckImeMode"].Value = chkAutoCheckImeMode.Checked.ToString();
                    appSection.Settings["AlwaysOnTop"].Value = chkOnTop.Checked.ToString();
                    config.Save();
                }
            }
            catch
            {
            }
        }

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

            #region i18n locale UI
            this.Text = Resources.strTitleShort;
            lblTitle.Text = Resources.strTitleLong;
            grpOptions.Text = Resources.strOptions;
            chkAutoRun.Text = Resources.strAutoRun;
            chkCapsState.Text = Resources.strCapsState;
            chkAutoCheckImeMode.Text = Resources.strAutoCheckImeMode;
            chkOnTop.Text = Resources.strOnTop;
            #endregion

            config = ConfigurationManager.OpenExeConfiguration(Path.Combine(AppPath, Path.ChangeExtension(AppFile, Path.GetExtension(AppFile).ToLower())));
            appSection = config.AppSettings;
            LoadConfig();

            //chkAutoRun.Checked = IsAutoStart();
            //chkOnTop.Checked = true;
            //chkCapsState.Checked = true;
#if !DEBUG
            SetHide(true);
#endif
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveConfig();
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

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            SetHide(false);
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Close();
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
                updateConfig = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed," + exception.Message);
            }
        }

        private void chkCapsState_CheckedChanged(object sender, EventArgs e)
        {
            KeyMapper.CapsLockLight = chkCapsState.Checked;
            timer.Enabled = chkCapsState.Checked;
            updateConfig = true;
        }

        private void chkAutoCheckImeMode_CheckedChanged(object sender, EventArgs e)
        {
            updateConfig = true;
        }

        private void chkOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkOnTop.Checked;
            updateConfig = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (chkCapsState.Checked)
            {
                KeyMapper.CurrentKeyboardLayout = (KeyboardLayout)Ime.KeyboardLayout;
                if (chkAutoCheckImeMode.Checked)
                {
                    KeyMapper.CurrentImeMode = Ime.Mode;
                    if(KeyMapper.CurrentImeMode != ImeIndicatorMode.Manual)
                        KeyMapper.ToggleLights(KeyMapper.Locks.KeyboardCapsLockOn);
                }
                else KeyMapper.CurrentImeMode = ImeIndicatorMode.Manual;
#if DEBUG
                Console.WriteLine($"{KeyMapper.CurrentImeMode}");
#endif
                lblImeLayout.Text = $"{Ime.KeyboardLayout}, {Ime.KeyboardLayoutName}";
                lblWindowText.Text = $"{Ime.ActiveWindowTitle}[{Ime.ActiveWindowHandle.ToInt32()}]";
                picIndicator.Image = Ime.CurrentImeIndicator;
            }
        }

    }
}
