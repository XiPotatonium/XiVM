using System;
using System.Collections.Generic;
using System.Linq;
using XiVM.Errors;
using XiVM.Xir;

namespace XiVM
{
    [Serializable]
    internal class BinaryFunction
    {
        public byte[] LocalTypes { set; get; }
        public byte[] ParamTypes { set; get; }
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
                    foreach ((VariableType p1, VariableType p2) in Params.Zip(functionType.Params))
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
        /// 函数的在全局函数表中的Index
        /// Call的时候就是call这个index
        /// Global的Index是0，可以根据这个进行特殊处理
        /// </summary>
        public uint Index { private set; get; }
        public string Name { set; get; }
        public FunctionType Type { set; get; }
        public LinkedList<BasicBlock> BasicBlocks { get; } = new LinkedList<BasicBlock>();

        public List<Variable> Locals { get; } = new List<Variable>();
        public List<Variable> Params { get; } = new List<Variable>();

        internal Function(uint index, string name, FunctionType type)
        {
            Index = index;
            Name = name;
            Type = type;
        }

        internal BinaryFunction ToBinary()
        {
            BinaryFunction binaryFunction = new BinaryFunction();

            binaryFunction.ParamTypes = Params.Select(v => v.Type.ToBinary()).ToArray();
            binaryFunction.LocalTypes = Locals.Select(v => v.Type.ToBinary()).ToArray();

            // 检查每个BB最后是不是br
            foreach (BasicBlock basicBlock in BasicBlocks)
            {
                foreach (Instruction inst in basicBlock.Instructions)
                {
                    if ((inst.IsBranch && inst != basicBlock.Instructions.Last.Value) ||
                        (!inst.IsBranch && inst == basicBlock.Instructions.Last.Value))
                    {
                        throw new XiVMError($"Basic Block of function {Name} is not ended with br");
                    }
                }
            }

            // 遍历各个BasicBlock的指令，将带label的指令转换为正确的displacement
            int offset = 0;
            foreach (BasicBlock bb in BasicBlocks)
            {
                // 计算每个BasicBlock在函数中的offset
                bb.Offset = offset;
                offset += bb.Instructions.Count;
            }
            foreach (BasicBlock bb in BasicBlocks)
            {
                if (bb.Instructions.Last.Value.OpCode == InstructionType.JMP)
                {
                    // offset是目的地的地址减Next IP
                    BitConverter.TryWriteBytes(bb.Instructions.Last.Value.Params,
                        bb.JmpTargets[0].Offset - (bb.Offset + bb.Instructions.Count));
                }
                else if (bb.Instructions.Last.Value.OpCode == InstructionType.JCOND)
                {
                    // offset是目的地的地址减Next IP
                    BitConverter.TryWriteBytes(bb.Instructions.Last.Value.Params,
                        bb.JmpTargets[0].Offset - (bb.Offset + bb.Instructions.Count));
                    BitConverter.TryWriteBytes(new Span<byte>(bb.Instructions.Last.Value.Params, sizeof(int), sizeof(int)),
                        bb.JmpTargets[1].Offset - (bb.Offset + bb.Instructions.Count));
                }
            }

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
