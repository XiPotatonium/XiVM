using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.Executor;
using XiVM.Xir.Symbol;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        private string Name { set; get; }

        /// <summary>
        /// 函数表
        /// </summary>
        private List<Function> Functions { get; } = new List<Function>();
        public Function MainFunction { set; get; }
        public Function CurrentFunction => CurrentBasicBlock?.Value.Function;
        public LinkedListNode<BasicBlock> CurrentBasicBlock { set; get; }
        private LinkedList<Instruction> CurrentInstructions => CurrentBasicBlock?.Value.Instructions;
        public Dictionary<string, ClassType> Classes { private set; get; }

        public ConstantTable<string> StringLiterals { get; } = new ConstantTable<string>();

        /// <summary>
        /// 符号栈
        /// </summary>
        public SymbolTable SymbolTable { get; } = new SymbolTable();


        public ModuleConstructor(string name)
        {
            Name = name;

            // 添加全局代码
            Function global = new Function(0, null, null);
            Functions.Add(global);

            ImportSystemFunctions();

            CurrentBasicBlock = AddBasicBlock(global);
        }

        /// <summary>
        /// 用比较Dirty的方法设置系统函数，以后有import功能后将系统函数做成一个模块
        /// </summary>
        private void ImportSystemFunctions()
        {
            Function putchar = AddFunction("putchar", new FunctionType(null, new List<VariableType>() { VariableType.IntType }));
            CurrentBasicBlock = AddBasicBlock(putchar);
            AddLocalA(putchar.Params[0].Offset);
            AddLoadI();
            AddPutC();
            AddRet();
        }

        public void Dump(string dirName, bool dumpXir = true)
        {
            CurrentBasicBlock = Functions[0].BasicBlocks.First;
            if (MainFunction != null)
            {
                AddPushA(0);    // 暂时给main函数传NULL
                AddCall(MainFunction.Index);
            }
            AddRet();           // 为了满足bb的要求，全局也ret一下

            if (string.IsNullOrEmpty(dirName))
            {
                dirName = ".";
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            BinaryModule binaryModule = new BinaryModule
            {
                Functions = Functions.Select(f => f.ToBinary()).ToArray(),
                // TODO Class
                StringLiterals = StringLiterals.ToArray()
            };

            using (FileStream fs = new FileStream(Path.Combine(dirName, $"{Name}.xibc"), FileMode.Create))
            {
                binaryFormatter.Serialize(fs, binaryModule);
            }

            if (dumpXir)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(dirName, $"{Name}.xir")))
                {
                    sw.WriteLine($"# {Name}");

                    sw.WriteLine($"\n.global:");
                    foreach (Instruction inst in Functions[0].BasicBlocks.First.Value.Instructions)
                    {
                        sw.WriteLine(inst.ToString());
                    }

                    for (int i = 1; i < Functions.Count; ++i)
                    {
                        sw.WriteLine($"\n.f{i}:\t# {Functions[i].Name}");
                        foreach (BasicBlock bb in Functions[i].BasicBlocks)
                        {
                            foreach (Instruction inst in bb.Instructions)
                            {
                                sw.WriteLine(inst.ToString());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 函数会被加入符号表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Function AddFunction(string name, FunctionType type)
        {
            Function function = new Function((uint)Functions.Count, name, type);
            Functions.Add(function);

            // 添加参数
            int offset = 0;
            foreach (VariableType paramType in function.Type.Params)
            {
                offset -= paramType.Size;
                function.Params.Add(new Variable(paramType, offset));
            }

            // 加入符号表
            SymbolTable.Add(name, new FunctionSymbol(name, function));

            return function;
        }

        /// <summary>
        /// 给函数的参数提供一个名字，同时也会将该参数加入符号表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="name"></param>
        public void SetFunctionParamName(Variable param, string name)
        {
            VariableSymbol ret = new VariableSymbol(name, param);
            SymbolTable.Add(name, ret);
        }

        public LinkedListNode<BasicBlock> AddBasicBlock(Function function)
        {
            return function.BasicBlocks.AddLast(new BasicBlock(function));
        }

        public LinkedListNode<BasicBlock> InsertBeforeBasicBlock(LinkedListNode<BasicBlock> basicBlock)
        {
            return basicBlock.List.AddBefore(basicBlock, new BasicBlock(basicBlock.Value.Function));
        }

        public Variable AddLocalVariable(string id, VariableType type)
        {
            Variable xirVariable;
            if (CurrentFunction.Locals.Count == 0)
            {
                // 第一个局部变量
                xirVariable = new Variable(type, Stack.MiscDataSize);
            }
            else
            {
                xirVariable = new Variable(type, CurrentFunction.Locals[^1].Offset + CurrentFunction.Locals[^1].Type.Size);
            }
            CurrentFunction.Locals.Add(xirVariable);

            // 添加到符号表
            SymbolTable.Add(id, new VariableSymbol(id, xirVariable));

            return xirVariable;
        }
    }
}
