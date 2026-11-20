using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using OverlappedPresenter = Microsoft.UI.Windowing.OverlappedPresenter;

namespace RustMeTweaker;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    private const uint WM_NULL = 0x0000;
    private const uint WM_CONTEXTMENU = 0x007B;
    private const uint WM_TRAYICON = 0x0401;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_LBUTTONDBLCLK = 0x0203;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint LR_LOADFROMFILE = 0x00000010;
    private const uint LR_DEFAULTSIZE = 0x00004000;
    private const uint IMAGE_ICON = 0x00000001;
    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint TPM_NONOTIFY = 0x0080;
    private const uint TPM_RETURNCMD = 0x0100;
    private const uint CMD_OPEN = 1;
    private const uint CMD_EXIT = 2;
    private const int GWLP_WNDPROC = -4;

    private IntPtr _hwnd;
    private IntPtr _origWndProc;
    private WndProcDelegate? _wndProcDelegate;
    private IntPtr _trayIconHandle;
    private bool _trayShown;
    private bool _crunkOn;
    private bool _exitRequested;

    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam);

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;

        AppWindow.SetIcon("Assets/AppIcon.ico");

        MainViewModel.AutoCrunkToggled += OnAutoCrunk;
        if (MainViewModel.CrunkOn)
        {
            OnAutoCrunk(true);
        }
        AppWindow.Closing += (sender, args) =>
        {
            if (_exitRequested || !_crunkOn)
            {
                return;
            }
            args.Cancel = true;
            AppWindow.Hide();
        };
        Closed += (_, _) =>
        {
            RemoveTray();
            MainViewModel.Current?.ShutdownCrunk();
        };

        if (AppWindow.Presenter is not OverlappedPresenter p) return;

        p.PreferredMinimumWidth = 1000;
        p.PreferredMinimumHeight = 600;

        AppWindow.Resize(new SizeInt32(1000, 800));

        RootFrame.Navigate(typeof(MainPage));
    }

    private void OnAutoCrunk(bool on)
    {
        _crunkOn = on;
        if (on)
        {
            ShowTray();
            AppWindow.Hide();
        }
        else
        {
            RemoveTray();
        }
    }

    private void ShowTray()
    {
        if (_trayShown)
        {
            return;
        }
        _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _wndProcDelegate = WndProc;
        _origWndProc = GetWindowLongPtrW(_hwnd, GWLP_WNDPROC);
        SetWindowLongPtrW(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        _trayIconHandle = LoadImageW(
            IntPtr.Zero,
            Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"),
            IMAGE_ICON,
            0,
            0,
            LR_LOADFROMFILE | LR_DEFAULTSIZE
        );
        NOTIFYICONDATAW data = BuildData();
        Shell_NotifyIconW(NIM_ADD, ref data);
        _trayShown = true;
    }

    private void RemoveTray()
    {
        if (!_trayShown)
        {
            return;
        }
        NOTIFYICONDATAW data = BuildData();
        Shell_NotifyIconW(NIM_DELETE, ref data);
        if (_trayIconHandle != IntPtr.Zero)
        {
            DestroyIcon(_trayIconHandle);
        }
        _trayIconHandle = IntPtr.Zero;
        if (_origWndProc != IntPtr.Zero)
        {
            SetWindowLongPtrW(_hwnd, GWLP_WNDPROC, _origWndProc);
        }
        _origWndProc = IntPtr.Zero;
        _trayShown = false;
    }

    private void RestoreWindow()
    {
        AppWindow.Show();
        Activate();
    }

    private void ExitApp()
    {
        MainViewModel.Current?.ShutdownCrunk();
        _exitRequested = true;
        RemoveTray();
        Close();
    }

    private void ShowTrayMenu()
    {
        SetForegroundWindow(_hwnd);
        GetCursorPos(out POINT pt);
        IntPtr menu = CreatePopupMenu();
        AppendMenuW(menu, MF_STRING, (UIntPtr)CMD_OPEN, "Открыть");
        AppendMenuW(menu, MF_SEPARATOR, UIntPtr.Zero, null);
        AppendMenuW(menu, MF_STRING, (UIntPtr)CMD_EXIT, "Закрыть");
        int cmd = TrackPopupMenu(menu, TPM_RIGHTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);
        DestroyMenu(menu);
        PostMessageW(_hwnd, WM_NULL, UIntPtr.Zero, IntPtr.Zero);
        if ((uint)cmd == CMD_OPEN)
        {
            RestoreWindow();
        }
        else if ((uint)cmd == CMD_EXIT)
        {
            ExitApp();
        }
    }

    private NOTIFYICONDATAW BuildData()
    {
        NOTIFYICONDATAW data = new();
        data.cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>();
        data.hWnd = _hwnd;
        data.uID = 1;
        data.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        data.uCallbackMessage = WM_TRAYICON;
        data.hIcon = _trayIconHandle;
        data.szTip = "RustMeTweaker";
        data.szInfo = "";
        data.szInfoTitle = "";
        return data;
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_TRAYICON)
        {
            uint ev = (uint)lParam.ToInt64();
            if (ev == WM_LBUTTONUP || ev == WM_LBUTTONDBLCLK)
            {
                RestoreWindow();
            }
            else if (ev == WM_RBUTTONUP || ev == WM_CONTEXTMENU)
            {
                ShowTrayMenu();
            }
            return IntPtr.Zero;
        }
        return CallWindowProcW(_origWndProc, hwnd, msg, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATAW
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATAW lpData);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImageW(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AppendMenuW(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessageW(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);
}