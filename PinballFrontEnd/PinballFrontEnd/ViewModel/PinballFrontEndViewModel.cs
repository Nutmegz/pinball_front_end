using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinballFrontEnd.ViewModel
{
    public class PinballFrontEndViewModel : ViewModelBase
    {

        //Properties
        private string videoFile;
        public string VideoFile
        {
            get { return videoFile; }
            set { videoFile = value; NotifyPropertyChanged("VideoFile"); }
        }

        private List<String> files = new List<string>();
        
        // Commands
        public ICommand NextVideoFile { get { return new RelayCommand(NVF); } }


        public PinballFrontEndViewModel()
        {
            

            files.Add("D:\\AmericanDad.mp4");
            files.Add("D:\\Archer.mp4");
            files.Add("D:\\BioLab.mp4");

            VideoFile = files.ElementAt(0);


        }

        public void NVF(object obj)
        {
            VideoFile=files.ElementAt(
                files.FindIndex(x => x == VideoFile) >=0 && files.FindIndex(x => x == VideoFile) < files.Count()-1 ? files.FindIndex(x => x == VideoFile)+1 : 0);
        }
    }
}
