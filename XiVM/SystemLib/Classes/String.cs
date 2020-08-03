using System.Collections.Generic;

namespace XiVM.SystemLib.Classes
{
    public static class String
    {
        public static ClassType StringClassType { private set; get; }
        public static ArrayType StringArrayType { private set; get; }
        public static readonly LinkedList<Instruction> StringConstructor = new LinkedList<Instruction>();

        static String()
        {
            StringClassType = new ClassType("String");
            StringArrayType = new ArrayType(StringClassType);

            StringClassType.AddVariable(VariableType.IntType);                  // String.Length
            StringClassType.AddVariable(new ArrayType(VariableType.ByteType));  // 数据
        }
    }
}
