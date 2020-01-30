using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    public enum SysKeyboardLayout { ENG = 1033, CHS = 2052, CHT = 1028, CHK = 3076, JAP = 1041, KOR = 1042 };

    public class KeyMapper
    {
        static public bool CapsLockLightEnabled { get; set; } = false;
        static public bool CapsLockLightAutoCheck { get; set; } = false;
        static public bool AutoCloseKeePassIME { get; set; } = false;
        static public Keys KeePassHotKey { get; set; } = Keys.None;

        static public SysKeyboardLayout CurrentKeyboardLayout { get; set; } = SysKeyboardLayout.ENG;
        static private ImeIndicatorMode currentImeMode = ImeIndicatorMode.Disabled;
        static public ImeIndicatorMode CurrentImeMode
        {
            get
            {
                return (currentImeMode);
            }
            set
            {
                currentImeMode = value;
                if (CapsLockLightAutoCheck) CapsLockLightAuto();
            }
        }

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
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct UNICODE_STRING : IDisposable
        {
            public ushort Length;
            public ushort MaximumLength;
            private IntPtr buffer;

            public UNICODE_STRING(string s)
            {
                Length = (ushort)(s.Length * 2);
                MaximumLength = (ushort)(Length + 2);
                buffer = Marshal.StringToHGlobalUni(s);
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(buffer);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_ATTRIBUTES : IDisposable
        {
            public int Length;
            public IntPtr RootDirectory;
            private IntPtr objectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;

            public OBJECT_ATTRIBUTES(string name, uint attrs)
            {
                Length = 0;
                RootDirectory = IntPtr.Zero;
                objectName = IntPtr.Zero;
                Attributes = attrs;
                SecurityDescriptor = IntPtr.Zero;
                SecurityQualityOfService = IntPtr.Zero;

                Length = Marshal.SizeOf(this);
                ObjectName = new UNICODE_STRING(name);
            }

            public UNICODE_STRING ObjectName
            {
                get
                {
                    return (UNICODE_STRING)Marshal.PtrToStructure(
                     objectName, typeof(UNICODE_STRING));
                }

                set
                {
                    bool fDeleteOld = objectName != IntPtr.Zero;
                    if (!fDeleteOld)
                        objectName = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                    Marshal.StructureToPtr(value, objectName, fDeleteOld);
                }
            }

            public void Dispose()
            {
                if (objectName != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(objectName, typeof(UNICODE_STRING));
                    Marshal.FreeHGlobal(objectName);
                    objectName = IntPtr.Zero;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_STATUS_BLOCK
        {
            internal uint Status;
            internal IntPtr Information;
        }

        [Flags]
        public enum ACCESS_MASK : uint
        {
            //
            //  The following are masks for the predefined standard access types
            //

            DELETE                   = 0x00010000,
            READ_CONTROL             = 0x00020000,
            WRITE_DAC                = 0x00040000,
            WRITE_OWNER              = 0x00080000,
            SYNCHRONIZE              = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000F0000,

            STANDARD_RIGHTS_READ     = 0x00020000,
            STANDARD_RIGHTS_WRITE    = 0x00020000,
            STANDARD_RIGHTS_EXECUTE  = 0x00020000,

            STANDARD_RIGHTS_ALL      = 0x001F0000,

            SPECIFIC_RIGHTS_ALL      = 0x0000FFFF,

            //
            // AccessSystemAcl access type
            //

            ACCESS_SYSTEM_SECURITY   = 0x01000000,

            //
            // MaximumAllowed access type
            //

            MAXIMUM_ALLOWED          = 0x02000000,

            //
            //  These are the generic rights.
            //

            GENERIC_READ             = 0x80000000,
            GENERIC_WRITE            = 0x40000000,
            GENERIC_EXECUTE          = 0x20000000,
            GENERIC_ALL              = 0x10000000,

            DESKTOP_READOBJECTS      = 0x00000001,
            DESKTOP_CREATEWINDOW     = 0x00000002,
            DESKTOP_CREATEMENU       = 0x00000004,
            DESKTOP_HOOKCONTROL      = 0x00000008,
            DESKTOP_JOURNALRECORD    = 0x00000010,
            DESKTOP_JOURNALPLAYBACK  = 0x00000020,
            DESKTOP_ENUMERATE        = 0x00000040,
            DESKTOP_WRITEOBJECTS     = 0x00000080,
            DESKTOP_SWITCHDESKTOP    = 0x00000100,

            WINSTA_ENUMDESKTOPS      = 0x00000001,
            WINSTA_READATTRIBUTES    = 0x00000002,
            WINSTA_ACCESSCLIPBOARD   = 0x00000004,
            WINSTA_CREATEDESKTOP     = 0x00000008,
            WINSTA_WRITEATTRIBUTES   = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS       = 0x00000040,
            WINSTA_ENUMERATE         = 0x00000100,
            WINSTA_READSCREEN        = 0x00000200,

            WINSTA_ALL_ACCESS        = 0x0000037F,


            FILE_ANY_ACCESS           = 0x00000000, // any type

            FILE_READ_ACCESS          = 0x00000001, // file & pipe

            FILE_READ_DATA            = 0x00000001, // file & pipe
            FILE_LIST_DIRECTORY       = 0x00000001, // directory

            FILE_WRITE_ACCESS         = 0x00000002, // file & pipe

            FILE_WRITE_DATA           = 0x00000002, // file & pipe
            FILE_ADD_FILE             = 0x00000002, // directory

            FILE_APPEND_DATA          = 0x00000004, // file
            FILE_ADD_SUBDIRECTORY     = 0x00000004, // directory
            FILE_CREATE_PIPE_INSTANCE = 0x00000004, // named pipe

            FILE_READ_EA              = 0x00000008, // file & directory

            FILE_WRITE_EA             = 0x00000010, // file & directory

            FILE_EXECUTE              = 0x00000020, // file
            FILE_TRAVERSE             = 0x00000020, // directory

            FILE_DELETE_CHILD         = 0x00000040, // directory

            FILE_READ_ATTRIBUTES      = 0x00000080, // all types

            FILE_WRITE_ATTRIBUTES     = 0x00000100, // all types
            
            FILE_ALL_ACCESS           = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF, // All of the preceding +

            FILE_GENERIC_READ         = STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE,

            FILE_GENERIC_WRITE        = STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE,

            FILE_GENERIC_EXECUTE      = STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE,
            
        }

        [Flags]
        public enum CREATE_OPTION : uint
        {
            FILE_DIRECTORY_FILE                    = 0x00000001,
            FILE_WRITE_THROUGH                     = 0x00000002,
            FILE_SEQUENTIAL_ONLY                   = 0x00000004,
            FILE_NO_INTERMEDIATE_BUFFERING         = 0x00000008,

            FILE_SYNCHRONOUS_IO_ALERT              = 0x00000010,
            FILE_SYNCHRONOUS_IO_NONALERT           = 0x00000020,
            FILE_NON_DIRECTORY_FILE                = 0x00000040,
            FILE_CREATE_TREE_CONNECTION            = 0x00000080,

            FILE_COMPLETE_IF_OPLOCKED              = 0x00000100,
            FILE_NO_EA_KNOWLEDGE                   = 0x00000200,
            FILE_OPEN_REMOTE_INSTANCE              = 0x00000400,
            FILE_RANDOM_ACCESS                     = 0x00000800,

            FILE_DELETE_ON_CLOSE                   = 0x00001000,
            FILE_OPEN_BY_FILE_ID                   = 0x00002000,
            FILE_OPEN_FOR_BACKUP_INTENT            = 0x00004000,
            FILE_NO_COMPRESSION                    = 0x00008000,

            FILE_OPEN_REQUIRING_OPLOCK             = 0x00010000,
            FILE_DISALLOW_EXCLUSIVE                = 0x00020000,

            FILE_RESERVE_OPFILTER                  = 0x00100000,
            FILE_OPEN_REPARSE_POINT                = 0x00200000,
            FILE_OPEN_NO_RECALL                    = 0x00400000,
            FILE_OPEN_FOR_FREE_SPACE_QUERY         = 0x00800000,


            FILE_VALID_OPTION_FLAGS                = 0x00FFFFFF,
            FILE_VALID_PIPE_OPTION_FLAGS           = 0x00000032,
            FILE_VALID_MAILSLOT_OPTION_FLAGS       = 0x00000032,
            FILE_VALID_SET_FLAGS                   = 0x00000036,
        }

        [DllImport("ntdll.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ZwCreateFile(
            out SafeFileHandle handle,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            ref OBJECT_ATTRIBUTES objectAttributes,
            ref IO_STATUS_BLOCK ioStatus,
            ref long allocSize,
            uint fileAttributes,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            uint createDisposition,
            uint createOptions,
            IntPtr eaBuffer,
            uint eaLength);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        private static extern int NtCreateFile(
            out SafeFileHandle handle,
            ACCESS_MASK access,
            ref OBJECT_ATTRIBUTES objectAttributes,
            ref IO_STATUS_BLOCK ioStatus,
            ref long allocSize,
            uint fileAttributes,
            FileShare share,
            uint createDisposition,
            CREATE_OPTION createOptions,
            IntPtr eaBuffer,
            uint eaLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll")]
        private static extern bool DefineDosDevice(uint dwFlags, string lpDeviceName, string lpTargetPath);

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
            ScrollLock = 1,
            NumLock = 2,
            CapsLock = 4
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
        private static uint IOCTL_KEYBOARD_QUERY_TYPEMATIC = ControlCode(FileDeviceKeyboard, 0x0008, MethodBuffered, FileAnyAccess);
        private static uint IOCTL_KEYBOARD_QUERY_INDICATORS = ControlCode(FileDeviceKeyboard, 0x0010, MethodBuffered, FileAnyAccess);

        internal static SafeFileHandle NtCreateFile(string kbName)
        {
            SafeFileHandle result = new SafeFileHandle(IntPtr.Zero, true);

            try
            {
                long allocSize = 0;
                uint FILE_OPEN = 0x1;
                uint OBJ_CASE_INSENSITIVE = 0x40;
                OBJECT_ATTRIBUTES objAttributes = new OBJECT_ATTRIBUTES(kbName, OBJ_CASE_INSENSITIVE); //InitializeObjectAttributes();
                IO_STATUS_BLOCK ioStatusBlock = new IO_STATUS_BLOCK();

                var ret = NtCreateFile(
                    out result,
                    ACCESS_MASK.FILE_ANY_ACCESS | ACCESS_MASK.FILE_READ_ATTRIBUTES | ACCESS_MASK.FILE_WRITE_ATTRIBUTES | ACCESS_MASK.SYNCHRONIZE,
                    ref objAttributes,
                    ref ioStatusBlock,
                    ref allocSize,
                    0,
                    FileShare.Read,
                    FILE_OPEN,
                    CREATE_OPTION.FILE_NON_DIRECTORY_FILE | CREATE_OPTION.FILE_SYNCHRONOUS_IO_NONALERT,
                    IntPtr.Zero,
                    0);

                var err = Marshal.GetLastWin32Error();
            }
            finally
            {
            }

            return (result);
        }
        #endregion

        public static List<string> GetKeyboardList()
        {
            List<string> result = new List<string>();

            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\KeyboardClass", false))
                {
                    var names = rk.GetValueNames();
                    result = names.ToList();
                    rk.Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("RegScan Failed, using default names" + exception.Message);
                for(int i = 0; i < 16; i++)
                {
                    result.Add($"\\Device\\KeyboardClass{i}");
                }
            }

            return (result);
        }

        public static bool SetLights(int idx, LockLights locks, LockLightsOp op = LockLightsOp.Toggle)
        {
            bool result = false;

            DefineDosDevice(DddRawTargetPath, $"keyboard{idx}", $"\\Device\\KeyboardClass{idx}");

            var indicatorsIn = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };
            var indicatorsOut = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };

            //using (var hKeybd = NtCreateFile("\\\\.\\keyboard"))
            using (var hKeybd = CreateFile($"\\\\.\\keyboard{idx}", FileAccess.Write, 0, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
            {
                if (!hKeybd.IsInvalid)
                {
                    var size = Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
                    int bytesReturned = 0;

                    if (DeviceIoControl(hKeybd, (int)IOCTL_KEYBOARD_QUERY_INDICATORS, ref indicatorsOut, size, ref indicatorsOut, size, out bytesReturned, IntPtr.Zero))
                    {
                        if (op == LockLightsOp.Toggle)
                        {
                            if (indicatorsOut.LedFlags.HasFlag(locks))
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
                        result = retS;
                    }
                }
            }

            return (result);
        }

        public static bool SetKeyboardLights(LockLights locks, LockLightsOp op = LockLightsOp.Toggle)
        {
            bool result = false;

            //return (SetLights(locks, op));

            var indicatorsIn = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };
            var indicatorsOut = new KeyboardIndicatorParameters() { UnitId = 0, LedFlags = LockLights.None };

            var kbds = GetKeyboardList();

            //int start = 0;
            //if (SetLights(locks, op)) start = 1;
            //for (int i = start; i < 16; i++)
            foreach (var kbd in kbds)
            {
                //using (var hKeybd = NtCreateFile($"\\Device\\KeyboardClass{i}"))
                using (var hKeybd = NtCreateFile(kbd))
                {
                    if (!hKeybd.IsInvalid)
                    {
                        var size = Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
                        int bytesReturned = 0;

                        if (DeviceIoControl(hKeybd, (int)IOCTL_KEYBOARD_QUERY_INDICATORS,
                                            ref indicatorsOut, size,
                                            ref indicatorsOut, size,
                                            out bytesReturned,
                                            IntPtr.Zero))
                        {
                            if (op == LockLightsOp.Toggle)
                            {
                                if (indicatorsOut.LedFlags.HasFlag(locks))
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
                            var retS = DeviceIoControl(hKeybd, (int)IOCTL_KEYBOARD_SET_INDICATORS,
                                                        ref indicatorsIn, size,
                                                        ref indicatorsOut, size,
                                                        out bytesReturned,
                                                        IntPtr.Zero);
                            result = retS || result;
                        }
                    }
                }
            }

            return (result);
        }

        public static void CapsLockLightOn()
        {
            //SetLights(LockLights.CapsLock, LockLightsOp.On);
            SetKeyboardLights(LockLights.CapsLock, LockLightsOp.On);
        }

        public static void CapsLockLightOff()
        {
            //SetLights(LockLights.CapsLock, LockLightsOp.Off);
            SetKeyboardLights(LockLights.CapsLock, LockLightsOp.Off);
        }

        public static void CapsLockLightToggle()
        {
            //SetLights(LockLights.CapsLock, LockLightsOp.Toggle);
            SetKeyboardLights(LockLights.CapsLock, LockLightsOp.Toggle);
        }

        public static void CapsLockLightAuto()
        {
            if(CurrentKeyboardLayout != SysKeyboardLayout.ENG)
            {
#if DEBUG
                //Console.WriteLine($"{CurrentKeyboardLayout}:{CurrentImeMode}");
#endif
                if (CurrentImeMode == ImeIndicatorMode.Locale)
                    CapsLockLightOn();
                else
                    CapsLockLightOff();
            }
        }

        #region Keyboard event for toggle capslock state
        [DllImport("user32.dll",
            CharSet = CharSet.Auto,
            ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        #endregion

        public static bool CapsLockState()
        {
            bool CapsLock = (((ushort) GetKeyState(0x14)) & 0xffff) != 0;
            //bool NumLock = (((ushort) GetKeyState(0x90)) & 0xffff) != 0;
            //bool ScrollLock = (((ushort) GetKeyState(0x91)) & 0xffff) != 0;
            return (CapsLock);
        }

        public static void ToggleCapsLock()
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        public static void CloseCapsLock()
        {
            if (CapsLockState())
            {
                ToggleCapsLock();
            }
        }

        public static void OpenCapsLock()
        {
            if (!CapsLockState())
            {
                ToggleCapsLock();
            }
        }

        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYUP = 0x2;
        public static void ToggleCtrlSpace()
        {
            //SendKeys.SendWait("^");
            keybd_event(0x11, 0, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x20, 0, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x20, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            keybd_event(0x11, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }
        
        public static void SendKey(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        #region Keyboard Hook
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private static SysKeyboardLayout[] CapsLockEnabledLayout = new SysKeyboardLayout[] {
            SysKeyboardLayout.CHS,
            SysKeyboardLayout.CHT,
            SysKeyboardLayout.CHK,
            SysKeyboardLayout.JAP,
            SysKeyboardLayout.KOR
        };

        private static IntPtr _keyboardHookID = IntPtr.Zero;

        private static int last_keydown = 0;
        private static int count_keydown = 0;
        private static IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN) && CurrentImeMode != ImeIndicatorMode.Disabled && CapsLockEnabledLayout.Contains(CurrentKeyboardLayout))
            {
                var kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vkCode = kbd.vkCode; //Marshal.ReadInt32(lParam);
                int timestamp = kbd.time;
                if (last_keydown == 0) last_keydown = timestamp - 31;

                if (kbd.flags == 0 && (Keys)vkCode == Keys.Capital)
                {
                    var diff = timestamp - last_keydown;
#if DEBUG
                    Console.WriteLine($"KeyDown: {vkCode}, {kbd.scanCode}, {kbd.flags}, {kbd.dwExtraInfo}, {last_keydown}/{timestamp}, {timestamp - last_keydown}, {CurrentKeyboardLayout}, {wParam}, {count_keydown}");
#endif
                    last_keydown = timestamp;

                    if ( diff <= 63 || diff == 500)
                        count_keydown++;
                    else
                        count_keydown = 0;

                    if (count_keydown > 1 && diff == 500)
                    {
                        //ToggleCapsLock();
                        //OpenCapsLock();
                        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                        //return (IntPtr)1;
                    }

                    if (count_keydown < 1)
                    {
                        //if (diff == 500)
                        //{
                        //    //count_keydown++;
                        //    return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                        //}

                        try
                        {
                            if (CurrentKeyboardLayout == SysKeyboardLayout.CHS)
                            {
                                SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                            }
                            else if (CurrentKeyboardLayout == SysKeyboardLayout.CHT)
                            {
                                if (CurrentImeMode == ImeIndicatorMode.Disabled || CurrentImeMode == ImeIndicatorMode.Close)
                                    SendKeys.Send("^ "); //Open Input
                                SendKeys.Send("+");      //将CapsLock转换为Shift
                            }
                            else if (CurrentKeyboardLayout == SysKeyboardLayout.CHK)
                            {
                                SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                            }
                            else if (CurrentKeyboardLayout == SysKeyboardLayout.JAP)
                            {
                                SendKeys.Send("+{CAPSLOCK}"); //将CapsLock转换为Shift+CapsLock
                            }
                            else if (CurrentKeyboardLayout == SysKeyboardLayout.KOR)
                            {
                                SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                            }

                            if (CapsLockLightEnabled)
                            {
                                if (CapsLockLightAutoCheck) CapsLockLightAuto();
                                else CapsLockLightToggle();
                            }
                        }
                        catch (Exception)
                        {
                            //MessageBox.Show(ex.Message);
                            //throw new System.ComponentModel.Win32Exception();
                        }
                        return (IntPtr)1;
                    }
                }
                else if (AutoCloseKeePassIME && CurrentImeMode == ImeIndicatorMode.Locale && kbd.flags == 0 && (Keys)vkCode == KeePassHotKey)
                {
                    try
                    {
                        if (CurrentKeyboardLayout == SysKeyboardLayout.CHS)
                            SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.CHT)
                            SendKeys.Send("+");      //将CapsLock转换为Shift
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.CHK)
                            SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.JAP)
                            SendKeys.Send("+{CAPSLOCK}"); //将CapsLock转换为Shift+CapsLock
                        else if (CurrentKeyboardLayout == SysKeyboardLayout.KOR)
                            SendKeys.Send("^ "); //将CapsLock转换为Ctrl+Space

                        if (CapsLockLightEnabled)
                        {
                            if (CapsLockLightAutoCheck) CapsLockLightAuto();
                            else CapsLockLightOff();
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(ex.Message);
                        //throw new System.ComponentModel.Win32Exception();
                    }
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
