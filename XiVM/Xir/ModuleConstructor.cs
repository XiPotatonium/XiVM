using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.Errors;
using XiVM.Xir.Symbol;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        private string Name { set; get; }

        /// <summary>
        /// 函数表
        /// </summary>
        private List<Function> Functions { set; get; } = new List<Function>();
        public BasicBlock CurrentBasicBlock { set; get; }

        /// <summary>
        /// 全局代码（以函数形式储存）
        /// </summary>
        private Function Global { set; get; }
        private BasicBlock GlobalBasicBlock { set; get; }

        /// <summary>
        /// 常量表
        /// </summary>
        private List<BinaryConstant> Constants { set; get; } = new List<BinaryConstant>();

        /// <summary>
        /// 符号栈
        /// </summary>
        public SymbolTable SymbolTable { get; } = new SymbolTable();


        public ModuleConstructor(string name)
        {
            Name = name;

            // 在符号栈中找不到Global数所以参数随便填都是安全的
            Global = new Function(0, null, null);
            GlobalBasicBlock = CurrentBasicBlock = AddBasicBlock(Global);
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
                BinaryModule binaryModule = new BinaryModule
                {
                    Functions = Functions.Select(f => f.ToBinary()).ToArray(),
                    Entry = Global.ToBinary(),
                    // TODO Class
                    Constants = Constants.ToArray()
                };

                binaryFormatter.Serialize(fs, binaryModule);
            }
        }

        /// <summary>
        /// 注意会默认生成一个BasicBlock，是函数的entry
        /// 同时函数的Variable里会生成形参，不需要自行创建
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Function AddFunction(string name, FunctionType type)
        {
            Function function = new Function((uint)Functions.Count + 1, name, type);
            Functions.Add(function);

            // entry
            CurrentBasicBlock = new BasicBlock(function);
            function.BasicBlocks.Add(CurrentBasicBlock);

            // 创建形参
            int offset = 0;
            foreach (VariableType paramType in type.Params)
            {
                function.Variables.Add(new Variable(paramType, offset));
                // 创建传参代码，Call的时候已经把参数值准备好了
                // 注意参数是倒序进栈的
                // 不必担心是void，因为Variable的产生排除了void
                AddGetA(0, offset);
                AddStoreT(paramType);
                offset += paramType.Size;
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

        public BasicBlock AddBasicBlock(Function function)
        {
            BasicBlock bb = new BasicBlock(function);
            function.BasicBlocks.Add(bb);
            return bb;
        }

        public Variable AddVariable(string id, VariableType type)
        {
            Function currentFunction = CurrentBasicBlock == null ? Global : CurrentBasicBlock.Function;
            Variable xirVariable;
            if (currentFunction.Variables.Count == 0)
            {
                xirVariable = new Variable(type, 0);
            }
            else
            {
                xirVariable = new Variable(type, currentFunction.Variables[^1].Offset + currentFunction.Variables[^1].Type.Size);
            }
            currentFunction.Variables.Add(xirVariable);

            // 添加到符号表
            SymbolTable.Add(id, new VariableSymbol(id, xirVariable));

            return xirVariable;
        }
    }
}
