using XiVM.Errors;

namespace XiVM
{
    public class ArrayType : VariableType
    {
        public static readonly ArrayType ByteArrayType = new ArrayType(ByteType);
        public static readonly ArrayType IntArrayType = new ArrayType(IntType);
        public static readonly ArrayType DoubleArrayType = new ArrayType(DoubleType);

        public static ArrayType GetArrayType(string descriptor)
        {
            if (descriptor[0] != '[')
            {
                throw new XiVMError($"{descriptor} is not an array descriptor");
            }
            return new ArrayType(VariableType.GetType(descriptor.Substring(1)));
        }

        public VariableType ElementType { private set; get; }

        public ArrayType(VariableType elementType)
            : base(VariableTypeTag.ADDRESS)
        {
            ElementType = elementType;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }

            if (b is ArrayType arrayType)
            {
                return ElementType.Equivalent(arrayType.ElementType);
            }
            else
            {
                return false;
            }
        }

        public override string GetDescriptor()
        {
            return $"[{ElementType.GetDescriptor()}";
        }
    }
}
