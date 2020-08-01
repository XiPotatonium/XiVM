using System;
using XiLang.Lexical;

namespace XiLang.Pass
{
    /// <summary>
    /// 有AST之前的pass，处理Token流
    /// 未来可以加入的预编译（解析import之类的）
    /// ClassPass
    /// ParsePass
    /// </summary>
    public interface ITokenPass
    {
        object Run(Func<Token> nextToken);
    }
}
