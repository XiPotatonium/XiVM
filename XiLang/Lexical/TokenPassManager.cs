using System;
using System.Collections.Generic;

namespace XiLang.Lexical
{
    /// <summary>
    /// 有AST之前的pass，处理Token流
    /// ClassPass
    /// ParsePass
    /// </summary>
    public interface ITokenPass
    {
        object Run(Func<Token> nextToken);
    }


    /// <summary>
    /// 基于Token的PassManager
    /// </summary>
    public class TokenPassManager
    {
        private Lexer Lexer { set; get; }
        private bool FirstPass { set; get; } = true;
        private List<Token> TokenBuf { get; } = new List<Token>();
        private int TokenBufIndex { set; get; } = 0;

        public TokenPassManager(string file)
        {
            Lexer = new Lexer(file);
        }

        public object Run(ITokenPass pass)
        {
            TokenBufIndex = 0;
            return pass.Run(NextToken);
        }

        private Token NextToken()
        {
            Token ret;
            if (FirstPass)
            {
                ret = Lexer.Next();
                TokenBuf.Add(ret);
                if (ret == Token.EOF)
                {
                    FirstPass = false;
                }
            }
            else
            {
                ret = TokenBuf[TokenBufIndex++];
            }
            return ret;
        }
    }
}
