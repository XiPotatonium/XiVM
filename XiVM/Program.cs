using ConsoleArgumentParser;
using XiVM.Executor;
using XiVM.Xir;

namespace XiVM
{
    internal class Program
    {
        public static BinaryModule BinaryModule { private set; get; }

        private static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());

            argumentParser.Parse(args);

            string fileName = argumentParser.GetValue().StringValue;
            BinaryModule = BinaryModule.Load(fileName);

            VMExecutor executor = new VMExecutor();
            executor.Execute();
        }
    }
}
