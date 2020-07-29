namespace XiVM.Xir
{
    public enum XirTypeTag
    {
        VOID, BYTE, INT, DOUBLE, STRING, CLASS
    }

    public class XirType
    {
        public static XirType XirVoidType = new XirType() { Tag = XirTypeTag.VOID };
        public static XirType XirByteType = new XirType() { Tag = XirTypeTag.BYTE };
        public static XirType XirByteArrayType = new XirType() { Tag = XirTypeTag.BYTE, IsArray = true };
        public static XirType XirInt32Type = new XirType() { Tag = XirTypeTag.INT };
        public static XirType XirInt32ArrayType = new XirType() { Tag = XirTypeTag.INT, IsArray = true };
        public static XirType XirDoubleType = new XirType() { Tag = XirTypeTag.DOUBLE };
        public static XirType XirDoubleArrayType = new XirType() { Tag = XirTypeTag.DOUBLE, IsArray = true };
        public static XirType XirStringType = new XirType() { Tag = XirTypeTag.STRING };
        public static XirType XirStringArrayType = new XirType() { Tag = XirTypeTag.STRING, IsArray = true };
        public static XirType XirClassType(string name, bool isArray)
        {
            return new XirType()
            {
                IsArray = isArray,
                Tag = XirTypeTag.CLASS,
                Name = name
            };
        }

        public XirTypeTag Tag { private set; get; }
        public bool IsArray { private set; get; } = false;
        /// <summary>
        /// 用户定义类型的名字
        /// </summary>
        public string Name { set; get; }

        internal XirType() { }
    }
}
