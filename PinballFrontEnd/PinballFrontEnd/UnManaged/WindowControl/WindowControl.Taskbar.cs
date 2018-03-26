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
    }
}
