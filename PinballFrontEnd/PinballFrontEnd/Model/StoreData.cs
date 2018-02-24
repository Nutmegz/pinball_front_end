using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PinballFrontEnd.Model
{
    //Class Stores all data that needs to be saved for single file saving
    public class StoreData
    {
        public ObservableCollection<PinballSystem> SystemList { get; set; }
        public ObservableCollection<PinballTable> TableList { get; set; }
        public MediaLocation MediaLocation { get; set; }
        public Keybindings BIND { get; set; }
    }
}
