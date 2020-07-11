using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XiLang.Lexical
{
    public enum TokenType
    {
        // 特殊符号
        EOF,
        // 类型
        BOOL, INT, FLOAT, VOID, STRING,
        // 其他关键词
        CLASS, BASE, FOR, WHILE, IF, ELSE, CONTINUE, BREAK, RETURN,
        // 常量
        TRUE, FALSE, NULL,
        // 运算子
        ID, DEC_LITERAL, HEX_LITERAL, FLOAT_LITERAL, STR_LITERAL,
        // 算数运算符
        ADD, SUB, MUL, DIV, MOD, INC, DEC,
        //关系运算符
        EQ, NE, GT, GE, LT, LE,
        // 逻辑运算符
        LOG_AND, LOG_OR, LOG_NOT,
        // 位运算符
        BIT_AND, BIT_OR, BIT_XOR, BIT_NOT, BIT_SL, BIT_SR,
        // 赋值运算符
        ASSIGN, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, SL_ASSIGN, SR_ASSIGN,
        // 特殊运算符
        QUESTION, COLON, DOT,
        // 标点
        LBRACES, RBRACES, LPAREN, RPAREN, SEMICOLON, COMMA, LBRACKET, RBRACKET,
    }

    public static class LexicalRules
    {
        /// <summary>
        /// Keyword后面只能接空白符
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
        {
            { "bool", TokenType.BOOL },
            { "int", TokenType.INT },
            { "float", TokenType.FLOAT },
            { "void", TokenType.VOID },
            { "string", TokenType.STRING },

            { "class", TokenType.CLASS },
            { "base", TokenType.BASE },
            { "for", TokenType.FOR },
            { "while", TokenType.WHILE },
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "continue", TokenType.CONTINUE },
            { "break", TokenType.BREAK },
            { "return", TokenType.RETURN },

            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "null", TokenType.NULL },
        };

        /// <summary>
        /// 所有可能出现在Type中的Token
        /// 未来会包括更多的基础类型以及const之类的修饰符
        /// 这个东西会在Parser中用于Check与预判
        /// </summary>
        public static readonly TokenType[] TypeTokens =
        {
            TokenType.BOOL, TokenType.INT, TokenType.FLOAT, TokenType.VOID, TokenType.STRING
        };

        /// <summary>
        /// Operator后面可以接任意字符
        /// </summary>
        private static readonly Dictionary<string, TokenType> Operators = new Dictionary<string, TokenType>()
        {
            { @"\+", TokenType.ADD },
            { @"-", TokenType.SUB },
            { @"\*", TokenType.MUL },           // *在RE中有特殊含义
            { @"/", TokenType.DIV },
            { @"%", TokenType.MOD },
            { @"\+\+", TokenType.INC },
            { @"--", TokenType.DEC },

            { @"==", TokenType.EQ },
            { @">", TokenType.GT },
            { @">=", TokenType.GE },
            { @"<", TokenType.LT },
            { @"<=", TokenType.LE },

            { @"\&\&", TokenType.LOG_AND },
            { @"\|\|", TokenType.LOG_OR },
            { @"!", TokenType.LOG_NOT },

            { @"\&", TokenType.BIT_AND },
            { @"\|", TokenType.BIT_OR },
            { @"\^", TokenType.BIT_XOR },
            { @"~", TokenType.BIT_NOT },
            { @"<<", TokenType.BIT_SL },
            { @">>", TokenType.BIT_SR },

            { @"=", TokenType.ASSIGN },
            { @"\+=", TokenType.ADD_ASSIGN },
            { @"-=", TokenType.SUB_ASSIGN },
            { @"\*=", TokenType.MUL_ASSIGN },
            { @"/=", TokenType.DIV_ASSIGN },
            { @"%=", TokenType.MOD_ASSIGN },
            { @"\&=", TokenType.AND_ASSIGN },
            { @"\|=", TokenType.OR_ASSIGN },
            { @"\^=", TokenType.XOR_ASSIGN },
            { @"<<=", TokenType.SL_ASSIGN },
            { @">>=", TokenType.SR_ASSIGN },

            { @"\?", TokenType.QUESTION },
            { @":", TokenType.COLON },
            { @"\.", TokenType.DOT },

            { @"{", TokenType.LBRACES },
            { @"}", TokenType.RBRACES },
            { @"\(", TokenType.LPAREN },      // 括号在re里有特殊含义
            { @"\)", TokenType.RPAREN },
            { @"\[", TokenType.LBRACKET },
            { @"\]", TokenType.RBRACKET },
            { @";", TokenType.SEMICOLON },
            { @",", TokenType.COMMA },
        };

        /// <summary>
        /// 其他规则
        /// </summary>
        private static readonly Dictionary<string, Func<string, int, Token>> Rules = new Dictionary<string, Func<string, int, Token>>()
        {
            { @"[_a-zA-Z][_a-zA-Z0-9]*", (s, l) => { return new Token(TokenType.ID, s, l); } }, // identifier
            { @"//.*", (s, l) => { return null; } },                                            // 注释
            { @"\s", (s, l) => { return null; } },      // 空白，最好不要用+，因为词法分析需要换行来记录行信息，把换行弄掉了可能会出问题
            { @"\d+\.\d+", (s, l) => { return new Token(TokenType.FLOAT_LITERAL, s, l); } },
            { @"\d+", (s, l) => { return new Token(TokenType.DEC_LITERAL, s, l); } },
            { @"0[xX][a-fA-F\d]+", (s, l) => { return new Token(TokenType.HEX_LITERAL, s, l); } },
            { "\"(\\.|[^\\\"\\n])*\"", (s, l) => { return new Token(TokenType.STR_LITERAL, s, l); } }
        };

        public static readonly Dictionary<Regex, Func<string, int, Token>> RegexRules = new Dictionary<Regex, Func<string, int, Token>>();

        /// <summary>
        /// 自动构建Keywords，Operators和Rules的规则
        /// </summary>
        static LexicalRules()
        {
            foreach (KeyValuePair<string, TokenType> keyword in Keywords)
            {
                RegexRules.Add(new Regex(@"\G\b" + keyword.Key, RegexOptions.Compiled),
                    (s, l) => { return new Token(keyword.Value, s, l); });
            }
            foreach (KeyValuePair<string, TokenType> op in Operators)
            {
                RegexRules.Add(new Regex(@"\G" + op.Key, RegexOptions.Compiled),
                    (s, l) => { return new Token(op.Value, s, l); });
            }
            foreach (KeyValuePair<string, Func<string, int, Token>> rule in Rules)
            {
                RegexRules.Add(new Regex(@"\G" + rule.Key), rule.Value);
            }
        }
    }
}
