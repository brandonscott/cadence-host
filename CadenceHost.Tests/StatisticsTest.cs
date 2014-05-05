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
    }
}
