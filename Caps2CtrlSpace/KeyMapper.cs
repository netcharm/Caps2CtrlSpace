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
        #region Import low level keyboard hook functions
        //[DllImport("user32.dll")]
        //static extern IntPtr GetForegroundWindow();
        //[DllImport("user32.dll")]
        //static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
        //[DllImport("user32.dll")]
        //static extern IntPtr GetKeyboardLayout(uint thread);
        //public static CultureInfo GetCurrentKeyboardLayout()
        //{
        //    try
        //    {
        //        IntPtr foregroundWindow = GetForegroundWindow();
        //        uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
        //        var layout = GetKeyboardLayout(foregroundProcess);
        //        Console.WriteLine(layout);
        //        int keyboardLayout = layout.ToInt32() & 0xFFFF;
        //        return new CultureInfo(keyboardLayout);
        //    }
        //    catch (Exception _)
        //    {
        //        return new CultureInfo(1033); // Assume English if something went wrong.
        //    }
        //}

        //[DllImport("imm32.dll")]
        //public static extern IntPtr ImmGetContext(IntPtr hWnd);

        //[DllImport("Imm32.dll")]
        //public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        //[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        //private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

        //[DllImport("Imm32.dll")]
        //public static extern bool ImmGetOpenStatus(IntPtr hIMC);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region Import keyboard indication light functions
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
        #endregion

        #region Import keyboard event for setting capslock state
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

        static private bool capslocklight = false;
        static public bool CapsLockLight
        {
            get
            {
                return (capslocklight);
            }
            set
            {
                capslocklight = value;
            }
        }

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == Keys.Capital)
                {
                    SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                    if(capslocklight)
                        ToggleLights(Locks.KeyboardCapsLockOn);
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        ~KeyMapper()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
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

            _hookID = SetHook(_proc);
        }

    }
}
