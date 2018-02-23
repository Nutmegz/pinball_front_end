using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PinballFrontEnd.Model
{
    public class PinballSystem : INotifyPropertyChanged
    {

        public String Name { get; set; } = "";
        public String WorkingPath { get; set; } = "";
        public String Executable { get; set; } = "";
        public String Parameters { get; set; } = "";
        public int WaitTime { get; set; } = 0;

        public ObservableCollection<PinballTable> Tables { get; set; } = new ObservableCollection<PinballTable>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyname)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }
}
