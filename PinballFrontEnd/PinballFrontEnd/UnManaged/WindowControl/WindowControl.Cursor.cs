using System.Runtime.InteropServices;

namespace Nutmegz.UnManaged
{
    public static partial class WindowControl
    {

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        //Hide the mouse cursor for the calling program
        public static void ShowCursor()
        {
            logger.Info($"Showing Cursor");
            ShowCursor(true);
        }

        //Show the mouse cursor for the calling program
        public static void HideCursor()
        {
            logger.Info($"Hiding Cursor");
            ShowCursor(false);
        }
    }
}
