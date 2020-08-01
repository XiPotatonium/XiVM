namespace XiVM.SystemLib.Classes
{
    public static class String
    {
        public static ClassType StringClassType { private set; get; }
        public static ArrayType StringArrayType { private set; get; }
        public static Class StringClass { private set; get; }

        static String()
        {
            StringClassType = new ClassType("String");
            StringArrayType = new ArrayType(StringClassType);
            StringClass = new Class(StringClassType);
        }
    }
}
