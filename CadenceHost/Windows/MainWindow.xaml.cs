// ---------------------------------------------------------------------------------------
//  <copyright file="MainWindow.xaml.cs" company="Cadence">
//      Copyright © 2013-2014 by Brandon Scott and Christopher Franklin.
// 
//      Permission is hereby granted, free of charge, to any person obtaining a copy of
//      this software and associated documentation files (the "Software"), to deal in
//      the Software without restriction, including without limitation the rights to
//      use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//      of the Software, and to permit persons to whom the Software is furnished to do
//      so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//      WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//      CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
//  </copyright>
//  ---------------------------------------------------------------------------------------

#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CadenceHost.Helpers;
using CadenceHost.Models;
using CadenceHost.Properties;
using PubNubMessaging.Core;

#endregion

namespace CadenceHost.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<DebugInfo> _debugList;
        private readonly Pubnub _pn;
        private readonly Statistics _statsHelper;
        private readonly DispatcherTimer _timer;
        private bool _isRunning;

        public MainWindow()
        {
            InitializeComponent();
            _statsHelper = new Statistics();

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 5), DispatcherPriority.Normal,
                OnPulse, Dispatcher);
            //timer.Stop();
            _pn = new Pubnub("pub-c-18bc7bd1-2981-4cc4-9c4e-234d25519d36", "sub-c-5782df52-d147-11e3-93dd-02ee2ddab7fe")
            {
                PresenceHeartbeatInterval = 13,
                PresenceHeartbeat = 10
            };

            if (String.IsNullOrEmpty(Settings.Default.ServerGUID))
            {
                _pn.SessionUUID = Guid.NewGuid().ToString();
                Settings.Default.ServerGUID = _pn.SessionUUID;
                Settings.Default.Save();
                AddNewServer();
            }
            else
            {
                _pn.SessionUUID = Settings.Default.ServerGUID;
                //UpdateCurrentServer();
            }

            UuidTextBox.Text = _pn.SessionUUID;

            _pn.Subscribe("test", delegate { }, delegate { }, delegate { });
            //pn.Presence<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceError);
            _pn.EnableResumeOnReconnect = true;
            _debugList = new ObservableCollection<DebugInfo>();
            DebugListView.ItemsSource = _debugList;
        }

        private void OnPulse(object sender, EventArgs e)
        {
            AddDebugInfo(
                String.Format("Sending Pulse to server with CPU: {0}, RAM: {1}, Disk Usage: {2} and Uptime: {3} with a Total RAM Size of: {4}",
                    _statsHelper.GetCurrentCpu(), _statsHelper.GetCurrentRam(), "%DISK%", "%UPTIME%", _statsHelper.GetTotalRamSize()));

            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

            var dataToSend = new NameValueCollection
            {
                {"server_id", "2"},
                {"ram_usage", (100 -(Convert.ToDouble(_statsHelper.GetCurrentRam()) / Convert.ToDouble(_statsHelper.GetTotalRamSize()) * 100)).ToString(CultureInfo.InvariantCulture)},
                {"cpu_usage", _statsHelper.GetCurrentCpu()},
                {"disk_usage", "0"},
                {"uptime", "0"},
                {"timestamp", timeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture)}
            };
            Cadence.SendPulse(dataToSend);
        }

        /// <summary>
        /// Updates the current instance of the server based on GUID
        /// </summary>
        private void UpdateCurrentServer()
        {
            var dataToSend = new NameValueCollection
            {
                {"available_disk", "0"},
                {"available_ram", "0"},
                {"cpu_speed", "0"},
                {"os_name", "0"},
                {"os_version", "0"}
            };
            Cadence.CreateServer(dataToSend);
        }

        private void OnPresenceError(PubnubClientError obj)
        {
        }

        /// <summary>
        /// Adds a new server to Cadence Service.
        /// </summary>
        private static void AddNewServer()
        {
            var dataToSend = new NameValueCollection
            {
                {"servergroup_id", "0"},
                {"name", Environment.MachineName},
                {"available_disk", "0"},
                {"available_ram", "0"},
                {"cpu_speed", "0"},
                {"os_name", "0"},
                {"os_version", "0"},
                {"guid", Settings.Default.ServerGUID}
            };
            Cadence.CreateServer(dataToSend);
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
            _debugList.Add(new DebugInfo
            {
                ID = 1,
                Type = "Sending Pulse",
                Message = info
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            //pn.PresenceUnsubscribe<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceDisconnect, OnPresenceError);
        }

        private void OnPresenceDisconnect(object obj)
        {
        }

        private void OnStatusClick(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _timer.Stop();
                _pn.Unsubscribe<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceDisconnect, OnPresenceError);
                _isRunning = false;
                tbStatus.Text = "Stopped";
                StatusButton.Content = "Start";
            }
            else
            {
                _timer.Start();
                _isRunning = true;
                tbStatus.Text = "Started";
                StatusButton.Content = "Stop";
            }
        }
    }
}