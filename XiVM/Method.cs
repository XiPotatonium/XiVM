using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XiVM.Errors;
using XiVM.Runtime;
using XiVM.Xir;

namespace XiVM
{
    [Serializable]
    public class BinaryMethod
    {
        public int ConstantPoolIndex { set; get; }
        public int LocalDescriptorIndex { set; get; }

        public byte[] Instructions { set; get; }
    }

    internal class VMMethod
    {
        public VMClass Parent { set; get; }
        /// <summary>
        /// 描述符在常量池中的地址
        /// </summary>
        public uint DescriptorAddress { set; get; }
        public uint LocalDescriptorAddress { set; get; }
        /// <summary>
        /// VMMethod在MethodArea的MethodIndexTable中的Index
        /// </summary>
        public int MethodIndex { set; get; }
        public LinkedListNode<HeapData> CodeBlock { set; get; }
    }

    public class MethodType : VariableType, IConstantPoolValue
    {
        public static string GetDescriptor(VariableType retType, List<VariableType> ps)
        {
            StringBuilder stringBuilder = new StringBuilder("(");
            foreach (VariableType p in ps)
            {
                stringBuilder.Append(p.ToString());
            }
            stringBuilder.Append(")");
            stringBuilder.Append(retType == null ? "V" : retType.ToString());
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 如果为null表示返回void
        /// </summary>
        public VariableType ReturnType { set; get; }
        public List<VariableType> Params { set; get; }
        public int ConstantPoolIndex { get; set; }

        internal MethodType(VariableType retType, List<VariableType> ps, int index)
            : base(VariableTypeTag.ADDRESS)
        {
            ReturnType = retType;
            Params = ps;
            ConstantPoolIndex = index;
        }

        /// <summary>
        /// 要求都是函数，以及返回值类型相等，以及参数类型相等
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public override bool Equivalent(VariableType b)
        {
            if (b is MethodType functionType)
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

        /// <summary>
        /// Descriptor形式和JVM相同
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetDescriptor(ReturnType, Params);
        }
    }

    public class Method : IClassMember
    {
        public MethodType Type { set; get; }
        public LinkedList<BasicBlock> BasicBlocks { get; } = new LinkedList<BasicBlock>();

        public List<Variable> Locals { get; } = new List<Variable>();
        public List<Variable> Params { get; } = new List<Variable>();

        public ClassType Parent { get; set; }
        public AccessFlag AccessFlag { get; set; }
        public int ConstantPoolIndex { get; set; }
        public int LocalDescriptorIndex { set; get; }
        public string Name => Parent.Parent.StringPool.ElementList[
            Parent.Parent.MemberPool.ElementList[ConstantPoolIndex - 1].Name - 1];
        public string Descriptor => Parent.Parent.StringPool.ElementList[
            Parent.Parent.MemberPool.ElementList[ConstantPoolIndex - 1].Type - 1];

        internal Method(MethodType type, ClassType parent, AccessFlag flag, int index)
        {
            Parent = parent;
            AccessFlag = flag;
            Type = type;
            ConstantPoolIndex = index;
        }

        internal BinaryMethod ToBinary()
        {
            BinaryMethod binaryMethod = new BinaryMethod()
            {
                ConstantPoolIndex = ConstantPoolIndex,
                LocalDescriptorIndex = LocalDescriptorIndex
            };

            // 检查每个BB最后是不是br
            foreach (BasicBlock basicBlock in BasicBlocks)
            {
                foreach (Instruction inst in basicBlock.Instructions)
                {
                    if ((inst.IsBranch && inst != basicBlock.Instructions.Last.Value) ||
                        (!inst.IsBranch && inst == basicBlock.Instructions.Last.Value))
                    {
                        throw new XiVMError($"Basic Block is not ended with br");
                    }
                }
            }

            // 遍历各个BasicBlock的指令，将带label的指令转换为正确的displacement
            int offset = 0;
            foreach (BasicBlock bb in BasicBlocks)
            {
                // 计算每个BasicBlock在函数中的offset
                bb.Offset = offset;
                bb.InstLength = 0;
                foreach (Instruction inst in bb.Instructions)
                {
                    bb.InstLength += 1;
                    if (inst.Params != null)
                    {
                        bb.InstLength += inst.Params.Length;
                    }
                }
                offset += bb.InstLength;
            }
            foreach (BasicBlock bb in BasicBlocks)
            {
                if (bb.Instructions.Last.Value.OpCode == InstructionType.JMP)
                {
                    // offset是目的地的地址减Next IP
                    BitConverter.TryWriteBytes(bb.Instructions.Last.Value.Params,
                        bb.JmpTargets[0].Offset - (bb.Offset + bb.InstLength));
                }
                else if (bb.Instructions.Last.Value.OpCode == InstructionType.JCOND)
                {
                    // offset是目的地的地址减Next IP
                    BitConverter.TryWriteBytes(new Span<byte>(bb.Instructions.Last.Value.Params, 0, sizeof(int)),
                        bb.JmpTargets[0].Offset - (bb.Offset + bb.InstLength));
                    BitConverter.TryWriteBytes(new Span<byte>(bb.Instructions.Last.Value.Params, sizeof(int), sizeof(int)),
                        bb.JmpTargets[1].Offset - (bb.Offset + bb.InstLength));
                }
            }

            // 拼接每个BB的指令生成最终指令
            Stream instStream = new MemoryStream();
            foreach (Instruction inst in BasicBlocks.SelectMany(b => b.Instructions))
            {
                instStream.WriteByte((byte)inst.OpCode);
                instStream.Write(inst.Params);
            }
            binaryMethod.Instructions = new byte[instStream.Length];
            instStream.Seek(0, SeekOrigin.Begin);
            instStream.Read(binaryMethod.Instructions);

            return binaryMethod;
        }
    }
}
