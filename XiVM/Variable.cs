using System;
using XiVM.Runtime;
using XiVM.Xir;

namespace XiVM
{
    public enum VariableTypeTag
    {
        INVALID,
        BYTE,
        INT,
        DOUBLE,
        ADDRESS
    }

    /// <summary>
    /// 变量的类型
    /// 函数类型和类类型是这个类的子类
    /// </summary>
    public class VariableType
    {
        public static readonly VariableType ByteType = new VariableType(VariableTypeTag.BYTE);
        public static readonly VariableType IntType = new VariableType(VariableTypeTag.INT);
        public static readonly VariableType DoubleType = new VariableType(VariableTypeTag.DOUBLE);

        /// <summary>
        /// 不要用这个Type来表示class，因为他仅带有size信息无任何关于class的信息
        /// 只用于null等特殊用途
        /// </summary>
        public static readonly VariableType AddressType = new VariableType(VariableTypeTag.ADDRESS);

        #region Descriptor


        public static VariableType GetType(string descriptor)
        {
            switch (descriptor[0])
            {
                case 'B':
                    return ByteType;
                case 'I':
                    return IntType;
                case 'D':
                    return DoubleType;
                case 'L':
                    return ObjectType.GetObjectType(descriptor);
                case 'V':
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual string GetDescriptor()
        {
            return Tag switch
            {
                VariableTypeTag.BYTE => "B",
                VariableTypeTag.INT => "I",
                VariableTypeTag.DOUBLE => "D",
                _ => throw new NotImplementedException(),
            };
        }

        #endregion


        public VariableTypeTag Tag { private set; get; }

        protected VariableType(VariableTypeTag tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// 获取这个类型占几个Slot
        /// </summary>
        public int SlotSize => Tag switch
        {
            VariableTypeTag.BYTE => MemoryMap.ByteSize,
            VariableTypeTag.INT => MemoryMap.IntSize,
            VariableTypeTag.DOUBLE => MemoryMap.DoubleSize,
            VariableTypeTag.ADDRESS => MemoryMap.AddressSize,
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// 获取这个类型占几个byte
        /// </summary>
        public int Size => Tag switch
        {
            VariableTypeTag.BYTE => sizeof(byte),
            VariableTypeTag.INT => sizeof(int),
            VariableTypeTag.DOUBLE => sizeof(double),
            VariableTypeTag.ADDRESS => sizeof(uint),
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Equivalent的类型之间允许（无任何转换）赋值
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public virtual bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }
            return Tag == b.Tag;
        }
    }

    public class Variable
    {
        public VariableType Type { private set; get; }
        /// <summary>
        /// 如果这个Variable是栈上的，offset单位为slot
        /// 如果是堆上的，offset单位为byte
        /// </summary>
        public int Offset { internal set; get; }

        internal Variable(VariableType type)
        {
            Type = type;
        }
    }

    public class ClassField : Variable, IClassMember
    {
        public Class Parent { get; set; }
        public AccessFlag AccessFlag { get; set; }
        public int ConstantPoolIndex { get; set; }

        internal ClassField(AccessFlag flag, Class parent, VariableType type, int index)
            : base(type)
        {
            Parent = parent;
            AccessFlag = flag;
            ConstantPoolIndex = index;
        }
    }

    internal class VMClassField : Variable
    {
        public AccessFlag AccessFlag { get; set; }

        internal VMClassField(uint flag, VariableType type, int offset)
            : base(type)
        {
            Offset = offset;
            AccessFlag = new AccessFlag() { Flag = flag };
        }
    }
}
