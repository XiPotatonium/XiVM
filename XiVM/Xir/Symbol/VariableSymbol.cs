namespace XiVM.Xir.Symbol
{
    public class VariableSymbol : Symbol
    {
        public Variable Variable { private set; get; }

        internal VariableSymbol(string name, Variable xirVariable) : base(name)
        {
            Variable = xirVariable;
        }
    }
}
