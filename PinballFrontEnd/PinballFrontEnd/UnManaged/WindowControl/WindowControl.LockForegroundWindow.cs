using System.Runtime.InteropServices;

namespace Nutmegz.UnManaged
{
    public static partial class WindowControl
    {

        private enum LSFW : int
        {
            LOCK = 0x00000001,
            UNLOCK = 0x00000002
        }


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int LockSetForegroundWindow(LSFW locked);


        //Prevent other programs from setting focus / activate
        public static void LockForground()
        {
            var r = LockSetForegroundWindow(LSFW.LOCK);
            logger.Info($"Locking Foreground Window: {r}");
        }

        //Allow other programs to set focus / activate
        public static void UnlockForground()
        {
            var r = LockSetForegroundWindow(LSFW.UNLOCK);
            logger.Info($"Unlocking Foreground Window: {r}");
        }
    }
}
