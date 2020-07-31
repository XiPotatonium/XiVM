namespace XiVM.SystemLib.Classes
{
    public static class XirString
    {
        public static ClassType StringClassType { private set; get; }
        public static ArrayType StringArrayType { private set; get; }
        public static Class String { private set; get; }

        static XirString()
        {
            StringClassType = new ClassType("String");
            StringArrayType = new ArrayType(StringClassType);
            String = new Class(StringClassType);
        }
    }
}
