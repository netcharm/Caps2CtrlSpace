using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caps2CtrlSpace
{
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    public enum SysKeyboardLayout { ENG = 1033, CHS = 2052, CHT = 1028, JAP = 1041 };

    public class KeyMapper
    {
        static public bool CapsLockLight { get; set; } = false;

        static public SysKeyboardLayout CurrentKeyboardLayout { get; set; } = SysKeyboardLayout.ENG;
        static public ImeIndicatorMode CurrentImeMode { get; set; } = ImeIndicatorMode.Disabled;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        #region Hook functions
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        #endregion

        #region Keyboard indication light functions
        [DllImport("kernel32.dll")]
        private static extern bool DefineDosDevice(uint dwFlags, string lpDeviceName, string lpTargetPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern SafeFileHandle CreateFileA(
             [MarshalAs(UnmanagedType.LPStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("Kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            int IoControlCode,
            ref KeyboardIndicatorParameters InBuffer,
            int nInBufferSize,
            ref KeyboardIndicatorParameters OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr Overlapped
        );

        [Flags]
        public enum LockLights : ushort
        {
            None = 0,
            KeyboardScrollLockOn = 1,
            KeyboardNumLockOn = 2,
            KeyboardCapsLockOn = 4
        }
        public enum LockLightsOp { Toggle, On, Off };

        public struct KeyboardIndicatorParameters
        {
            public ushort UnitId;
            public LockLights LedFlags;
        }

        private const uint FileAnyAccess = 0;
        private const uint MethodBuffered = 0;
        private const uint FileDeviceKeyboard = 0x0000000b;
        private const int DddRawTargetPath = 0x00000001;

        static uint ControlCode(uint deviceType, uint function, uint method, uint access)
        {
            return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
        }

        private static uint IOCTL_KEYBOARD_SET_INDICATORS = ControlCode(FileDeviceKeyboard, 0x0002, MethodBuffered, FileAnyAccess);
        private static uint IOCTL_KEYBOARD_QUERY_INDICATORS = ControlCode(FileDeviceKeyboard, 0x0010, MethodBuffered, FileAnyAccess);
        #endregion

        public static void SetLights(LockLights locks, LockLightsOp op = LockLightsOp.Toggle)
        {
            DefineDosDevice(DddRawTargetPath, "keyboard", "\\Device\\KeyboardClass0");

            var indicatorsIn = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };
            var indicatorsOut = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };

            using (var hKeybd = CreateFile("\\\\.\\keyboard", FileAccess.Write, 0, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
            {
                var size = Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
                int bytesReturned = 0;

                var retQ = DeviceIoControl(hKeybd, (int)IOCTL_KEYBOARD_QUERY_INDICATORS, ref indicatorsOut, size, ref indicatorsOut, size, out bytesReturned, IntPtr.Zero);
                if (retQ)
                {
                    if (op == LockLightsOp.Toggle)
                    {
                        if(indicatorsOut.LedFlags.HasFlag(locks))
                            indicatorsIn.LedFlags = indicatorsOut.LedFlags & ~locks;
                        else
                            indicatorsIn.LedFlags = indicatorsOut.LedFlags | locks;
                    }
                    else if (op == LockLightsOp.On)
                    {
                        indicatorsIn.LedFlags = indicatorsOut.LedFlags | locks;
                    }
                    else if (op == LockLightsOp.Off)
                    {
                        indicatorsIn.LedFlags = indicatorsOut.LedFlags & ~locks;
                    }
                    var retS = DeviceIoControl(hKeybd, (int)IOCTL_KEYBOARD_SET_INDICATORS, ref indicatorsIn, size, ref indicatorsOut, size, out bytesReturned, IntPtr.Zero);
                }                
            }
        }

        public static void CapsLockLightOn()
        {
            SetLights(LockLights.KeyboardCapsLockOn, LockLightsOp.On);
        }

        public static void CapsLockLightOff()
        {
            SetLights(LockLights.KeyboardCapsLockOn, LockLightsOp.Off);
        }

        public static void CapsLockLightToggle()
        {
            SetLights(LockLights.KeyboardCapsLockOn, LockLightsOp.Toggle);
        }

        public static void CapsLockLightAuto()
        {
            if(CurrentKeyboardLayout != SysKeyboardLayout.ENG)
            {
                if (CurrentImeMode == ImeIndicatorMode.Locale)
                    CapsLockLightOn();
                else
                    CapsLockLightOff();
            }
        }

        #region Keyboard event for toggle capslock state
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        #endregion

        public static void ToggleCapsLock()
        {
            //SendKeys.SendWait("^");
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        public static void CloseCapsLock()
        {
            if (Control.IsKeyLocked(Keys.CapsLock)) ToggleCapsLock();
        }

        public static void ToggleCtrlSpace()
        {
            //SendKeys.SendWait("^");
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(0x11, 0, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x20, 0, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x20, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            keybd_event(0x11, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }
        
        #region Keyboard Hook
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private static SysKeyboardLayout[] CapsLockEnabledLayout = new SysKeyboardLayout[] { SysKeyboardLayout.CHS, SysKeyboardLayout.CHT, SysKeyboardLayout.JAP };

        private static IntPtr _keyboardHookID = IntPtr.Zero;

        private static IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && CurrentKeyboardLayout != SysKeyboardLayout.ENG)
            {
                var kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vkCode = kbd.vkCode; //Marshal.ReadInt32(lParam);
#if DEBUG
                Console.WriteLine($"KeyCode: {vkCode}, {kbd.flags}, {CurrentKeyboardLayout}");
#endif
                if (kbd.flags == 0 && (Keys)vkCode == Keys.Capital && CapsLockEnabledLayout.Contains(CurrentKeyboardLayout))
                {
                    try
                    {
                        if (CurrentKeyboardLayout == SysKeyboardLayout.CHS)
                        {
                            SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                        }
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.CHT)
                        {
                            if(CurrentImeMode == ImeIndicatorMode.Disabled || CurrentImeMode == ImeIndicatorMode.Manual)
                                SendKeys.Send("^ ");
                            //SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                            SendKeys.Send("+"); //将CapsLock转换为Shift
                        }
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.JAP)
                        {
                            SendKeys.Send("+{CAPSLOCK}"); //将CapsLock转换为Shift+CapsLock
                        }

                        if (CapsLockLight) CapsLockLightAuto();
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(ex.Message);
                        //throw new System.ComponentModel.Win32Exception();
                    }
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private static HookProc _keyboardProc = LowLevelKeyboardProc;

        private static IntPtr SetKeyBoardHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }
        #endregion

        ~KeyMapper()
        {
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
            }
        }

        public void SetupHook()
        {
            // Console.CapsLock
            // WinForm: Control.IsKeyLocked(Keys.CapsLock) 
            // WPF: Keyboard.IsKeyToggled(Key.CapsLock)
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                ToggleCapsLock();
            }

            try
            {
                _keyboardHookID = SetKeyBoardHook(_keyboardProc);
                if (_keyboardHookID == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
