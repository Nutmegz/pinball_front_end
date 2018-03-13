using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinballFrontEnd.Model
{
    public class Keybindings
    {

        public Key KEYBIND_EXIT { get; set; } = Key.Escape;
        public Key KEYBIND_TABLEMANAGER { get; set; } = Key.F1;
        public Key KEYBIND_NEXT { get; set; } = Key.Right;
        public Key KEYBIND_PREV { get; set; } = Key.Left;
        public Key KEYBIND_RAND { get; set; } = Key.F2;
        public Key KEYBIND_START { get; set; } = Key.Enter;
        public Key KEYBIND_RECORD { get; set; } = Key.F4;

    }
}
