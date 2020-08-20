using ConsoleArgumentParser;
using System.IO;
using XiVM.Runtime;

namespace XiVM
{
    public class Program
    {
        private static string DirName { set; get; } = ".";

        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());
            ConsoleArgument dirArg = new ConsoleArgument("d", ArgumentValueType.STRING);
            argumentParser.AddArgument(dirArg);
            argumentParser.Parse(args);

            string moduleName = argumentParser.DefaultRule.StringValue;
            if (dirArg.IsSet)
            {
                DirName = dirArg.StringValue;
            }

            VMExecutor executor = new VMExecutor(MethodArea.AddModule(LoadModule(moduleName)));
            executor.ExecuteStaticConstructor();
            executor.ExecuteMain();
        }

        public static BinaryModule LoadModule(string moduleName)
        {
            return BinaryModule.Load(Path.Combine(DirName, moduleName + ".xibc"));
        }
    }
}
