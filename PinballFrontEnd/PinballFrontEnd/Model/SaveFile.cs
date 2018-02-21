using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PinballFrontEnd.Model
{
    public class SaveFile
    {
        public ObservableCollection<PinballTable> PinballTables { get; set; }
        public ObservableCollection<PinballSystem> PinballSystems { get; set; }
    }
}
