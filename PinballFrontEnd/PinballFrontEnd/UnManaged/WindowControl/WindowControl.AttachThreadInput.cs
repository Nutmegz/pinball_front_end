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
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        public static bool AttachThread(uint idAttach, uint idAttachTo, bool fAttach)
        {
            var r = AttachThreadInput((uint)idAttach, (uint)idAttachTo, fAttach);
            logger.Info($"Attaching to Thread: Attach={idAttach}, AttachTo={idAttachTo}, State={fAttach}, Result={r}");
            return r;
        }
    }
}
