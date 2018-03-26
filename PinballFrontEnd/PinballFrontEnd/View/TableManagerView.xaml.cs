using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using NLog;
using PinballFrontEnd.Model;
using System.Collections.ObjectModel;
using Vlc.DotNet.Wpf;
using MahApps.Metro.Controls;

//TODO: Conform with MVVM and move most code into the view model.

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : MetroWindow
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ViewModel.TableManagerViewModel viewModel;
        //private string databasepath = "";

        public TableManagerView(string databasepath)
        {
            InitializeComponent();

            //this.databasepath = databasepath;
            viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel(databasepath);
            this.DataContext = viewModel;

        }

        public TableManagerView(PinballData data)
        {
            InitializeComponent();

            viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel(data);
            this.DataContext = viewModel;

        }

        //clean up view model
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.Dispose();
        }


    }
}
