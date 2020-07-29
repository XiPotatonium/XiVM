namespace XiVM.Xir
{
    public class XirVariable
    {
        public XirType Type { private set; get; }

        internal XirVariable(XirType type)
        {
            Type = type;
        }
    }
}
