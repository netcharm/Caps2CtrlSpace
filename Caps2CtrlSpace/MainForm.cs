﻿using System;
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

        private bool loadingConfig = false;
        private bool updateConfig = false;

        private const string AppName = "Caps2CtrlSpace";

        private void LoadConfig()
        {
            loadingConfig = true;
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
                chkOnTop.Checked = bool.Parse(appSection.Settings["AlwaysOnTop"].Value);
            }
            catch
            {
                appSection.Settings.Add("AlwaysOnTop", chkOnTop.Checked.ToString());
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
                edAutoCheckInterval.Value = int.Parse(appSection.Settings["AutoCheckInterval"].Value);
            }
            catch
            {
                appSection.Settings.Add("AutoCheckInterval", edAutoCheckInterval.Value.ToString());
                updateConfig = true;
            }
            try
            {
                chkImeAutoCloseKeePass.Checked = bool.Parse(appSection.Settings["AutoCloseKeePassIME"].Value);
            }
            catch
            {
                appSection.Settings.Add("AutoCloseKeePassIME", chkImeAutoCloseKeePass.Checked.ToString());
                updateConfig = true;
            }
            try
            {
                edKeePassHotKey.Text = appSection.Settings["KeePassHotKey"].Value;
            }
            catch
            {
                appSection.Settings.Add("KeePassHotKey", edKeePassHotKey.Text);
                updateConfig = true;
            }
            try
            {
                double Tolerance = 0.10;
                double.TryParse(appSection.Settings["ModeDetectTolerance"].Value, out Tolerance);
                Ime.Tolerance = Tolerance;
            }
            catch
            {
                appSection.Settings.Add("ModeDetectTolerance", $"{Ime.Tolerance}");
                updateConfig = true;
            }

            if (updateConfig) config.Save();
            updateConfig = false;
            loadingConfig = false;
        }

        private void SaveConfig(bool force = false)
        {
            try
            {
                if (loadingConfig) return;
                if (updateConfig || force)
                {
                    appSection.Settings["AutoRun"].Value = chkAutoRun.Checked.ToString();
                    appSection.Settings["CapsState"].Value = chkCapsState.Checked.ToString();
                    appSection.Settings["AutoCheckImeMode"].Value = chkAutoCheckImeMode.Checked.ToString();
                    appSection.Settings["AutoCheckInterval"].Value = edAutoCheckInterval.Value.ToString();
                    appSection.Settings["AlwaysOnTop"].Value = chkOnTop.Checked.ToString();
                    appSection.Settings["AutoCloseKeePassIME"].Value = chkImeAutoCloseKeePass.Checked.ToString();
                    appSection.Settings["KeePassHotKey"].Value = edKeePassHotKey.Text.Trim();
                    appSection.Settings["ModeDetectTolerance"].Value = Ime.Tolerance.ToString();

                    config.Save();
                    updateConfig = false;
                }
            }
            catch
            {
            }
        }

        private void SetHide(bool hide = true)
        {
            if (hide)
            {
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
            else
            {
                this.ShowInTaskbar = true;
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
#if !DEBUG
            //this.WindowState = FormWindowState.Minimized;
#endif
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            #region i18n locale UI
            this.Text = Resources.strTitleShort;
            lblTitle.Text = Resources.strTitleLong.Replace("\\n", "\n");
            grpOptions.Text = Resources.strOptions;
            chkAutoRun.Text = Resources.strAutoRun;
            chkCapsState.Text = Resources.strCapsState;
            chkAutoCheckImeMode.Text = Resources.strAutoCheckImeMode;
            chkOnTop.Text = Resources.strOnTop;
            chkImeAutoCloseKeePass.Text = Resources.strImeAutoCloseKeePass;
            lblKeePassHotKey.Text = Resources.strKeePassHotKey;

            tsmiImeModeEnglish.Text = Resources.strSaveAsImeModeEnglish;
            tsmiImeModeLocale.Text = Resources.strSaveAsImeModeLocale;
            tsmiImeModeDisabled.Text = Resources.strSaveAsImeModeDisabled;
            tsmiImeModeClose.Text = Resources.strSaveAsImeModeClose;
            tsmiInputIndicator.Text = Resources.strSaveAsInputIndicator;

            tsmiShow.Text = Resources.strShowMainForm;
            tsmiExit.Text = Resources.strExit;
            #endregion

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Icon = Icon;
            notifyIcon.Text = this.Text;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipTitle = Resources.strTitleShort;
            notifyIcon.BalloonTipText = Resources.strTitleLong;

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
            KeyMapper.CloseCapsLock();
            KeyMapper.CapsLockLightOff();
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

        private void edTest_DoubleClick(object sender, EventArgs e)
        {
            if (KeyMapper.CurrentKeyboardLayout == SysKeyboardLayout.CHS ||
                KeyMapper.CurrentKeyboardLayout == SysKeyboardLayout.CHK)
            {
                if (edTest.ImeMode == ImeMode.On)
                    edTest.ImeMode = ImeMode.Off;
                else if (edTest.ImeMode == ImeMode.Off)
                    edTest.ImeMode = ImeMode.On;
                else
                    edTest.ImeMode = ImeMode.Off;
            }
            else if (KeyMapper.CurrentKeyboardLayout == SysKeyboardLayout.CHT)
            {
                if (edTest.ImeMode == ImeMode.On)
                    edTest.ImeMode = ImeMode.Off;
                else if (edTest.ImeMode == ImeMode.Off)
                    edTest.ImeMode = ImeMode.Close;
                else if (edTest.ImeMode == ImeMode.Close)
                    edTest.ImeMode = ImeMode.Disable;
                else if (edTest.ImeMode == ImeMode.Disable)
                    edTest.ImeMode = ImeMode.On;
                else
                    edTest.ImeMode = ImeMode.Off;
            }
            else if (KeyMapper.CurrentKeyboardLayout == SysKeyboardLayout.JAP)
            {
                if (edTest.ImeMode == ImeMode.Hiragana)
                    edTest.ImeMode = ImeMode.Off;
                else if (edTest.ImeMode == ImeMode.Off)
                    edTest.ImeMode = ImeMode.Disable;
                else if (edTest.ImeMode == ImeMode.Disable)
                    edTest.ImeMode = ImeMode.Hiragana;
                else
                    edTest.ImeMode = ImeMode.Off;
            }
            else if (KeyMapper.CurrentKeyboardLayout == SysKeyboardLayout.KOR)
            {
                if (edTest.ImeMode == ImeMode.Alpha)
                    edTest.ImeMode = ImeMode.Disable;
                else if (edTest.ImeMode == ImeMode.Disable)
                    edTest.ImeMode = ImeMode.AlphaFull;
                else if (edTest.ImeMode == ImeMode.AlphaFull)
                    edTest.ImeMode = ImeMode.Hangul;
                else if (edTest.ImeMode == ImeMode.Hangul)
                    edTest.ImeMode = ImeMode.HangulFull;
                else if (edTest.ImeMode == ImeMode.HangulFull)
                    edTest.ImeMode = ImeMode.Alpha;
                else
                    edTest.ImeMode = ImeMode.Alpha;
            }
        }

        private void edKeePassHotKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.ShiftKey) return;
            else if (e.Alt && e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu) return;
            else if (e.Control && e.KeyCode == Keys.ControlKey) return;

            var modifiers = string.Empty;
            if (e.Modifiers != Keys.None)
                modifiers = $"{e.Modifiers.ToString()}+";
            //edKeePassHotKey.Text = $"{modifiers}{e.KeyCode.ToString()}";            
        }

        private void edKeePassHotKey_TextChanged(object sender, EventArgs e)
        {
            if (sender == edKeePassHotKey)
            {
                Keys hotkey = Keys.None;
                //Enum.TryParse<Keys>(edKeePassHotKey.Text, out hotkey);

                var kc = new KeysConverter();
                try
                {
                    var keys = edKeePassHotKey.Text.Trim().Split('+');
                    var kv = keys.Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1).ToLower()}");
                    var pos = edKeePassHotKey.SelectionStart;
                    edKeePassHotKey.Text = string.Join("+", kv);
                    edKeePassHotKey.SelectionStart = pos;
                    hotkey = (Keys)kc.ConvertFromInvariantString(edKeePassHotKey.Text);
                }
                catch { }

                if (hotkey != Keys.None)
                {
                    KeyMapper.KeePassHotKey = hotkey;
                }
                else
                {
                    KeyMapper.KeePassHotKey = Keys.None;
                }
                updateConfig = true;
                SaveConfig(updateConfig);
            }
        }

        private void edAutoCheckInterval_ValueChanged(object sender, EventArgs e)
        {
            if (sender == edAutoCheckInterval)
            {
                timer.Stop();
                timer.Interval = Convert.ToInt32(edAutoCheckInterval.Value);
                timer.Start();
                updateConfig = true;
                SaveConfig(updateConfig);
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

        private void tsmiImeState_Click(object sender, EventArgs e)
        {
            if (sender == tsmiImeModeEnglish && picImeMode.Image is Bitmap)
                picImeMode.Image.Save(Path.Combine(AppPath, $"{Ime.KeyboardLayout}_{(int)ImeIndicatorMode.English}.png"));
            else if (sender == tsmiImeModeLocale && picImeMode.Image is Bitmap)
                picImeMode.Image.Save(Path.Combine(AppPath, $"{Ime.KeyboardLayout}_{(int)ImeIndicatorMode.Locale}.png"));
            else if (sender == tsmiImeModeDisabled && picImeMode.Image is Bitmap)
                picImeMode.Image.Save(Path.Combine(AppPath, $"{Ime.KeyboardLayout}_{(int)ImeIndicatorMode.Disabled}.png"));
            else if (sender == tsmiImeModeClose && picImeMode.Image is Bitmap)
                picImeMode.Image.Save(Path.Combine(AppPath, $"{Ime.KeyboardLayout}_{(int)ImeIndicatorMode.Close}.png"));
            else if (sender == tsmiInputIndicator && picInputIndicator.Image is Bitmap)
                picInputIndicator.Image.Save(Path.Combine(AppPath, $"{Ime.KeyboardLayout}_{(int)ImeIndicatorMode.Layout}.png"));
        }

        private void cmsImeMode_Opening(object sender, CancelEventArgs e)
        {
            if (sender == cmsImeMode)
            {
                var source = (sender as ContextMenuStrip).SourceControl;
                if (source == picImeMode)
                {
                    tsmiImeModeEnglish.Enabled = true;
                    tsmiImeModeLocale.Enabled = true;
                    tsmiImeModeDisabled.Enabled = true;
                    tsmiImeModeClose.Enabled = true;

                    tsmiInputIndicator.Enabled = false;
                }
                else if (source == picInputIndicator)
                {
                    tsmiImeModeEnglish.Enabled = false;
                    tsmiImeModeLocale.Enabled = false;
                    tsmiImeModeDisabled.Enabled = false;
                    tsmiImeModeClose.Enabled = false;

                    tsmiInputIndicator.Enabled = true;
                }
            }
        }

        private void chkAutoRun_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkAutoRun)
            {
                try
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (chkAutoRun.Checked)
                            rk.SetValue(AppName, Application.ExecutablePath.ToString());
                        else
                            rk.DeleteValue(AppName);
                        rk.Close();
                        updateConfig = true;
                        SaveConfig(updateConfig);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Failed," + exception.Message);
                }
            }
        }

        private void chkOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkOnTop)
            {
                this.TopMost = chkOnTop.Checked;
                updateConfig = true;
                SaveConfig(updateConfig);
            }
        }

        private void chkCapsState_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkCapsState)
            {
                KeyMapper.CapsLockLightEnabled = chkCapsState.Checked;
                timer.Enabled = chkCapsState.Checked;
                updateConfig = true;
                SaveConfig(updateConfig);
            }
        }

        private void chkAutoCheckImeMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkAutoCheckImeMode)
            {
                if (chkAutoCheckImeMode.Checked == false)
                {
                    chkImeAutoCloseKeePass.Enabled = false;
                    KeyMapper.AutoCloseKeePassIME = false;
                }
                else
                {
                    chkImeAutoCloseKeePass.Enabled = true;
                    KeyMapper.AutoCloseKeePassIME = chkImeAutoCloseKeePass.Checked;
                }
                updateConfig = true;
                SaveConfig(updateConfig);
            }
        }

        private void chkImeAutoCloseKeePass_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == chkImeAutoCloseKeePass)
            {
                chkImeAutoCloseKeePass.Enabled = chkAutoCheckImeMode.Checked;
                KeyMapper.AutoCloseKeePassIME = chkAutoCheckImeMode.Checked && chkImeAutoCloseKeePass.Enabled && chkImeAutoCloseKeePass.Checked;
                updateConfig = true;
                SaveConfig(updateConfig);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (Ime.GetLastInputTime() > 5) GC.Collect();
            if (Ime.GetLastInputTime() > 1) return;
            if (chkCapsState.Checked || chkAutoCheckImeMode.Checked)
            {
                var currentLayout = Ime.KeyboardLayout;
                var currentMode = Ime.Mode;
                if (currentMode != ImeIndicatorMode.Disabled)
                {
                    KeyMapper.CapsLockLightAutoCheck = chkCapsState.Checked;
                    KeyMapper.CurrentImeMode = currentMode;
                    try
                    {
                        KeyMapper.CurrentKeyboardLayout = (SysKeyboardLayout)currentLayout;
                    }
                    catch (Exception)
                    {
                        KeyMapper.CurrentKeyboardLayout = SysKeyboardLayout.ENG;
                    }
                }
                picImeMode.Image = Ime.CurrentImeModeBitmap;
                picInputIndicator.Image = Ime.CurrentInputIndicatorBitmap;

                lblImeLayout.Text = $"{currentLayout}, {Ime.KeyboardLayoutName}";
                lblWindowText.Text = $"{Ime.ActiveWindowTitle}[{Ime.ActiveWindowHandle.ToInt32()}]";

                edTest.Text = edTest.ImeMode.ToString();
            }
        }
    }
}
