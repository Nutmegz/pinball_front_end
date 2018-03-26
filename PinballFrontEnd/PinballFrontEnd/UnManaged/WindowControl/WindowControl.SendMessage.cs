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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, WM Msg, int wParam, StringBuilder lParam);

        //Window Message Constants
        private enum WM : uint
        {
            GETTEXT = 0x000D
        }
    }
}
