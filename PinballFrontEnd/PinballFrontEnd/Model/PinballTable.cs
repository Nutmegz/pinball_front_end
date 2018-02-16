using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinballFrontEnd.Model
{
    public class PinballTable
    {

        //Properties
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Rom { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public int Year { get; set; } = 0;
        public bool HideDMD { get; set; } = true;
        public bool HideBackglass { get; set; } = true;
        public bool Enabled { get; set; } = true;

        //Media File Names
        public Uri Playfield { get; set; } = null;
        public Uri Backglass { get; set; } = null;
        public Uri DMD { get; set; } = null;
        public Uri Wheel { get; set; } = null;

    }
}
