using System.Collections.ObjectModel;
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
using CadenceHost.Models;
using PubNubMessaging.Core;

namespace CadenceHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<DebugInfo> debugList;
        private Pubnub pn;
        public MainWindow()
        {
            InitializeComponent();
            var statsHelper = new Statistics();

            var timer = new DispatcherTimer(new TimeSpan(0,0,0,1),DispatcherPriority.Normal,
                (sender, args) => AddDebugInfo(statsHelper.GetCurrentCpu()),Dispatcher );
            pn = new Pubnub("pub-c-18bc7bd1-2981-4cc4-9c4e-234d25519d36", "sub-c-5782df52-d147-11e3-93dd-02ee2ddab7fe");
           

            if (String.IsNullOrEmpty(Properties.Settings.Default.ServerGUID))
            {
                pn.SessionUUID = Guid.NewGuid().ToString();
                Properties.Settings.Default.ServerGUID = pn.SessionUUID;
                Properties.Settings.Default.Save();
            }
            else
            {
                pn.SessionUUID = Properties.Settings.Default.ServerGUID;
            }

            UuidTextBox.Text = pn.SessionUUID;

            pn.Subscribe<string>("test", delegate(string o) { }, delegate(string o) { }, delegate(PubnubClientError error) { });
            //pn.Presence<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceError);
            pn.EnableResumeOnReconnect = true;
            debugList = new ObservableCollection<DebugInfo>();
            DebugListView.ItemsSource = debugList;
        }

        private void OnPresenceError(PubnubClientError obj)
        {
            throw new NotImplementedException();
        }

        private void OnPresenceConnect(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnUserPresence(object obj)
        {
            throw new NotImplementedException();
        }

        private void AddDebugInfo(string info)
        {
            debugList.Add(new DebugInfo()
            {
                ID = 1,
                Type = "CPU", 
                Message = info
            });
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pn.Unsubscribe<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceDisconnect, OnPresenceError);
            pn.PresenceUnsubscribe<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceDisconnect, OnPresenceError);
        }

        private void OnPresenceDisconnect(object obj)
        {
           
        }
    }

}
