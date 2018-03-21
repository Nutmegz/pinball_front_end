using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PinballFrontEnd.Model
{
    public static class PinballFunctions
    {

        public static string GetSystemParameters(PinballSystem system, PinballTable table)
        {
            //Replace [TABLENAME]
            var regex = new Regex(@"\[TABLENAME\]");
            var param = regex.Replace(system.Parameters, table.Name);

            //Replace [SYSTEMPATH]
            regex = new Regex(@"\[SYSTEMPATH\]");
            param = regex.Replace(param, system.WorkingPath);

            return param;
        }

        public static string GetSystemPath(PinballSystem system)
        {
            return $@"{system.WorkingPath}\{system.Executable}";
        }

        public static Process StartTable(PinballSystem system, PinballTable table)
        {
            var proc = new Process();
            proc.StartInfo.FileName = GetSystemPath(system);
            proc.StartInfo.Arguments = GetSystemParameters(system, table);
            proc.Start();
            return proc;
        }



    }
}
