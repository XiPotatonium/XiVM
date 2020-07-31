namespace XiVM
{
    public class ArrayType : VariableType
    {
        public static readonly VariableType ByteArrayType = new ArrayType(ByteType);
        public static readonly VariableType IntArrayType = new ArrayType(IntType);
        public static readonly VariableType DoubleArrayType = new ArrayType(DoubleType);

        public VariableType ElementType { private set; get; }

        public ArrayType(VariableType elementType)
            : base(VariableTypeTag.ADDRESS)
        {
            ElementType = elementType;
        }
    }

    public class Array
    {
    }
}
