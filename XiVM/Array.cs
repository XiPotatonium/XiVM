namespace XiVM
{
    public class ArrayType : VariableType
    {
        public static readonly ArrayType ByteArrayType = new ArrayType(ByteType);
        public static readonly ArrayType IntArrayType = new ArrayType(IntType);
        public static readonly ArrayType DoubleArrayType = new ArrayType(DoubleType);

        public VariableType ElementType { private set; get; }

        public ArrayType(VariableType elementType)
            : base(VariableTypeTag.ADDRESS)
        {
            ElementType = elementType;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b is ArrayType arrayType)
            {
                return ElementType.Equivalent(arrayType.ElementType) && base.Equivalent(b);
            }
            else
            {
                return false;
            }
        }
    }
}
