using System;
using System.Collections.Generic;

namespace XiVM.Xir
{
    [Serializable]
    internal class BinaryFunction
    {
        /// <summary>
        /// 函数名在Module的String常量池中的index
        /// </summary>
        public uint NameIndex { set; get; }
        public uint ParamSize { set; get; }
        public Instruction[] Instructions { set; get; }
    }

    public class FunctionType
    {
        public XirType ReturnType { set; get; }
        public List<XirType> Params { set; get; }
        public bool IsVarArg { set; get; }

        public FunctionType(XirType retType, List<XirType> ps, bool isVarArg = false)
        {
            ReturnType = retType;
            Params = ps;
            IsVarArg = isVarArg;
        }
    }

    public class Function
    {
        public string Name { set; get; }
        public FunctionType Type { set; get; }
        public List<BasicBlock> BasicBlocks { get; } = new List<BasicBlock>();
        /// <summary>
        /// 局部变量表
        /// </summary>
        public List<XirVariable> Variables { get; } = new List<XirVariable>();

        internal Function(string name, FunctionType type)
        {
            Name = name;
            Type = type;
        }

        internal BinaryFunction ToBinary()
        {
            throw new NotImplementedException();
        }
    }
}
