namespace XiVM.Xir.Symbol
{
    public class VariableSymbol : Symbol
    {
        public Variable XirVariable { private set; get; }

        internal VariableSymbol(string name, Variable xirVariable) : base(name)
        {
            XirVariable = xirVariable;
        }
    }
}
