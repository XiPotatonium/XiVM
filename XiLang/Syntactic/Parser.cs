using System;
using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;
using XiLang.Lexical;
using XiLang.PassMgr;

namespace XiLang.Syntactic
{
    /// <summary>
    /// 语法分析，第二个Pass
    /// </summary>
    public partial class Parser : AbstractParser, ITokenPass
    {
        public Parser(HashSet<string> classes)
        {
            Classes = classes;
        }

        public object Run(Func<Token> nextToken)
        {
            NextToken = nextToken;
            return Parse();
        }

        private void AppendASTLinkedList<T>(ref T root, ref T cur, T newv) where T : AST
        {
            if (cur == null)
            {
                root = cur = newv;
            }
            else
            {
                cur.SiblingAST = newv;
                cur = (T)cur.SiblingAST;
            }
        }

        /// <summary>
        /// Program
        ///     (GlobalStmt)*
        /// </summary>
        /// <param name="terminateTokenTypes"></param>
        /// <returns></returns>
        public AST Parse()
        {
            AST root = null;
            AST cur = null;
            while (!Check(TokenType.EOF))
            {
                AppendASTLinkedList(ref root, ref cur, ParseGlobalStmt());
            }
            return root;
        }

        /// <summary>
        /// GlobalStmt
        ///     ClassStmt | DeclarationStmt
        /// </summary>
        /// <returns></returns>
        private AST ParseGlobalStmt()
        {
            if (Check(TokenType.CLASS))
            {
                return ParseClassStmt();
            }
            else
            {
                return ParseDeclOrDefStmt();
            }
        }

        /// <summary>
        /// ClassStmt
        ///     CLASS ID LBRACES DeclarationStmt* RBRACES
        /// </summary>
        /// <returns></returns>
        private ClassStmt ParseClassStmt()
        {
            Consume(TokenType.CLASS);
            ClassStmt ret = new ClassStmt(Consume(TokenType.ID).Literal);
            FuncStmt fs = null;
            FuncStmt fcur = null;
            VarStmt vs = null;
            VarStmt vcur = null;
            Consume(TokenType.LBRACES);
            while (!Check(TokenType.RBRACES))
            {
                DeclarationStmt stmt = ParseDeclOrDefStmt();
                if (stmt is FuncStmt)
                {
                    AppendASTLinkedList(ref fs, ref fcur, (FuncStmt)stmt);
                }
                else
                {
                    AppendASTLinkedList(ref vs, ref vcur, (VarStmt)stmt);
                }
            }
            Consume(TokenType.RBRACES);
            ret.Functions = fs;
            ret.Variables = vs;
            return ret;
        }

        /// <summary>
        /// 这里用了LookAhead
        /// 此外不允许不带定义的声明
        /// DeclarationStmt
        ///     TypeExpr FuncDeclarator BlockStmt
        ///     TypeExpr VarDeclarator SEMICOLON
        /// </summary>
        /// <returns></returns>
        private DeclarationStmt ParseDeclOrDefStmt()
        {
            TypeExpr type = ParseTypeExpr();
            if (CheckAt(1, TokenType.LPAREN))
            {
                FuncStmt ret = ParseFuncDeclarator(type);
                ret.Body = ParseBlockStmt();
                return ret;
            }
            else
            {
                VarStmt ret = ParseVarDeclarator(type);
                Consume(TokenType.SEMICOLON);
                return ret;
            }
        }

        /// <summary>
        /// 参数中不能出现void
        /// FuncDeclarator
        ///     ID LPAREN ParamsAST? RPAREN
        /// </summary>
        /// <param name="retType"></param>
        /// <returns></returns>
        private FuncStmt ParseFuncDeclarator(TypeExpr retType)
        {
            string id = Consume(TokenType.ID).Literal;
            Consume(TokenType.LPAREN);
            ParamsAst ps = null;
            if (!Check(TokenType.RPAREN))
            {
                ps = ParseParamsAST();
            }

            Consume(TokenType.RPAREN);
            return new FuncStmt(retType, id, ps);
        }

        /// <summary>
        /// VarDeclarator
        ///     Id (ASSIGN Expr)? (COMMA VarDeclarator)?
        /// </summary>
        /// <returns></returns>
        private VarStmt ParseVarDeclarator(TypeExpr type)
        {
            Expr declarator = ParseExpr(true);
            VarStmt vars;
            if (declarator.ExprType == ExprType.ID)
            {
                vars = new VarStmt(type, declarator.Value.StringValue, null);
            }
            else
            {
                vars = new VarStmt(type, declarator.Expr1.Value.StringValue, declarator.Expr2);
            }
            if (Check(TokenType.COMMA))
            {
                Consume(TokenType.COMMA);
                vars.SiblingAST = ParseVarDeclarator(type);
            }
            return vars;
        }

        /// <summary>
        /// ParamsAST
        ///     Params
        /// </summary>
        /// <returns></returns>
        private ParamsAst ParseParamsAST()
        {
            return new ParamsAst(ParseParams());
        }

        /// <summary>
        /// Params
        ///     TypeExpr Id (ASSIGN Expr)? (COMMA Params)?
        /// </summary>
        /// <returns></returns>
        private VarStmt ParseParams()
        {
            TypeExpr type = ParseTypeExpr();
            Expr declarator = ParseExpr(true);
            VarStmt vars;
            if (declarator.ExprType == ExprType.ID)
            {
                vars = new VarStmt(type, declarator.Value.StringValue, null);
            }
            else
            {
                vars = new VarStmt(type, declarator.Expr1.Value.StringValue, declarator.Expr2);
            }
            if (Check(TokenType.COMMA))
            {
                Consume(TokenType.COMMA);
                vars.SiblingAST = ParseParams();
            }
            return vars;
        }

