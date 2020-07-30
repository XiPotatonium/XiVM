namespace XiVM.Xir.Symbol
{
    public class FunctionSymbol : Symbol
    {
        public Function Function { private set; get; }
        internal FunctionSymbol(string name, Function function) : base(name)
        {
            Function = function;
        }
    }
}
