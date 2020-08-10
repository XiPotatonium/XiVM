using System;
using System.Collections.Generic;
using System.IO;
using XiVM.Xir;

namespace SystemLib
{
    internal class Program
    {
        public static ModuleConstructor ModuleConstructor { private set; get; }

        private static void Main(string[] args)
        {
            string moduleName = "System";
            string dirName = ".";

            ModuleConstructor = new ModuleConstructor(moduleName);

            List<AbstractClass> Classes = new List<AbstractClass>()
            {
                new System.IO.IO()
            };

            foreach (AbstractClass systemClass in Classes)
            {
                systemClass.ClassGen();
            }

            // 输出生成字节码
            ModuleConstructor.Dump(dirName);

            Console.WriteLine($"System lib generated at {Path.GetFullPath(dirName)}.");
        }
    }
}
