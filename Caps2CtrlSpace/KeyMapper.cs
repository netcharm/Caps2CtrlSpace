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
    public class KeyMapper
    {
        static public bool CapsLockLight { get; set; } = false;

        static public uint CurrentKeyboardLayout { get; set; } = 1033;

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
        static extern bool DefineDosDevice(uint dwFlags, string lpDeviceName, string lpTargetPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateFileA(
             [MarshalAs(UnmanagedType.LPStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("Kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
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
        public enum Locks : ushort
        {
            None = 0,
            KeyboardScrollLockOn = 1,
            KeyboardNumLockOn = 2,
            KeyboardCapsLockOn = 4
        }

        public struct KeyboardIndicatorParameters
        {
            public ushort UnitId;
            public Locks LedFlags;
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

        public static void ToggleLights(Locks locks)
        {
            DefineDosDevice(DddRawTargetPath, "keyboard", "\\Device\\KeyboardClass0");

            var indicatorsIn = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = Locks.None };
            var indicatorsOut = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = Locks.None };

            //using (var hKeybd = CreateFile("\\\\.\\keyboard", FileAccess.Write, 0, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
            {
                var hKeybd = CreateFile("\\\\.\\keyboard", FileAccess.Write, 0, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                var size = Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
                int bytesReturned = 0;

                var retQ = DeviceIoControl(new SafeFileHandle(hKeybd, false), (int)IOCTL_KEYBOARD_QUERY_INDICATORS, ref indicatorsOut, size, ref indicatorsOut, size, out bytesReturned, IntPtr.Zero);
                if (retQ)
                {
                    if (!indicatorsOut.LedFlags.HasFlag(Locks.KeyboardCapsLockOn))
                        indicatorsIn.LedFlags |= Locks.KeyboardCapsLockOn;
                    if (indicatorsOut.LedFlags.HasFlag(Locks.KeyboardNumLockOn))
                        indicatorsIn.LedFlags |= Locks.KeyboardNumLockOn;
                    if (indicatorsOut.LedFlags.HasFlag(Locks.KeyboardScrollLockOn))
                        indicatorsIn.LedFlags |= Locks.KeyboardScrollLockOn;
                    var retS = DeviceIoControl(new SafeFileHandle(hKeybd, false), (int)IOCTL_KEYBOARD_SET_INDICATORS, ref indicatorsIn, size, ref indicatorsOut, size, out bytesReturned, IntPtr.Zero);
                }
            }
        }
        #endregion

        #region Keyboard event for toggle capslock state
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public static void ToggleCapsLock()
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }
        #endregion

        #region Keyboard Hook
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private static uint[] CapsLockEnabledLayout = new uint[] { 2052, 1028, 1041 };

        private static IntPtr _keyboardHookID = IntPtr.Zero;

        private static IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == Keys.Capital && CapsLockEnabledLayout.Contains(CurrentKeyboardLayout))
                {
                    if(CurrentKeyboardLayout == 2052 || CurrentKeyboardLayout == 1028)
                        SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                    else if(CurrentKeyboardLayout == 1041)
                        SendKeys.Send("+{CAPSLOCK}"); //将CapsLock转换为Ctrl+Space

                    if (CapsLockLight) ToggleLights(Locks.KeyboardCapsLockOn);

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
