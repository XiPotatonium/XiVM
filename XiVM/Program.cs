﻿using ConsoleArgumentParser;
using XiVM.Executor;
using XiVM.Xir;

namespace XiVM
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());

            argumentParser.Parse(args);

            string fileName = argumentParser.GetValue().StringValue;
            BinaryModule binaryModule = BinaryModule.Load(fileName);

            VMExecutor executor = new VMExecutor(binaryModule);
            executor.Execute();
        }
    }
}
