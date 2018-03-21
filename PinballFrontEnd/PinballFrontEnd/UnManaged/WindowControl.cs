using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NLog;
//using IntPtr = System.IntPtr;
using System.Diagnostics;


//Modified From:
//https://ronniediaz.com/2011/05/03/start-a-process-in-the-foreground-in-c-net-without-appactivate/

//and
//https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c


namespace UnManaged
{


    static class WindowControl
    {

        #region Class Logger

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);


        #region FindWindow

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);


        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, WM Msg, int wParam, StringBuilder lParam);

        //Window Message Constants
        private enum WM : uint
        {
            GETTEXT = 0x000D
        }


        public static IEnumerable<IntPtr> FindAllProcessWindows(string processName)
        {
            return EnumerateProcessWindowHandles(FindProcessId(processName));
        }

        public static IntPtr FindProcessWindow(string processName, string windowName)
        {
            foreach (var handle in FindAllProcessWindows(processName))
            {
                StringBuilder message = new StringBuilder(1000);
                SendMessage(handle, WM.GETTEXT, message.Capacity, message);
                if (message.ToString() == windowName)
                {
                    //Console.WriteLine($"My Handle: {handle}");
                    return handle;
                }
            }
            return IntPtr.Zero;
        }

        public static int FindProcessId(string processName)
        {
            return FindProcess(processName).Id;
        }

        
        public static Process FindProcess(string processName, bool waitFind = false)
        {
            return Process.GetProcessesByName(processName).First();
        }

        #endregion

        //Group to handle locking programs from setting focus
        #region LockForegroundWindow


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int LockSetForegroundWindow(LSFW locked);

        private enum LSFW : int
        {
            LOCK = 0x00000001,
            UNLOCK = 0x00000002
        }

        //Prevent other programs from setting focus / activate
        public static void LockForground()
        {
            var r = LockSetForegroundWindow(LSFW.LOCK);
            logger.Info($"Locking Foreground Window: {r}");
        }

        //Allow other programs to set focus / activate
        public static void UnlockForground()
        {
            var r = LockSetForegroundWindow(LSFW.UNLOCK);
            logger.Info($"Unlocking Foreground Window: {r}");
        }

        #endregion

        #region Thread Attach

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        public static bool AttachThread(string currentProcess, string processName)
        {
            IntPtr idC = FindWindowByCaption(IntPtr.Zero, currentProcess);
            IntPtr idN = FindWindowByCaption(IntPtr.Zero, processName);

            var r = AttachThreadInput((uint)idN, (uint)idC, true);



            return false;
        }

        public static bool AttachThread(uint idAttach, uint idAttachTo, bool fAttach)
        {

            //var id = Process.GetCurrentProcess().Id;
            IntPtr id = FindWindowByCaption(IntPtr.Zero, "PFE Main Window");

            var r = AttachThreadInput((uint)idAttach, (uint)id, fAttach);

            logger.Info($"Attaching to Thread: Attach={idAttach}, AttachTo={id}, State={fAttach}, Result={r}");
            logger.Info($"Attaching to Thread: Attach={idAttach}, AttachTo={idAttachTo}, State={fAttach}, Result={r}");

            return false;
        }

        #endregion

        //Functions to set focus of windows
        #region Set Focus

        public static bool SetFocusForeground(string processName, bool waitName = false, bool waitSet = false)
        {
            IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);

            if (waitName)
                while (id == IntPtr.Zero)
                {
                    id = FindWindowByCaption(IntPtr.Zero, processName);
                }

            var r = SetFocusForeground(id, waitSet);

            logger.Info($"Setting Foreground Window: Name={processName}, ID={id}, Foreground Window={r},");

            return r;
        }

        public static bool SetFocusForeground(IntPtr processId, bool waitSet = false)
        {

            bool r = SetForegroundWindow(processId);

            if (waitSet)
                while (!r)
                {
                    r = SetForegroundWindow(processId);
                }

            logger.Info($"Setting Foreground Window: ID={processId}, Foreground Window={r},");

            return r;
        }

        public static bool SetFocusActive(string processName)
        {
            IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            var r = SetActiveWindow(id);

            logger.Info($"Setting Focus: Name={processName}, ID={id}, Foreground Window={r},");

            if (r == null)
                return false;
            else
                return true;
        }

        public static bool SetFocus(string processName)
        {
            IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            var r = SetActiveWindow(id);
            var r1 = SetForegroundWindow(id);

            logger.Info($"Setting Focus: Name={processName}, ID={id}, Active Window={r}, Foreground Window={r1} ");

            return r1;
        }

        //public static bool SetFocus(string processName)
        //{
        //    IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
        //    var r = SetForegroundWindow(id);
        //    //IntPtr h = FindWindow(className, windowName);
        //    //var r = ShowWindow(id, SW_SHOWNORMAL);
        //    logger.Info($"Setting Focus: Name={processName}, ID={id}, Result={r}");
        //    return r;
        //}

        public static void SetFocus(string classname, string windowname)
        {
            //IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            //var success = SetForegroundWindow(id);
            IntPtr h = FindWindow(classname, windowname);
            var r = ShowWindow(h, SW.SHOWNORMAL);
            logger.Info($"Setting Focus: Name={classname}.{windowname}, Handle={h}, Result={r}");
        }

        public static bool SetFocus(string processName, IntPtr id)
        {
            //IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            var r = SetForegroundWindow(id);
            logger.Info($"Setting Focus: Name={processName}, ID={id}, Result={r}");
            return r;
        }

        #endregion

        #region Window Title Functions

        //Gets the active windows title
        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        //Finds the class the active window is
        public static string GetActiveWindowClass()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetClassName(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        #endregion

        #region ShowHideWindow

        //Show Window Constants
        public enum SW : int
        {
            FORCEMINIMIZE = 0x00000011,
            HIDE = 0x00000000,
            MAXIMIZE = 0x00000003,
            MINIMIZE = 0x00000006,
            RESTORE = 0x00000009,
            SHOW = 0x00000005,
            SHOWDEFAULT = 0x000000010,
            SHOWMAXIMIZED = 0x00000003,
            SHOWMINIMIZED = 0x00000002,
            SHOWMINNOACTIVE = 0x00000007,
            SHOWNA = 0x00000008,
            SHOWNOACTIVATE = 0x00000004,
            SHOWNORMAL = 0x00000001
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hwnd, SW command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        public static IntPtr ShowWindow(string windowTitle, bool waitTitle = false, bool waitSuccess = false)
        {
            var hwnd = FindWindowHandleByTitle(windowTitle, waitTitle);
            var result = ShowWindow(hwnd, waitSuccess);
            logger.Info($"Show Window: Title: {windowTitle}, ID: {hwnd}, Result: {result}");
            return hwnd;
        }

        public static bool ShowWindow(IntPtr hwnd, bool waitSuccess = false)
        {
            bool result = ShowWindow(hwnd, SW.SHOWNORMAL);

            //Wait incase windows is not yet hidden and keep trying (could hang program)
            if (waitSuccess)
                while (!IsWindowVisible(hwnd))
                    result = ShowWindow(hwnd, SW.SHOWNORMAL);

            logger.Info($"Show Window: ID: {hwnd}, Result: {result}");
            return result;
        }

        public static IntPtr HideWindow(string windowTitle, bool waitTitle = false, bool waitSuccess = false)
        {
            var hwnd = FindWindowHandleByTitle(windowTitle, waitTitle);
            var result = HideWindow(hwnd, waitSuccess);
            logger.Info($"Hide Window: Title: {windowTitle}, ID: {hwnd}, Result: {result}");
            return hwnd;
        }

        public static bool HideWindow(IntPtr hwnd, bool waitSuccess = false)
        {
            bool result = ShowWindow(hwnd, SW.HIDE);

            //Wait incase windows is not yet shown and keep trying (could hang program)
            if (waitSuccess)
                while (IsWindowVisible(hwnd))
                    result = ShowWindow(hwnd, SW.HIDE);

            logger.Info($"Hide Window: ID: {hwnd}, Result: {result}");
            return result;
        }

        public static void HideAllProcessWindows(string processName)
        {
            foreach (var handle in FindAllProcessWindows(processName))
            {
                //Console.WriteLine($"Handle: {handle}");
                HideWindow(handle, true);
            }
        }


        public static IntPtr FindWindowHandleByTitle(string windowTitle, bool waitTitle = false)
        {
            //Window Handle
            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, windowTitle); ;

            //Wait until a handle is found (could hang program)
            if (waitTitle)
                while (hwnd == IntPtr.Zero)
                    hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, windowTitle);

            return hwnd;
        }

        #endregion

        #region Task Bar

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        //Handle to the windows task bar
        public static IntPtr HandleOfTaskBar
        {
            get
            {
                return FindWindow("Shell_TrayWnd", "");
            }
        }
        //Handle to the start button
        public static IntPtr HandleOfStartButton
        {
            get
            {
                return FindWindowEx(GetDesktopWindow(), IntPtr.Zero, "Button", "Start");
            }
        }

        //Handle to the desktop
        public static IntPtr HandleOfDesktop
        {
            get
            {
                return (IntPtr)GetDesktopWindow();
            }
        }

        //Shows the Windows Task Bar
        public static void ShowTaskbar()
        {
            logger.Info($"Showing Task Bar");
            ShowWindow(HandleOfTaskBar, SW.SHOWNORMAL);
            ShowWindow(HandleOfStartButton, SW.SHOWNORMAL);
        }

        //Hides the Windows Task Bar
        public static void HideTaskbar()
        {
            logger.Info($"Hiding Task Bar");
            ShowWindow(HandleOfTaskBar, SW.HIDE);
            ShowWindow(HandleOfStartButton, SW.HIDE);
        }
        #endregion

        #region Cursor

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        //Hide the mouse cursor for the calling program
        public static void ShowCursor()
        {
            logger.Info($"Showing Cursor");
            ShowCursor(true);
        }

        //Show the mouse cursor for the calling program
        public static void HideCursor()
        {
            logger.Info($"Hiding Cursor");
            ShowCursor(false);
        }


        #endregion

        #region PowerState

        /////////////////////////////////////////////////////////////////////////////
        // WINDOWS POWER OVERRIDE
        /////////////////////////////////////////////////////////////////////////////

        //https://stackoverflow.com/questions/241222/need-to-disable-the-screen-saver-screen-locking-in-windows-c-net

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            // Legacy flag, should not be used.
            // ES_USER_PRESENT   = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
        }

        public static class SleepUtil
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        }

        public static void PreventSleep()
        {
            if (SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS
                | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                | EXECUTION_STATE.ES_SYSTEM_REQUIRED
                | EXECUTION_STATE.ES_AWAYMODE_REQUIRED) == 0) //Away mode for Windows >= Vista
                SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS
                    | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                    | EXECUTION_STATE.ES_SYSTEM_REQUIRED); //Windows < Vista, forget away mode
        }

        #endregion


    }
}
