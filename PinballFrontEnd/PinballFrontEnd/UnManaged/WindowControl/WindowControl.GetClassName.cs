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

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hWnd, StringBuilder lpClassName, long nMaxCount);

        //Finds the window class of hWnd
        public static string GetClassName(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetClassName(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        //Finds the class the active window is
        public static string GetForegroundWindowClassName()
        {
            return GetClassName(GetForegroundWindow());
        }
    }
}
