using System;
using XiLang.Lexical;

namespace XiLang.PassMgr
{
    public interface IPass
    {
        object Run(Func<Token> nextToken);
    }
}
