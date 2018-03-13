using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NLog;
using HWND = System.IntPtr;

//Modified From:
//https://ronniediaz.com/2011/05/03/start-a-process-in-the-foreground-in-c-net-without-appactivate/

//and
//https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c


namespace UnManaged
{
    

    static class WindowControl
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();



        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern HWND FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int command);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int LockSetForegroundWindow(int locked);

        private const int LSFW_LOCK = 1;
        private const int LSFW_UNLOCK = 2;

        public static void LockForground()
        {
            var r = LockSetForegroundWindow(LSFW_LOCK);
            logger.Info($"Locking Foreground Window: {r}");
        }

        public static void UnlockForground()
        {
            var r = LockSetForegroundWindow(LSFW_UNLOCK);
            logger.Info($"Unlocking Foreground Window: {r}");
        }


        //////////////////////////////////////////////////////
        // Set Focus Function
        //////////////////////////////////////////////////////

        public static void SetFocus(string processName)
        {
            IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            var r = SetForegroundWindow(id);
            //HWND h = FindWindow(className, windowName);
            //var r = ShowWindow(id, SW_SHOWNORMAL);
            logger.Info($"Setting Focus: Name={processName}, ID={id}, Result={r}");
        }

        public static void SetFocus(string classname, string windowname)
        {
            //IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            //var success = SetForegroundWindow(id);
            HWND h = FindWindow(classname, windowname);
            var r = ShowWindow(h, SW_SHOWNORMAL);
            logger.Info($"Setting Focus: Name={classname}.{windowname}, Handle={h}, Result={r}");
        }

        public static void SetFocus(string processName, IntPtr id)
        {
            //IntPtr id = FindWindowByCaption(IntPtr.Zero, processName);
            var r = SetForegroundWindow(id);
            logger.Info($"Setting Focus: Name={processName}, ID={id}, Result={r}");
        }



        //////////////////////////////////////////////////////
        // Get Window Name
        //////////////////////////////////////////////////////

        

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

        //////////////////////////////////////////////////////
        // TaskBar Functions
        //////////////////////////////////////////////////////

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;

        public static IntPtr Handle
        {
            get
            {
                return FindWindow("HHTaskBar", "");
            }
        }
        public static IntPtr StartHandle
        {
            get
            {
                return FindWindow("Button", "Start");
            }
        }

        public static void ShowTaskbar()
        {
            logger.Info($"Showing Taskbar");
            ShowWindow(Handle, SW_SHOWNORMAL);
            //ShowWindow(StartHandle, SW_SHOW);
        }

        public static void HideTaskbar()
        {
            logger.Info($"Hiding Taskbar");
            ShowWindow(Handle, SW_HIDE);
            //ShowWindow(StartHandle, SW_HIDE);
        }


    }
}
