using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;

namespace XiLang.Symbol
{
    public class FuncSymbol : Symbol
    {
        public TypeExpr RetType { set; get; }
        public List<VarSymbol> Params { set; get; }
        public BlockStmt Body { set; get; }

        public FuncSymbol(string name) : base(name)
        {

        }
    }
}
