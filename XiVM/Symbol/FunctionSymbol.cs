using System.Collections.Generic;

namespace XiVM.Symbol
{
    internal class FunctionSymbol : Symbol
    {
        public List<VariableSymbol> Params { set; get; }

        public FunctionSymbol(string name) : base(name)
        {

        }
    }
}
