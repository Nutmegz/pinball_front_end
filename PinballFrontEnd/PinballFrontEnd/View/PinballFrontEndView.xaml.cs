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
using NLog;

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for PinballFrontEndView.xaml
    /// </summary>
    public partial class PinballFrontEndView : Window
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();


        //Main View Model
        private PinballFrontEnd.ViewModel.PinballFrontEndViewModel viewModel;

        public PinballFrontEndView()
        {
            InitializeComponent();
            viewModel = new PinballFrontEnd.ViewModel.PinballFrontEndViewModel();
            this.DataContext = viewModel;
            logger.Trace("Loading PinballFrondEndView");

        }
          
    }
}
