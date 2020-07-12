using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XiLang.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        [TestMethod()]
        public void MainTest()
        {
            Program.Main(new string[] { "Test0", "-d", "D:/Usr/XiVM/XiLangTests/TestSources", "-json" });
        }
    }
}