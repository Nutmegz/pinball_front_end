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
                            var TableManager = new View.TableManagerView($@"{Model.ProgramPath.Value}database.json");
                            TableManager.Show();
                            break;
                    }
                }
            } else
            {
                //No Arguments, Start Main Program

                

                //Start Front End (Need to add better loading screen)
                var MainProgram = new View.PinballFrontEndView();
                MainProgram.Show() ;
                //Lock other programs from setting focus
                //UnManaged.WindowControl.SetFocusForeground(System.Diagnostics.Process.GetCurrentProcess().Id);
                
                UnManaged.WindowControl.HideTaskbar();
                UnManaged.WindowControl.HideCursor();
            }
           
        }

        //Runs when program quits
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Show Everything
            UnManaged.WindowControl.UnlockForground();
            UnManaged.WindowControl.ShowTaskbar();
            UnManaged.WindowControl.ShowCursor();
        }
    }
}
