using System.Collections.Generic;
using XiLang.Lexical;

namespace XiLang.PassMgr
{
    /// <summary>
    /// 基于Token的PassManager
    /// </summary>
    public class PassManager
    {
        private Lexer Lexer { set; get; }
        private bool FirstPass { set; get; } = true;
        private List<Token> TokenBuf { get; } = new List<Token>();
        private int TokenBufIndex { set; get; } = 0;

        public PassManager(string file)
        {
            Lexer = new Lexer(file);
        }

        public object Run(IPass pass)
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
