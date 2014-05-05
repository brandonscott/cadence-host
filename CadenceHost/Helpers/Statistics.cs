using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadenceHost.Helpers
{
    public class Statistics
    {
        readonly PerformanceCounter _cpuCounter;
        readonly PerformanceCounter _ramCounter;

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
            return _cpuCounter.NextValue() + "%";
        }

        /// <summary>
        /// Gets the current machine RAM Usage
        /// </summary>
        /// <returns>The current RAM usage</returns>
        public string GetCurrentRam()
        {
            return _ramCounter.NextValue() + "MB";
        } 
    }
}
