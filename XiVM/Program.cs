using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XiVM.Errors;
using XiVM.Runtime;

namespace XiVM
{
    public class Program
    {
        private static string DirName { set; get; } = ".";
        private static Stopwatch StaticInitWatch { get; } = new Stopwatch();

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

            // 1 模块加载
            VMModule mainModule = ModuleLoader.AddModule(LoadModule(moduleName), false);

            // 2 执行静态构造
            StaticInitWatch.Start();
            foreach (var vmClass in ModuleLoader.Classes.Values)
            {
                vmClass.Methods.TryGetValue(MethodArea.Singleton.StaticConstructorNameAddress, out List<VMMethod> sinit);
                VMExecutor executor = ThreadManager.CreateThread();
                executor.Execute(sinit[0]);
                // TODO 静态构造是可以多线程的，但是如何回收堆栈呢？
                ThreadManager.CollectThreadSpace(executor);
            }
            StaticInitWatch.Stop();

            // 3 执行Main函数
            if (!mainModule.Classes.TryGetValue(MethodArea.Singleton.StringProgramAddress, out VMClass entryClass))
            {
                throw new XiVMError("Program.Main() not found");
            }
            if (!entryClass.Methods.TryGetValue(MethodArea.Singleton.StringMainAddress, out List<VMMethod> entryMethodGroup))
            {
                throw new XiVMError("Program.Main() not found");
            }
            VMMethod entryMethod = entryMethodGroup.Find(m => m.DescriptorAddress == MethodArea.Singleton.StringMainDescriptorAddress);
            if (entryMethod == null)
            {
                throw new XiVMError("Program.Main(System.String) not found");
            }
            VMExecutor mainThread = ThreadManager.CreateThread();
            mainThread.Execute(entryMethod, null);

            // 4 诊断信息
            if (diagnoseArg.IsSet)
            {
                DisplaDiagnoseIndo(mainThread.GetDiagnoseInfo());
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
                new string[] { "ModulesLoadTime", $"{ModuleLoader.ModuleLoadTime}(ms)" },
                new string[] { "DependenciesLoadTime", $"{ModuleLoader.DependencyLoadTime}(ms)" },

                new string[] { "StaticInitExecutionTime", $"{StaticInitWatch.ElapsedMilliseconds}(ms)" },
                new string[] { "MainThreadExecutionTime", $"{executorDiagnoseInfo.ExecutionTime}(ms)" },

                new string[] { "MainThreadStackConsumption", $"{executorDiagnoseInfo.MaxSP}/{Stack.SizeLimit}(slots)" },
                new string[] { "HeapConsumption",
                    $"{Math.Round((double)Heap.Singleton.MaxSize / 1024, 2)}/{Math.Round((double)Heap.SizeLimit / 1024, 2)}(MB)" },
                new string[] { "StaticAreaConsumption",
                    $"{Math.Round((double) StaticArea.Singleton.MaxSize / 1024, 2)}/{Math.Round((double) StaticArea.SizeLimit / 1024, 2)}(MB)" },
                new string[] { "MethodAreaConsumption",
                    $"{Math.Round((double) MethodArea.Singleton.Size / 1024, 2)}/{Math.Round((double) MethodArea.SizeLimit / 1024, 2)}(MB)" },

                new string[] { "GCTotalTime", $"{GarbageCollector.GCTotalTime}(ms)" },
                new string[] { "GCAverageTime",
                    $"{Math.Round(GarbageCollector.GCCount == 0 ? 0 : (double)GarbageCollector.GCTotalTime / GarbageCollector.GCCount, 2)}(ms)" },
                new string[] { "GCMaxTime", $"{GarbageCollector.GCMaxTime}(ms)" },
                new string[] { "FreedSize", $"{GarbageCollector.FreedSize}(MB)" },
            };

            foreach (var row in vs)
            {
                Console.WriteLine(string.Format("{0, 30}{1, 30}", row[0], row[1]));
            }

            Console.WriteLine($"=================================================================");
        }
    }
}
