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
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        //retuns the hWnd title
        public static string GetWindowText(IntPtr hWnd)
        {
            logger.Info($"Getting Window Title, hWnd={hWnd.ToString()}");
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            
            if (GetWindowText(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        //returns the active window title
        public static string GetForegroundWindowText()
        {
            return GetWindowText(GetForegroundWindow());
        }
    }
}
