﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XiVM.Tests
{
    [TestClass()]
    public class XiVMTests
    {
        [TestMethod()]
        public void MainTest()
        {
            Program.Main(new string[] { "Test1", "-d", "D:/Usr/XiVM/XiLangTests/TestSources" });
        }
    }
}