using XiVM.Xir;

namespace XiLang.Symbol
{
    internal class VariableSymbol : Symbol
    {
        public XirVariable XirVariable { private set; get; }

        public VariableSymbol(string name, XirVariable xirVariable) : base(name)
        {
            XirVariable = xirVariable;
        }
    }
}
