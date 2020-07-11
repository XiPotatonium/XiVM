using System;
using System.Text.RegularExpressions;
using XiLang.Exceptions;

namespace XiLang.Lexical
{
    public class Lexer
    {
        private string Text { set; get; }
        private int Index { set; get; } = 0;
        private int Line { set; get; } = 0;
        private int LineStartIndex { set; get; } = 0;
        private int Column { set; get; } = 0;

        public Lexer(string text)
        {
            Text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>null 表示EOF</returns>
        public Token Next()
        {
            Token ret = null;
            while (ret == null)
            {
                if (Index >= Text.Length)
                {
                    return Token.EOF;
                }
                if (Text[Index] == '\n')
                {   // 新的一行
                    ++Index;
                    LineStartIndex = Index;
                    ++Line;
                    Column = 0;
                }
                int maxLen = 0;
                Func<string, int, Token> func = null;
                foreach (System.Collections.Generic.KeyValuePair<Regex, Func<string, int, Token>> rule in LexicalRules.RegexRules)
                {
                    Match match = rule.Key.Match(Text, Index);
                    if (match.Success && match.Length > maxLen)
                    {   // 取最长的那个匹配
                        maxLen = match.Length;
                        func = rule.Value;
                    }
                }
                if (maxLen == 0)
                {   // 无法匹配的字符串
                    int nlIndex = Text.IndexOf('\n', Index);
                    nlIndex = nlIndex < 0 ? Text.Length : nlIndex;
                    throw new LexicalException(Line, Column, Text[LineStartIndex..nlIndex]);
                }
                else
                {
                    ret = func(Text.Substring(Index, maxLen), Line);
                    Index += maxLen;
                    Column += maxLen;
                }
            }
            return ret;
        }
    }
}
