using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiLang.Errors;
using XiLang.Lexical;

namespace XiLang.Syntactic
{
    public abstract class AbstractParser
    {
        protected Queue<Token> TokenBuf { get; } = new Queue<Token>();
        protected Func<Token> NextToken { set; get; } = null;
        protected HashSet<string> Classes { set; get; }

        protected AbstractParser() { }

        /// <summary>
        /// 判断下一个token是不是一个TypeExpr的开头
        /// </summary>
        /// <returns>true表示接下来一定是一个TypeExpr</returns>
        protected bool IsTypeExprPrefix(int index = 0)
        {
            Token t = Peek(index);
            if (t.Type == TokenType.ID && Classes.Contains(t.Literal))
            {
                return true;
            }
            return LexicalRules.TypeTokens.Contains(t.Type);
        }

        /// <summary>
        /// Lookahead N
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Token Peek(int index = 0)
        {
            while (TokenBuf.Count <= index)
            {
                TokenBuf.Enqueue(NextToken());
            }
            Queue<Token>.Enumerator iter = TokenBuf.GetEnumerator();
            for (int i = 0; i <= index; ++i)
            {
                iter.MoveNext();
            }
            return iter.Current;
        }

        /// <summary>
        /// types为空表示接受任何Token
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        protected Token Consume(params TokenType[] types)
        {
            if (TokenBuf.Count == 0)
            {
                TokenBuf.Enqueue(NextToken());
            }
            if (types.Length != 0 && !Check(types))
            {
                Token t = TokenBuf.Dequeue();
                StringBuilder sb = new StringBuilder("Expect ").Append(types[0].ToString());
                for (int i = 1; i < types.Length; ++i)
                {
                    sb.Append("/").Append(types[i].ToString());
                }
                throw new SyntaxError(sb.ToString(), t);
            }
            return TokenBuf.Dequeue();
        }

        protected bool Check(params TokenType[] types)
        {
            return CheckAt(0, types);
        }

        protected bool CheckAt(int index, params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Peek(index).Type == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
