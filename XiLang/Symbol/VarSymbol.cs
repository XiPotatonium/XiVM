using XiLang.AbstractSyntaxTree;

namespace XiLang.Symbol
{
    public class VarSymbol : Symbol
    {
        public TypeExpr Type { set; get; }
        public XiLangValue Value { set; get; }

        public VarSymbol(string name) : base(name)
        {

        }
    }
}
