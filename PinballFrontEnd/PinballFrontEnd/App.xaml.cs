using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;

namespace PinballFrontEnd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //global error logger
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            if (e.Args.Length > 0)
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
                
                Nutmegz.UnManaged.WindowControl.HideTaskbar();
                Nutmegz.UnManaged.WindowControl.HideCursor();
            }
           
        }

        //Runs when program quits
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Show Everything
            Nutmegz.UnManaged.WindowControl.UnlockForground();
            Nutmegz.UnManaged.WindowControl.ShowTaskbar();
            Nutmegz.UnManaged.WindowControl.ShowCursor();
        }


        //Catch all program errors and log the exception.
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //logger.ErrorException("Fatal Application Error",(System.Exception)e.ExceptionObject);
            logger.Error(e.ExceptionObject.ToString(), "Fatal Application Error");
            //logger.Error(e.ToString());
            //MessageBox.Show("Pinball Front End Fatal Error. Check Log File");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
