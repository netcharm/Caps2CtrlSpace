using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caps2CtrlSpace
{
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public class ImeIndicator
    {
        public Bitmap English { get; set; } = null;
        public Bitmap Locale { get; set; } = null;
        public Bitmap Disabled { get; set; } = null;
    }

    public enum ImeIndicatorMode { Manual, English, Locale, Disabled };

    class Ime
    {
        private static string AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().EscapedCodeBase).Replace("file:\\", "");

        #region get current input window keyboard layout functions
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetKeyboardLayout(uint thread);
        #endregion

        private static Dictionary<int, string> InstalledKeyboardLayout = new Dictionary<int, string>();
        private static int lastKeyboardLayout = (int)Caps2CtrlSpace.KeyboardLayout.ENG;
        private static int lastWindowHandle = 0;

        private Tuple<int, int> GetFocusKeyboardLayout()
        {
            IntPtr activeWindowHandle = GetForegroundWindow();
            IntPtr activeWindowThread = GetWindowThreadProcessId(activeWindowHandle, IntPtr.Zero);
            //IntPtr thisWindowThread = GetWindowThreadProcessId(this.Handle, IntPtr.Zero);

            //AttachThreadInput(activeWindowThread, thisWindowThread, true);
            IntPtr focusedControlHandle = GetFocus();
            var kl = GetKeyboardLayout((uint)activeWindowThread.ToInt32()).ToInt32() & 0xFFFF;
            //AttachThreadInput(activeWindowThread, thisWindowThread, false);
            return (new Tuple<int, int>(focusedControlHandle.ToInt32(), kl));
        }

        private static Dictionary<string, ImeIndicator> ImeIndicators = new Dictionary<string, ImeIndicator>();
        public static Bitmap CurrentImeIndicator { get; set; } = null;

        public static int KeyboardLayout
        {
            get
            {
                return (GetKeyboardLayout());
            }
        }
        public static string KeyboardLayoutName
        {
            get
            {
                if (InstalledKeyboardLayout.ContainsKey(lastKeyboardLayout))
                    return (InstalledKeyboardLayout[lastKeyboardLayout]);
                else
                    return string.Empty;
            }
        }
        public static ImeIndicatorMode Mode
        {
            get
            {
                return (GetImeMode(lastKeyboardLayout));
            }
        }

        public static IntPtr ActiveWindowHandle { get; private set; } = IntPtr.Zero;
        public static string ActiveWindowTitle
        {
            get
            {
                string title = string.Empty;
                int length = GetWindowTextLength(ActiveWindowHandle);
                if (length > 0)
                {
                    StringBuilder sb = new StringBuilder(length);
                    GetWindowText(ActiveWindowHandle, sb, length + 1);
                    title = sb.ToString();
                }
                return (title);
            }
        }

        public static IntPtr GetImeModeButtonHandle()
        {
            IntPtr result = IntPtr.Zero;

            IntPtr hWnd = FindWindow("Shell_TrayWnd", null);
            if(hWnd != IntPtr.Zero)
            {
                IntPtr hTray = FindWindowEx(hWnd, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hTray != IntPtr.Zero)
                {
                    IntPtr hInput = FindWindowEx(hTray, IntPtr.Zero, "TrayInputIndicatorWClass", null);
                    if (hInput != IntPtr.Zero)
                    {
                        IntPtr hIme = FindWindowEx(hInput, IntPtr.Zero, "IMEModeButton", null);
                        result = hIme;
                    }
                }
            }

            return (result);
        }

        public static Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;
            int a = 255;

            for (int y = 0; y < Bmp.Height; y++)
            {
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    a = (c.R == 0 && c.G == 0 && c.B == 0) ? 0 : c.A;
                    //a = (c.R < 0x80 && c.G < 0x80 && c.B < 0x80) ? 0 : c.A;
                    //a = (c.R < 0x40 && c.G < 0x40 && c.B < 0x40) ? 0 : c.A;
                    //Bmp.SetPixel(x, y, Color.FromArgb(a, c.R, c.G, c.B));

                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    a = rgb<0x80 ? 0 : c.A;
                    Bmp.SetPixel(x, y, Color.FromArgb(a, rgb, rgb, rgb));
                }
            }
            return ((Bitmap)Bmp.Clone());
        }

        public static Bitmap GetSanpshot(IntPtr hWnd)
        {
            Bitmap result = null;

            Rect rect = new Rect();
            GetWindowRect(hWnd, out rect);

            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;
            if (w > 0 && h > 0)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.Transparent, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    IntPtr hdc;
                    try
                    {
                        hdc = g.GetHdc();
                        bool succeeded = PrintWindow(hWnd, hdc, 0);
                        g.ReleaseHdc(hdc);
                        if (succeeded)
                        {
                            //g.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(Point.Empty, bmp.Size));
                            result = ToGrayScale(bmp);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return (result);
        }

        public static Bitmap GetImeModeBitmap()
        {
            var hWnd = GetImeModeButtonHandle();
            var img = GetSanpshot(hWnd);
            return(img);
        }

        public static Bitmap LoadBitmap(string bitmapFile)
        {
            Bitmap result = null;
            if (File.Exists(bitmapFile))
            {
                using (var ms = new FileStream(bitmapFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    result = (Bitmap)Image.FromStream(ms);
                }
            }
            return (result);
        }

        public static bool CompareBitmap(Bitmap src, Bitmap dst)
        {
            bool result = false;

            if (src is Bitmap && dst is Bitmap)
            {
                if (src.Width == dst.Width && src.Height == dst.Height)
                {
                    //result = true;
                    int count = 0;

                    var bSrc = ToGrayScale(src);
                    var bDst = ToGrayScale(dst);

                    Color cSrc, cDst;

                    for (int y = 0; y < bSrc.Height; y++)
                    {
                        for (int x = 0; x < bSrc.Width; x++)
                        {
                            cSrc = bSrc.GetPixel(x, y);
                            cDst = bDst.GetPixel(x, y);
                            if (cSrc.A == 0 && cDst.A == 0) continue;
                            if (cSrc.R != cDst.R || cSrc.G != cDst.G || cSrc.B != cDst.B)
                            {
                                //result = false;
                                //break;
                                count++;
                            }
                        }
                    }
                    if ((double)count / (src.Width * src.Height) < 0.1) result = true;
                }
            }
            return (result);
        }

        private static int GetKeyboardLayout()
        {
            int result = 0;

            //var klt = GetFocusKeyboardLayout();
            IntPtr activeWindowHandle = GetForegroundWindow();
            IntPtr activeWindowThread = GetWindowThreadProcessId(activeWindowHandle, IntPtr.Zero);
            var kl = GetKeyboardLayout((uint)activeWindowThread.ToInt32()).ToInt32() & 0xFFFF;

            if (InstalledKeyboardLayout.Count <= 0 || !InstalledKeyboardLayout.ContainsKey(kl))
            {
                InputLanguageCollection ilc = InputLanguage.InstalledInputLanguages; //获取所有安装的输入法
                foreach (InputLanguage il in ilc)
                {
                    InstalledKeyboardLayout.Add(il.Culture.KeyboardLayoutId, il.LayoutName);
                }
            }
            //InputLanguage cil = InputLanguage.CurrentInputLanguage; //获取当前UI线程的输入法以及状态
            if (InstalledKeyboardLayout.ContainsKey(kl))
            {
                //if (kl != lastKeyboardLayout)
                {
                    if (Control.IsKeyLocked(Keys.CapsLock))
                    {
                        KeyMapper.ToggleCapsLock();
                    }

                    if (lastWindowHandle == activeWindowHandle.ToInt32() && kl == (int)Caps2CtrlSpace.KeyboardLayout.CHT)
                    {
                        //KeyMapper.ToggleCtrlSpace();
                    }
                    result = kl;
                    lastKeyboardLayout = kl;
                    ActiveWindowHandle = activeWindowHandle;
                }
            }
            return (result);
        }

        private static ImeIndicatorMode GetImeMode(int KeyboardLayout)
        {
            ImeIndicatorMode result = ImeIndicatorMode.Manual;

            if (!ImeIndicators.ContainsKey($"{KeyboardLayout}"))
            {
                ImeIndicators[$"{KeyboardLayout}"] = new ImeIndicator();
                ImeIndicators[$"{KeyboardLayout}"].English = LoadBitmap(Path.Combine(AppPath, $"{KeyboardLayout}_0.png"));
                ImeIndicators[$"{KeyboardLayout}"].Locale = LoadBitmap(Path.Combine(AppPath, $"{KeyboardLayout}_1.png"));
                ImeIndicators[$"{KeyboardLayout}"].Disabled = LoadBitmap(Path.Combine(AppPath, $"{KeyboardLayout}_2.png"));
            }

            //if(ImeIndicators[$"{KeyboardLayout}"].Locale is Bitmap && ImeIndicators[$"{KeyboardLayout}"].English is Bitmap)
            if (ImeIndicators[$"{KeyboardLayout}"].Locale is Bitmap)
            {
                CurrentImeIndicator = GetImeModeBitmap();
                if (CompareBitmap(CurrentImeIndicator, ImeIndicators[$"{KeyboardLayout}"].Locale))
                    result = ImeIndicatorMode.Locale;
                else if (CompareBitmap(CurrentImeIndicator, ImeIndicators[$"{KeyboardLayout}"].English))
                    result = ImeIndicatorMode.English;
                else if (CompareBitmap(CurrentImeIndicator, ImeIndicators[$"{KeyboardLayout}"].Disabled))
                    result = ImeIndicatorMode.Disabled;
            }

            return (result);
        }
    }
}
