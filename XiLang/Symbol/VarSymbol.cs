using XiLang.AbstractSyntaxTree;

namespace XiLang.Symbol
{
    public class VarSymbol : Symbol
    {
        public static VarSymbol FromAST(VarStmt stmt)
        {
            return new VarSymbol(stmt.Id)
            {
                Type = stmt.Type,
                Value = stmt.Init?.Evaluate()
            };
        }

        public TypeExpr Type { set; get; }
        public XiLangValue Value { set; get; }

        public VarSymbol(string name) : base(name)
        {

        }
    }
}
