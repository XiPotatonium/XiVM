using System;
using System.Collections.Generic;
using System.IO;
using XiVM.Xir;

namespace SystemLib
{
    public class Program
    {
        public static readonly string ModuleName = "System";

        internal static ModuleConstructor ModuleConstructor { private set; get; }


        private static void Main(string[] args)
        {
            string dirName = ".";

            ModuleConstructor = new ModuleConstructor(ModuleName);

            List<AbstractClass> Classes = new List<AbstractClass>()
            {
                new System.IO.IO(),
                new System.String.String()
            };

            foreach (AbstractClass systemClass in Classes)
            {
                systemClass.DeclarationGen();
                foreach (var systemMethod in systemClass.Methods)
                {
                    systemMethod.DeclarationGen();
                }
            }

            foreach (AbstractClass systemClass in Classes)
            {
                systemClass.CodeGen();
            }

            // 输出生成字节码
            ModuleConstructor.Dump(dirName);

            Console.WriteLine($"System lib generated at {Path.GetFullPath(dirName)}.");
        }
    }
}
