﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XiLang.Tests
{
    [TestClass()]
    public class XiLangTests
    {
        [TestMethod()]
        public void MainTest()
        {
            Program.Main(new string[] { "Test0", "-d", "D:/Usr/XiVM/XiLangTests/TestSources", "-verbose" });
        }
    }
}