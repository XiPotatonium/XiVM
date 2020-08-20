using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XiVM.Runtime;

namespace XiVM
{
    public class Program
    {
        private static string DirName { set; get; } = ".";
        private static Stopwatch StaticInitWatch { get; } = new Stopwatch();
        private static Stopwatch MainWatch { get; } = new Stopwatch();

        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());
            ConsoleArgument dirArg = new ConsoleArgument("d", ArgumentValueType.STRING);
            ConsoleArgument diagnoseArg = new ConsoleArgument("diagnose");
            argumentParser.AddArgument(dirArg);
            argumentParser.AddArgument(diagnoseArg);
            argumentParser.Parse(args);

            string moduleName = argumentParser.DefaultRule.StringValue;
            if (dirArg.IsSet)
            {
                DirName = dirArg.StringValue;
            }

            VMExecutor executor = new VMExecutor(MethodArea.AddModule(LoadModule(moduleName), false));
            StaticInitWatch.Start();
            executor.ExecuteStaticConstructor();
            StaticInitWatch.Stop();
            MainWatch.Start();
            executor.ExecuteMain();
            MainWatch.Stop();
            if (diagnoseArg.IsSet)
            {
                DisplaDiagnoseIndo(executor.GetDiagnoseInfo());
            }
        }

        public static BinaryModule LoadModule(string moduleName)
        {
            return BinaryModule.Load(Path.Combine(DirName, moduleName + ".xibc"));
        }

        public static void DisplaDiagnoseIndo(ExecutorDiagnoseInfo executorDiagnoseInfo)
        {
            Console.WriteLine($"\n=================================================================\nDiagnose:");
            string[][] vs = new string[][]
            {
                new string[] { "ModulesLoadTime", $"{MethodArea.ModuleLoadTime}(ms)" },
                new string[] { "DependenciesLoadTime", $"{MethodArea.DependencyLoadTime}(ms)" },
                new string[] { "StaticInitExecutionTime", $"{StaticInitWatch.ElapsedMilliseconds}(ms)" },
                new string[] { "MainFunctionExecutionTime", $"{MainWatch.ElapsedMilliseconds}(ms)" },
                new string[] { "MainThreadStackConsumption", $"{executorDiagnoseInfo.MaxSP}/{Stack.SizeLimit}(slots)" },
                new string[] { "HeapConsumption", $"{Heap.MaxSize}/{Heap.SizeLimit}(byte)" },
                new string[] { "MethodAreaConsumption", $"{MethodArea.Size}/{MethodArea.SizeLimit}(byte)" },
            };

            foreach (var row in vs)
            {
                Console.WriteLine(string.Format("{0, 30}{1, 35}", row[0], row[1]));
            }

            Console.WriteLine($"=================================================================");
        }
    }
}
