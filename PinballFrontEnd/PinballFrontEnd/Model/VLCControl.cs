using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinballFrontEnd.Model
{
    public static class VlcGlobal
    {

        //Global VLC Media Options
        public static string[] GetVlcArguments()
        {
            var mediaOptions = new string[]
            {
                ":input-repeat=2147483647",
                ":no-audio"
            };

            return mediaOptions;
        }

        //Get VLC Libary Directory
        public static DirectoryInfo GetVlcLibrary()
        {
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP VLC LIBRARY
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            var currentAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            DirectoryInfo libDirectory;

            if (IntPtr.Size == 4)
                libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, @"libvlc\x86\"));
            else
                libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, @"libvlc\x64\"));

            return libDirectory;
        }

    }
}
