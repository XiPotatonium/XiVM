using System;
using System.Collections.Generic;
using System.Text;
using XiLang.AbstractSyntaxTree;

namespace XiLang.PassMgr
{
    public interface IASTPass
    {
        object Run(AST root);
    }
}
