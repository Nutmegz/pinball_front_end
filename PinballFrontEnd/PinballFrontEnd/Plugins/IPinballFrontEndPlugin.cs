using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinballFrontEnd.Plugins
{
    public interface IPinballFrontEndPlugin
    {
        string Name { get; }
        void NextTable();
        void PrevTable();
        void RandomTable();
        void StartTable();
        void EndTable();
    }
}
