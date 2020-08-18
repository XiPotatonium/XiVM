using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XiVM.Tests
{
    [TestClass()]
    public class XiVMTests
    {
        [TestMethod()]
        public void MainTest()
        {
            Program.Main(new string[] { "HelloWorld", "-d", "D:/Usr/XiVM/XiLangTests/TestSources" });
        }
    }
}