namespace XiVM.SystemLib.Classes
{
    public class ArrayType : ClassType
    {
        public VariableType ElementType { private set; get; }

        public ArrayType(VariableType elementType)
            : base($"Array<{elementType.Tag}>")
        {
            ElementType = elementType;
            AddVariable(VariableType.IntType);  // Array.Length
        }
    }
}
