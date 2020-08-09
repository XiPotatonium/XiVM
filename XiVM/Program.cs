using ConsoleArgumentParser;
using XiVM.Runtime;

namespace XiVM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());

            argumentParser.Parse(args);

            string fileName = argumentParser.GetValue().StringValue;

            VMExecutor executor = new VMExecutor(MethodArea.AddModule(BinaryModule.Load(fileName)));
            executor.Execute();
        }
    }
}
