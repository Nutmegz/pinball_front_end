using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nutmegz.UnManaged
{
    public static partial class WindowControl
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hwnd, SW command);

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

        //show window by hWnd
        public static bool ShowWindow(IntPtr hWnd, bool waitSuccess = false, bool silent = false)
        {
            bool result = ShowWindow(hWnd, SW.SHOWNORMAL);

            //Wait incase windows is not yet hidden and keep trying (could hang program)
            if (waitSuccess)
                while (!IsWindowVisible(hWnd))
                    result = ShowWindow(hWnd, SW.SHOWNORMAL);

            if (!silent)
                logger.Info($"Show Window: ID: {hWnd}, Result: {result}");
            return result;
        }

        //show window by title
        public static IntPtr ShowWindow(string windowTitle, bool waitTitle = false, bool waitSuccess = false, bool silent = false)
        {
            var hWnd = FindWindowHandleByTitle(windowTitle, waitTitle);
            var result = ShowWindow(hWnd, waitSuccess, silent);
            if (!silent)
                logger.Info($"Show Window: Title: {windowTitle}, ID: {hWnd}, Result: {result}");
            return hWnd;
        }

        //hide window by hWnd
        public static bool HideWindow(IntPtr hWnd, bool waitSuccess = false, bool silent = false)
        {
            bool result = ShowWindow(hWnd, SW.HIDE);

            //Wait incase windows is not yet shown and keep trying (could hang program)
            if (waitSuccess)
                while (IsWindowVisible(hWnd))
                    result = ShowWindow(hWnd, SW.HIDE);

            if (!silent)
                logger.Info($"Hide Window: ID: {hWnd}, Result: {result}");
            return result;
        }

        //hide window by title
        public static IntPtr HideWindow(string windowTitle, bool waitTitle = false, bool waitSuccess = false, bool silent = false)
        {
            var hWnd = FindWindowHandleByTitle(windowTitle, waitTitle);
            var result = HideWindow(hWnd, waitSuccess, silent);
            if (!silent)
                logger.Info($"Hide Window: Title: {windowTitle}, ID: {hWnd}, Result: {result}");
            return hWnd;
        }

    }
}
