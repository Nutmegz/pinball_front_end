using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PinballFrontEnd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(e.Args.Length > 0)
            {
                foreach (string arg in e.Args)
                {
                    switch(arg)
                    {
                        case "tablemanager":
                            var TableManager = new View.TableManagerView($@"{Model.ProgramPath.Value}\database.json");
                            TableManager.Show();
                            break;
                    }
                }
            } else
            {
                //No Arguments, Start Main Program
                var MainProgram = new View.PinballFrontEndView();
                MainProgram.Show();
            }
           
        }
    }
}
