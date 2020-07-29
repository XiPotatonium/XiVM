using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        private string Name { set; get; }
        private List<Function> Functions { set; get; } = new List<Function>();
        public BasicBlock CurrentBasicBlock { set; get; }


        public ModuleConstructor(string name)
        {
            Name = name;
        }

        public void Dump(string dirName)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                dirName = ".";
            }
            using (FileStream fs = new FileStream(Path.Combine(dirName, $"{Name}.xir"), FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                BinaryModule binaryModule = new BinaryModule();
                throw new NotImplementedException();
                binaryFormatter.Serialize(fs, binaryModule);
            }
        }

        public Function AddFunction(string name, FunctionType type)
        {
            Function function = new Function(name, type);
            Functions.Add(function);
            return function;
        }

        public BasicBlock AddBasicBlock(Function function)
        {
            BasicBlock bb = new BasicBlock(function);
            function.BasicBlocks.Add(bb);
            return bb;
        }

        public XirVariable AddVariable(XirType type)
        {
            XirVariable xirVariable = new XirVariable(type);
            CurrentBasicBlock.Function.Variables.Add(xirVariable);
            return xirVariable;
        }
    }
}
