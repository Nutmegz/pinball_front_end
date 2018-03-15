using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PinballFrontEnd.Model
{
    public class MediaLocation : INotifyPropertyChanged
    {
        public int ThumbnailResolution { get; set; } = 0;

        public int PlayfieldLocationX { get; set; } = 0;
        public int PlayfieldLocationY { get; set; } = 0;
        public int PlayfieldSizeX { get; set; } = 400;
        public int PlayfieldSizeY { get; set; } = 400;
        public int PlayfieldRotation { get; set; } = 0;
        [JsonIgnore]
        public bool PlayfieldVisable { get; set; } = true;

        public int BackglassLocationX { get; set; } = 500;
        public int BackglassLocationY { get; set; } = 0;
        public int BackglassSizeX { get; set; } = 400;
        public int BackglassSizeY { get; set; } = 400;
        public int BackglassRotation { get; set; } = 0;
        [JsonIgnore]
        public bool BackglassVisable { get; set; } = true;

        public int DMDLocationX { get; set; } = 1000;
        public int DMDLocationY { get; set; } = 0;
        public int DMDSizeX { get; set; } = 400;
        public int DMDSizeY { get; set; } = 400;
        public int DMDRotation { get; set; } = 0;
        [JsonIgnore]
        public bool DMDVisable { get; set; } = true;

        [JsonIgnore]
        public int WheelSizeXY
        {
            get
            {
                return (int)(PlayfieldSizeX * 0.25);
            }
        }
        [JsonIgnore]
        public int WheelLocationY
        {
            get
            {
                return (int)((PlayfieldSizeY * 0.5) - WheelSizeXY);
            }
        }
        [JsonIgnore]
        public int WheelLocationX
        {
            get
            {
                return (int)((PlayfieldSizeX * .75));
            }
        }
        [JsonIgnore]
        public int WheelRotation
        {
            get
            {
                return (int)(PlayfieldRotation + 90);
            }
        }




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
