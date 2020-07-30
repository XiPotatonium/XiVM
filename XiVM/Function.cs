using System;
using System.Collections.Generic;
using System.Linq;
using XiVM.Xir;

namespace XiVM
{
    [Serializable]
    internal class BinaryFunction
    {
        public int ARSize { set; get; } = 0;
        public BinaryInstruction[] Instructions { set; get; }
    }

    public class FunctionType : VariableType
    {
        /// <summary>
        /// 如果为null表示返回void
        /// </summary>
        public VariableType ReturnType { set; get; }
        public List<VariableType> Params { set; get; }
        public bool IsVarArg { set; get; }

        public FunctionType(VariableType retType, List<VariableType> ps, bool isVarArg = false)
            : base(VariableTypeTag.ADDRESS)
        {
            ReturnType = retType;
            Params = ps;
            IsVarArg = isVarArg;
        }

        /// <summary>
        /// 要求都是函数，以及返回值类型相等，以及参数类型相等
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public override bool Equivalent(VariableType b)
        {
            if (b is FunctionType functionType)
            {
                if ((ReturnType == null && functionType.ReturnType == null) ||
                    ReturnType.Equivalent(functionType.ReturnType) && Params.Count == functionType.Params.Count)
                {
                    foreach ((var p1, var p2) in Params.Zip(functionType.Params))
                    {
                        if (!p1.Equivalent(p2))
                        {
                            return false;
                        }
                    }
                    return base.Equivalent(b);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class Function
    {
        /// <summary>
        /// 函数的在全局函数表中的Index + 1
        /// Call的时候就是call这个index
        /// 注意因为要和NULL区分因此Index + 1
        /// </summary>
        public uint Index { private set; get; }
        public string Name { set; get; }
        public FunctionType Type { set; get; }
        public List<BasicBlock> BasicBlocks { get; } = new List<BasicBlock>();
        /// <summary>
        /// 局部变量表
        /// </summary>
        public List<Variable> Variables { get; } = new List<Variable>();

        internal Function(uint index, string name, FunctionType type)
        {
            Index = index;
            Name = name;
            Type = type;
        }

        internal BinaryFunction ToBinary()
        {
            BinaryFunction binaryFunction = new BinaryFunction();

            // 根据局部变量信息计算AR大小
            foreach (Variable localVar in Variables)
            {
                // 不必担心是void，因为Variable的产生排除了void
                binaryFunction.ARSize += localVar.Type.Size;
            }

            // 优化：可以检查每个BB最后是不是br

            // TODO: 遍历各个BasicBlock的指令，将带label的指令转换为正确的displacement

            // 拼接每个BB的指令生成最终指令
            int instructionCount = BasicBlocks.Sum(b => b.Instructions.Count);
            binaryFunction.Instructions = new BinaryInstruction[instructionCount];
            int i = 0;
            foreach (BasicBlock bb in BasicBlocks)
            {
                foreach (Instruction inst in bb.Instructions)
                {
                    binaryFunction.Instructions[i] = new BinaryInstruction()
                    {
                        OpCode = (byte)inst.OpCode,
                        Params = inst.Params
                    };
                    ++i;
                }
            }

            return binaryFunction;
        }
    }
}
