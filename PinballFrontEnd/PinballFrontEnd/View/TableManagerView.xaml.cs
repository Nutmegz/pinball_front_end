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

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : Window
    {
        public TableManagerView()
        {
            InitializeComponent();

            var viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel();
            this.DataContext = viewModel;

        }
    }
}
