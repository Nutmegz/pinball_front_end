using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinballFrontEnd.Model
{
    public class PinballSystem
    {

        public String Name { get; set; } = "";
        public String WorkingPath { get; set; } = "";
        public String Executable { get; set; } = "";
        public String Parameters { get; set; } = "";
        public int WaitTime { get; set; } = 0;
        
    }
}