        /// <summary>
        /// Function和Class不属于普通Stmt
        /// Stmt
        ///     LoopStmt | IfStmt | JumpStmt | BlockStmt | VarOrExprStmt
        /// </summary>
        /// <returns></returns>
        private Stmt ParseStmt()
        {
            if (Check(TokenType.FOR, TokenType.WHILE))
            {
                return ParseLoopStmt();
            }
            else if (Check(TokenType.IF))
            {
                return ParseIfStmt();
            }
            else if (Check(TokenType.CONTINUE, TokenType.BREAK, TokenType.RETURN))
            {
                return ParseJumpStmt();
            }
            else if (Check(TokenType.LBRACES))
            {
                return ParseBlockStmt();
            }
            else
            {
                return ParseVarOrExprStmt();
            }
        }

        /// <summary>
        /// BlockStmt
        ///     LBRACES Stmt* RBRACES
        /// </summary>
        /// <returns></returns>
        private BlockStmt ParseBlockStmt()
        {
            Stmt child = null;
            Stmt cur = null;
            Consume(TokenType.LBRACES);
            while (!Check(TokenType.RBRACES))
            {
                AppendASTLinkedList(ref child, ref cur, ParseStmt());
            }
            Consume(TokenType.RBRACES);
            return new BlockStmt(child);
        }

        /// <summary>
        /// 如果VarDecl是class，第一个token是id，此时无法和Expr区分
        /// 要看第二个token是不是id，如果是id则为VarDecl
        /// VarOrExprStmt
        ///     VarStmt | ExprStmt
        /// </summary>
        /// <returns></returns>
        private Stmt ParseVarOrExprStmt()
        {
            Stmt ret;

            if (IsTypeExprPrefix())
            {
                ret = ParseVarDeclarator(ParseTypeExpr());
            }
            else
            {
                return ParseExprStmt();
            }
            Consume(TokenType.SEMICOLON);
            return ret;
        }

        /// <summary>
        /// LoopStmt
        ///     WHILE LPAREN Expr RPAREN (SELICOLON | Stmt)
        ///     FOR LPAREN (VarOrExprStmt | SEMICOLON) Expr? SEMICOLON ExprList? RPAREN (SELICOLON | Stmt)
        /// </summary>
        /// <returns></returns>
        private LoopStmt ParseLoopStmt()
        {
            Stmt body = null;
            Expr cond = null;
            if (Check(TokenType.WHILE))
            {
                Consume(TokenType.WHILE);
                Consume(TokenType.LPAREN);
                cond = ParseExpr();
                Consume(TokenType.RPAREN);
                if (Check(TokenType.SEMICOLON))
                {
                    Consume(TokenType.SEMICOLON);
                }
                else
                {
                    body = ParseStmt();
                }
                return LoopStmt.MakeWhile(cond, body);
            }
            else
            {
                Consume(TokenType.FOR);
                Consume(TokenType.LPAREN);
                Stmt init = null;
                Expr step = null;
                if (!Check(TokenType.SEMICOLON))
                {
                    init = ParseVarOrExprStmt();
                }
                else
                {
                    Consume(TokenType.SEMICOLON);
                }
                if (!Check(TokenType.SEMICOLON))
                {
                    cond = ParseExpr();
                }
                Consume(TokenType.SEMICOLON);
                if (!Check(TokenType.RPAREN))
                {
                    step = ParseExprList();
                }
                Consume(TokenType.RPAREN);
                if (Check(TokenType.SEMICOLON))
                {
                    Consume(TokenType.SEMICOLON);
                }
                else
                {
                    body = ParseStmt();
                }
                return LoopStmt.MakeFor(init, cond, step, body);
            }
        }

        /// <summary>
        /// IfStmt
        ///     IF LPAREN Expr RPAREN Stmt (ELSE Stmt)?
        /// </summary>
        /// <returns></returns>
        private IfStmt ParseIfStmt()
        {
            Consume(TokenType.IF);
            Consume(TokenType.LPAREN);
            Expr cond = ParseExpr();
            Consume(TokenType.RPAREN);
            Stmt then = ParseStmt();
            Stmt otherwise = null;
            if (Check(TokenType.ELSE))
            {
                Consume(TokenType.ELSE);
                otherwise = ParseStmt();
            }
            return IfStmt.MakeIf(cond, then, otherwise);
        }

        /// <summary>
        /// JumpStmt
        ///     (CONTINUE | BREAK | RETURN (ListExpr)?) SEMICOLON
        /// </summary>
        /// <returns></returns>
        private JumpStmt ParseJumpStmt()
        {
            JumpStmt ret = new JumpStmt();

            Token t = Consume(TokenType.CONTINUE, TokenType.BREAK, TokenType.RETURN);
            switch (t.Type)
            {
                case TokenType.CONTINUE:
                    ret.Type = JumpType.CONTINUE;
                    break;
                case TokenType.BREAK:
                    ret.Type = JumpType.BREAK;
                    break;
                case TokenType.RETURN:
                    ret.Type = JumpType.RETURN;
                    if (!Check(TokenType.SEMICOLON))
                    {
                        ret.ReturnVal = ParseExprList();
                    }
                    break;
                default:
                    break;  // 不会执行到这里
            }

            Consume(TokenType.SEMICOLON);
            return ret;
        }

        /// <summary>
        /// ExprStmt
        ///     ListExpr SEMICOLON
        /// </summary>
        /// <returns></returns>
        private ExprStmt ParseExprStmt()
        {
            ExprStmt ret = new ExprStmt(ParseExprList());
            Consume(TokenType.SEMICOLON);
            return ret;
        }
    }
}
