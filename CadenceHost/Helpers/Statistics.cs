// ---------------------------------------------------------------------------------------
//  <copyright file="Statistics.cs" company="Cadence">
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;

#endregion

namespace CadenceHost.Helpers
{
    public class Statistics
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;

        public Statistics()
        {
            _cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };

            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
          
            //Set up CPU Counter for the first run
            _cpuCounter.NextValue();
        }

        /// <summary>
        /// Gets the current machine CPU Usage
        /// </summary>
        /// <returns>The current CPU usage</returns>
        public string GetCurrentCpu()
        {
            return _cpuCounter.NextValue().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the current machine RAM Usage
        /// </summary>
        /// <returns>The current RAM usage</returns>
        public string GetCurrentRam()
        {
            return _ramCounter.NextValue().ToString(CultureInfo.InvariantCulture);
        }

        public string GetCurrentRamPercent()
        {
            return
                (100 - (Convert.ToDouble(GetCurrentRam())/Convert.ToDouble(GetTotalRamSize())*100)).ToString(CultureInfo.InvariantCulture);
        }

        public String GetTotalRamSize()
        {
            var mc = new ManagementClass("Win32_ComputerSystem");
            var moc = mc.GetInstances();
            foreach (var item in moc.Cast<ManagementObject>())
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 1048576, 2));
            }
            return "RAMsize";
        }

        public String GetOsName()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        public String GetOsVersion()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Version")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        public String GetTotalDiskStorage()
        {
            var driveInfo = new DriveInfo(@"C");
            var totalSizeAsMb = driveInfo.TotalSize / 1048576;
            return totalSizeAsMb.ToString(CultureInfo.InvariantCulture);
        }

        public String GetFreeDiskStorage()
        {
            var driveInfo = new DriveInfo(@"C");
            var freeSpaceAsMb = driveInfo.AvailableFreeSpace / 1048576;
            return freeSpaceAsMb.ToString(CultureInfo.InvariantCulture);
        }

        public String GetFreeDiskStorageAsPercentage()
        {
            return (100 - Convert.ToDouble(GetFreeDiskStorage())/Convert.ToDouble(GetTotalDiskStorage())*100).ToString(CultureInfo.InvariantCulture);
        }

        public int GetUptime()
        {
            var uptime = new PerformanceCounter("System", "System Up Time");
            //This would otherwise be zero
            uptime.NextValue();
            return Convert.ToInt32(uptime.NextValue());
        }

        public uint GetCpuFrequency()
        {
            var searcher = new ManagementObjectSearcher(
            "select MaxClockSpeed from Win32_Processor");

            uint freq = 0;

            foreach (var item in searcher.Get())
            {
                freq = (uint)item["MaxClockSpeed"];
            }
            return freq;
        }
    }
}