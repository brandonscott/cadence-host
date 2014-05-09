using System;
using System.Threading;
using CadenceHost.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CadenceHost.Tests
{
    [TestClass]
    public class StatisticsTest
    {
        [TestMethod]
        public void TestCpu()
        {
            var statsHelper = new Statistics();
            Assert.IsNotNull(statsHelper.GetCurrentCpu());
       }

        [TestMethod]
        public void TestOsName()
        {
            var statsHelper = new Statistics();
            Assert.AreEqual(statsHelper.GetOsName(),"Microsoft Windows 8.1 Pro");
        }

        [TestMethod]
        public void TestOsVersion()
        {
            var statsHelper = new Statistics();
            Assert.AreEqual(statsHelper.GetOsVersion(), "6.3.9600");
        }

        [TestMethod]
        public void TestTotalDiskSpace()
        {
            var statsHelper = new Statistics();
            Assert.IsNotNull(statsHelper.GetTotalDiskStorage());
        }
    }
}
