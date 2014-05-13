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
        private bool _isRunning = true;
        private int _serverId;

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
                _serverId = Settings.Default.ServerID;
                UpdateCurrentServer();
            }

            UuidTextBox.Text = _pn.SessionUUID;
            IdTextBox.Text = Settings.Default.ServerID.ToString(CultureInfo.InvariantCulture);

            _pn.Subscribe("test", delegate { }, delegate { }, delegate { });
            //pn.Presence<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceError);
            _pn.EnableResumeOnReconnect = true;
            _debugList = new ObservableCollection<DebugInfo>();
            DebugListView.ItemsSource = _debugList;
        }

        private void OnPulse(object sender, EventArgs e)
        {
            var currentCpu = _statsHelper.GetCurrentCpu();

            AddDebugInfo(

                String.Format("Sending Pulse - CPU: {0}, RAM: {1}, Disk Usage: {2} and Uptime: {3}",
                    _statsHelper.GetCurrentCpu(), _statsHelper.GetCurrentRamPercent(), _statsHelper.GetFreeDiskStorageAsPercentage(), _statsHelper.GetUptime()));

            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

            var dataToSend = new NameValueCollection
            {
                {"server_id", _serverId.ToString(CultureInfo.InvariantCulture)},
                {"ram_usage", _statsHelper.GetCurrentRamPercent()},
                {"cpu_usage", _statsHelper.GetCurrentCpu()},
                {"disk_usage", _statsHelper.GetFreeDiskStorageAsPercentage()},
                {"uptime", _statsHelper.GetUptime().ToString(CultureInfo.InvariantCulture)},
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
                {"available_disk", _statsHelper.GetTotalDiskStorage()},
                {"available_ram", _statsHelper.GetTotalRamSize()},
                {"cpu_speed", _statsHelper.GetCpuFrequency().ToString(CultureInfo.InvariantCulture)},
                {"os_name", _statsHelper.GetOsName()},
                {"os_version", _statsHelper.GetOsVersion()},
            };
            Cadence.UpdateServer(dataToSend);
        }

        private void OnPresenceError(PubnubClientError obj)
        {
        }

        /// <summary>
        /// Adds a new server to Cadence Service.
        /// </summary>
        private void AddNewServer()
        {
            try
            {
                var dataToSend = new NameValueCollection
                {
                    {"servergroup_id", "0"},
                    {"name", Environment.MachineName},
                    {"available_disk", _statsHelper.GetTotalDiskStorage()},
                    {"available_ram", _statsHelper.GetTotalRamSize()},
                    {"cpu_speed", _statsHelper.GetCpuFrequency().ToString(CultureInfo.InvariantCulture)},
                    {"os_name", _statsHelper.GetOsName()},
                    {"os_version", _statsHelper.GetOsVersion()},
                    {"guid", Settings.Default.ServerGUID}
                };
                //Creates the server and responds with the ID
                var serverId = Cadence.CreateServer(dataToSend);

                Settings.Default.ServerID = serverId;
                Settings.Default.Save();

                _serverId = Settings.Default.ServerID;
            }
            catch (Exception)
            {       
            }
        }

        private void OnPresenceConnect(object obj)
        {
        }

        private void OnUserPresence(object obj)
        {
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

        /// <summary>
        /// Allows the entire window to be dragged no matter at which point the mouse is down
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="e">Event Arguments</param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OnPresenceDisconnect(object obj)
        {
        }

        /// <summary>
        /// Stops the pulsing timer
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="e">Event Arguments</param>
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