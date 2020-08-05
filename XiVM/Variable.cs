using XiVM.Errors;
using XiVM.Executor;

namespace XiVM
{
    public enum VariableTypeTag
    {
        BYTE = 0x00, 
        INT = 0x01, 
        DOUBLE = 0x02, 
        ADDRESS = 0x03
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
            VariableTypeTag.BYTE => MemMap.ByteSize,
            VariableTypeTag.INT => MemMap.IntSize,
            VariableTypeTag.DOUBLE => MemMap.DoubleSize,
            VariableTypeTag.ADDRESS => MemMap.AddressSize,
            _ => throw new XiVMError($"Unsupported Type {Tag}"),
        };

        /// <summary>
        /// Equivalent的类型之间允许互相赋值
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

        /// <summary>
        /// TODO ref等可以放在这里
        /// </summary>
        /// <returns></returns>
        public byte ToBinary()
        {
            return (byte)Tag;
        }
    }

    public class Variable
    {
        public VariableType Type { private set; get; }
        /// <summary>
        /// 单位为slot
        /// </summary>
        public int Offset { private set; get; }

        /// <summary>
        /// offset是VM相关的，因此不允许外部项目创建Variable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="offset"></param>
        internal Variable(VariableType type, int offset)
        {
            Type = type;
            Offset = offset;
        }
    }
}
