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
        public int[] LocalDescriptorIndex { set; get; }

        public byte[] Instructions { set; get; }
    }

    internal class VMMethod
    {
        public VMClass Parent { set; get; }
        public AccessFlag Flag { set; get; }
        /// <summary>
        /// 描述符在常量池中的地址
        /// </summary>
        public uint DescriptorAddress { set; get; }
        public List<uint> LocalDescriptorAddress { set; get; }
        public HeapData CodeBlock { set; get; }
        public uint CodeAddress => MemoryMap.MapToAbsolute(CodeBlock.Offset, MemoryTag.METHOD);
    }

    public class MethodDeclarationInfo : IConstantPoolValue
    {
        #region Descriptor
        public static string GetDescriptor(VariableType retType, List<VariableType> ps)
        {
            if (retType == null)
            {
                return $"{GetParamsDescriptor(ps)}V";
            }
            else
            {
                return $"{GetParamsDescriptor(ps)}{retType.GetDescriptor()}";
            }
        }

        /// <summary>
        /// 因为调用的时候是看参数是否匹配的，所以给了一个生成参数描述的函数
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string GetParamsDescriptor(List<VariableType> ps)
        {
            StringBuilder stringBuilder = new StringBuilder("(");
            foreach (VariableType p in ps)
            {
                stringBuilder.Append(p.GetDescriptor());
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        public static bool CallMatch(string methodDescriptor, string paramsDescriptor)
        {
            if (methodDescriptor.StartsWith(paramsDescriptor))
            {
                // 目前要求完全匹配，但是不区分地址类型
                return true;
            }
            return false;
        }

        public static VariableType GetReturnType(string methodDescriptor)
        {
            methodDescriptor = methodDescriptor.Substring(methodDescriptor.LastIndexOf(')') + 1);
            return VariableType.GetType(methodDescriptor);
        }

        #endregion


        /// <summary>
        /// 如果为null表示返回void
        /// </summary>
        public VariableType ReturnType { set; get; }
        public List<VariableType> Params { set; get; }
        public int ConstantPoolIndex { get; set; }

        internal MethodDeclarationInfo(VariableType retType, List<VariableType> ps, int index)
        {
            ReturnType = retType;
            Params = ps;
            ConstantPoolIndex = index;
        }

        /// <summary>
        /// Descriptor形式和JVM相同
        /// </summary>
        /// <returns></returns>
        public string GetDescriptor()
        {
            return GetDescriptor(ReturnType, Params);
        }

        public bool Equivalent(MethodDeclarationInfo info)
        {
            if ((ReturnType == null && info.ReturnType == null) ||
                ReturnType.Equivalent(info.ReturnType) && Params.Count == info.Params.Count)
            {
                foreach ((VariableType p1, VariableType p2) in Params.Zip(info.Params))
                {
                    if (!p1.Equivalent(p2))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public class Method : IClassMember
    {
        public MethodDeclarationInfo Declaration { set; get; }
        public LinkedList<BasicBlock> BasicBlocks { get; } = new LinkedList<BasicBlock>();

        public List<Variable> Locals { get; } = new List<Variable>();
        /// <summary>
        /// 如果是非静态，会比Declaration里多一个this
        /// </summary>
        public Variable[] Params { internal set; get; }

        public Class Parent { get; set; }
        public AccessFlag AccessFlag { get; set; }
        public int ConstantPoolIndex { get; set; }
        public string Name => Parent.Parent.StringPool.ElementList[
            Parent.Parent.MethodPool.ElementList[ConstantPoolIndex - 1].Name - 1];
        public string Descriptor => Parent.Parent.StringPool.ElementList[
            Parent.Parent.MethodPool.ElementList[ConstantPoolIndex - 1].Descriptor - 1];

        internal Method(MethodDeclarationInfo type, Class parent, AccessFlag flag, int index)
        {
            Parent = parent;
            AccessFlag = flag;
            Declaration = type;
            ConstantPoolIndex = index;
        }

        internal BinaryMethod ToBinary()
        {
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
                    // offset是目的地的地址
                    BitConverter.TryWriteBytes(bb.Instructions.Last.Value.Params,
                        bb.JmpTargets[0].Offset);
                }
                else if (bb.Instructions.Last.Value.OpCode == InstructionType.JCOND)
                {
                    // offset是目的地的地址
                    BitConverter.TryWriteBytes(new Span<byte>(bb.Instructions.Last.Value.Params, 0, sizeof(int)),
                        bb.JmpTargets[0].Offset);
                    BitConverter.TryWriteBytes(new Span<byte>(bb.Instructions.Last.Value.Params, sizeof(int), sizeof(int)),
                        bb.JmpTargets[1].Offset);
                }
            }

            // 拼接每个BB的指令生成最终指令
            Stream instStream = new MemoryStream();
            foreach (Instruction inst in BasicBlocks.SelectMany(b => b.Instructions))
            {
                instStream.WriteByte((byte)inst.OpCode);
                instStream.Write(inst.Params);
            }

            BinaryMethod ret = new BinaryMethod
            {
                Instructions = new byte[instStream.Length]
            };
            instStream.Seek(0, SeekOrigin.Begin);
            instStream.Read(ret.Instructions);

            // 局部变量信息
            ret.LocalDescriptorIndex = Locals.Select(v => Parent.Parent.StringPool.TryAdd(v.Type.GetDescriptor())).ToArray();

            return ret;
        }
    }
}
