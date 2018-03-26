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

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        //Find window handle by window title
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


    }
}
