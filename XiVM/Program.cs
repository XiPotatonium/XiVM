using ConsoleArgumentParser;
using XiVM.Executor;

namespace XiVM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());

            argumentParser.Parse(args);

            string fileName = argumentParser.GetValue().StringValue;

            BinaryModule binaryModule = BinaryModule.Load(fileName);
            
            VMModule module = Heap.AddModule(binaryModule);
            VMExecutor executor = new VMExecutor(module);
            executor.Execute();
        }
    }
}
