using System;
using System.Collections.Generic;
using XiLang.Errors;
using XiLang.Lexical;

namespace XiLang.Syntactic
{
    /// <summary>
    /// 负责提取文件中的所有class信息
    /// </summary>
    public class ClassPass : AbstractParser, ITokenPass
    {
        public object Run(Func<Token> nextToken)
        {
            NextToken = nextToken;
            Classes = new HashSet<string>();

            Token t = Consume();
            while (true)
            {
                while (!(t.Type == TokenType.CLASS || t.Type == TokenType.EOF))
                {
                    t = Consume();
                }
                if (t.Type == TokenType.EOF)
                {
                    break;
                }

                t = Consume(TokenType.ID);
                if (Classes.Contains(t.Literal))
                {   // class已存在
                    throw new ClassError($"Duplicate class definition {t.Literal}", t.Line);
                }
                Classes.Add(t.Literal);
                BalencedBraces();
            }

            return Classes;
        }

        private void BalencedBraces()
        {
            Consume(TokenType.LBRACES);
            int count = 1;
            while (count != 0)
            {
                Token t = Consume();
                if (t.Type == TokenType.RBRACES)
                {
                    --count;
                }
                else if (t.Type == TokenType.LBRACES)
                {
                    ++count;
                }
            }
        }
    }
}
