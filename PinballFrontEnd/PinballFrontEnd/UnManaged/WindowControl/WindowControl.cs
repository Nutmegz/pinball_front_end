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


namespace Nutmegz.UnManaged
{

    static partial class WindowControl
    {

        #region Class Logger

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        //Unmanaged Code is split into their specific funtions via partial class

        
        #region FindWindow


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


        public static void HideAllProcessWindows(string processName,bool silent = false)
        {
            foreach (var handle in FindAllProcessWindows(processName))
            {
                //Console.WriteLine($"Handle: {handle}");
                HideWindow(handle, false, silent);
            }
        }




    }
}
