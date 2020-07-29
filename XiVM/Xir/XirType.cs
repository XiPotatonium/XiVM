namespace XiVM.Xir
{
    public enum XirTypeTag
    {
        BYTE, INT, DOUBLE, STRING, USER_DEF
    }

    public class XirType
    {
        public XirTypeTag Tag { private set; get; }
        public bool IsArray { private set; get; }
        /// <summary>
        /// 用户定义类型的名字
        /// </summary>
        public string Name { set; get; }
    }
}
