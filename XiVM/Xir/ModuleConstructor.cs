using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        private Function MainFunction { set; get; }
        public BasicBlock CurrentBasicBlock { set; get; }

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

            // 添加全局代码
            Function global = new Function(0, null, null);
            Functions.Add(global);
            CurrentBasicBlock = AddBasicBlock(global);
        }

        public void Dump(string dirName)
        {
            CurrentBasicBlock = Functions[0].BasicBlocks[0];
            if (MainFunction != null)
            {
                AddPushA(0);    // 暂时给main函数传NULL
                AddPushA(MainFunction.Index);
                AddCall();
            }
            AddRet();           // 为了满足bb的要求，全局也ret一下

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
            Function function = new Function((uint)Functions.Count, name, type);
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

            if (SymbolTable.Count == 1 && name == "main")
            {
                // 是main函数
                MainFunction = function;
            }

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
            Variable xirVariable;
            if (CurrentBasicBlock.Function.Variables.Count == 0)
            {
                xirVariable = new Variable(type, 0);
            }
            else
            {
                xirVariable = new Variable(type,
                    CurrentBasicBlock.Function.Variables[^1].Offset + CurrentBasicBlock.Function.Variables[^1].Type.Size);
            }
            CurrentBasicBlock.Function.Variables.Add(xirVariable);

            // 添加到符号表
            SymbolTable.Add(id, new VariableSymbol(id, xirVariable));

            return xirVariable;
        }
    }
}
