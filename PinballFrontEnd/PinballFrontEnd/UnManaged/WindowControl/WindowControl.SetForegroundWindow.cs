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


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        //Set foreground window by process handle
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

        //set forground window via process name
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

    }
}
