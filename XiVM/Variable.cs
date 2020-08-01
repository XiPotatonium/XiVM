using XiVM.Errors;

namespace XiVM
{
    public enum VariableTypeTag
    {
        BYTE, INT, DOUBLE, ADDRESS
    }

    /// <summary>
    /// 变量的类型
    /// 函数类型和类类型是这个类的子类
    /// </summary>
    public class VariableType
    {
        public static readonly int ByteSize = sizeof(byte);
        public static readonly int IntSize = sizeof(int);
        public static readonly int DoubleSize = sizeof(double);
        public static readonly int AddressSize = sizeof(uint);

        public static readonly VariableType ByteType = new VariableType(VariableTypeTag.BYTE);
        public static readonly VariableType IntType = new VariableType(VariableTypeTag.INT);
        public static readonly VariableType DoubleType = new VariableType(VariableTypeTag.DOUBLE);

        public static readonly VariableType NullType = new VariableType(VariableTypeTag.ADDRESS);


        public VariableTypeTag Tag { private set; get; }

        protected VariableType(VariableTypeTag tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// 获取这个类型的值的大小
        /// </summary>
        public int Size => Tag switch
        {
            VariableTypeTag.BYTE => ByteSize,
            VariableTypeTag.INT => IntSize,
            VariableTypeTag.DOUBLE => DoubleSize,
            VariableTypeTag.ADDRESS => AddressSize,
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
    }

    public class Variable
    {
        public VariableType Type { private set; get; }
        public int StackOffset { private set; get; }

        /// <summary>
        /// stackOffset是VM相关的，因此不允许外部项目创建Variable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stackOffset"></param>
        internal Variable(VariableType type, int stackOffset)
        {
            Type = type;
            StackOffset = stackOffset;
        }
    }
}
