using System;
using System.Runtime.InteropServices;

namespace Nutmegz.UnManaged
{
    public static partial class WindowControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr SetFocus(HandleRef hWnd);

      
    }
}
