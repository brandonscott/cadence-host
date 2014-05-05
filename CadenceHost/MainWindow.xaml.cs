using System.Threading;
using System.Windows.Threading;
using CadenceHost.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CadenceHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            var statsHelper = new Statistics();
            
            var timer = new DispatcherTimer(new TimeSpan(0,0,0,1),DispatcherPriority.Normal,
                (sender, args) => AddDebugInfo(statsHelper.GetCurrentCpu()),Dispatcher );
        }

        private void AddDebugInfo(string info)
        {
            txtDebugInfo.AppendText(String.Format("{0}: {1}\r\n", DateTime.Now, info));
            txtDebugInfo.ScrollToEnd();
        }
    }
}
