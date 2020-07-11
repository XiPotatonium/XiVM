namespace XiLang.Lexical
{
    public class Token
    {
        public static readonly Token EOF = new Token(TokenType.EOF, null, -1);

        public TokenType Type { set; get; }
        public int Line { set; get; }
        public string Literal { set; get; }

        public Token(TokenType type, string literal, int line)
        {
            Type = type;
            Literal = literal;
            Line = line;
        }
    }
}
