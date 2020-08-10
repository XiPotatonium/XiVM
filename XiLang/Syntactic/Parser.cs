using System;
using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Lexical;
using XiVM;

namespace XiLang.Syntactic
{
    /// <summary>
    /// 语法分析，第二个Pass
    /// </summary>
    internal partial class Parser : AbstractParser, ITokenPass
    {
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
        ///     (ImportStmt)* (ClassStmt)*
        /// </summary>
        /// <param name="terminateTokenTypes"></param>
        /// <returns></returns>
        public AST Parse()
        {
            AST root = null;
            AST cur = null;
            while (!Check(TokenType.EOF))
            {
                while (Check(TokenType.IMPORT))
                {
                    AppendASTLinkedList(ref root, ref cur, ParseImport());
                }

                if (Check(TokenType.CLASS))
                {
                    AppendASTLinkedList(ref root, ref cur, ParseClassStmt());
                }
                else
                {
                    throw new XiLangError("Module can only directly contain class");
                }
            }
            return root;
        }

        /// <summary>
        /// ImportStmt
        ///     IMPORT ID (DOT ID)* SEMICOLON
        /// </summary>
        /// <returns></returns>
        private ImportStmt ParseImport()
        {
            Consume(TokenType.IMPORT);
            Token t = Consume(TokenType.ID);
            IdExpr root = IdExpr.MakeId(t.Literal, t.Line);
            IdExpr cur = root;

            while (Check(TokenType.DOT))
            {
                Consume(TokenType.DOT);
                t = Consume(TokenType.ID);
                AppendASTLinkedList(ref root, ref cur, IdExpr.MakeId(t.Literal, t.Line));
            }

            Consume(TokenType.SEMICOLON);
            return new ImportStmt(root);
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
            ret.Methods = fs;
            ret.Fields = vs;
            return ret;
        }

        /// <summary>
        /// 这里用了LookAhead
        /// 此外不允许不带定义的声明
        /// DeclarationStmt
        ///     ACCESS_FLAG* TypeExpr FuncDeclarator BlockStmt
        ///     ACCESS_FLAG* TypeExpr VarDeclarator SEMICOLON
        /// </summary>
        /// <returns></returns>
        private DeclarationStmt ParseDeclOrDefStmt()
        {
            AccessFlag flag = ParserAccessFlag();
            TypeExpr type = ParseTypeExpr();
            if (CheckAt(1, TokenType.LPAREN))
            {
                FuncStmt ret = ParseFuncDeclarator(type, flag);
                ret.Body = ParseBlockStmt();
                return ret;
            }
            else
            {
                VarStmt ret = ParseVarDeclarator(type, flag);
                Consume(TokenType.SEMICOLON);
                return ret;
            }
        }

        /// <summary>
        /// ACCESS_FLAG*
        /// 目前仅支持static，只允许出现一次
        /// </summary>
        /// <returns></returns>
        private AccessFlag ParserAccessFlag()
        {
            AccessFlag accessFlag = new AccessFlag();
            while (Check(LexicalRules.AccessFlagTokens))
            {
                Token t = Consume(LexicalRules.AccessFlagTokens);
                switch (t.Type)
                {
                    case TokenType.STATIC:
                        if (accessFlag.IsStatic)
                        {
                            throw new SyntaxError("Duplicated static modifier", t);
                        }
                        accessFlag.IsStatic = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return accessFlag;
        }

        /// <summary>
        /// 参数中不能出现void
        /// FuncDeclarator
        ///     ID LPAREN ParamsAST RPAREN
        /// </summary>
        /// <param name="retType"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private FuncStmt ParseFuncDeclarator(TypeExpr retType, AccessFlag flag)
        {
            string id = Consume(TokenType.ID).Literal;
            Consume(TokenType.LPAREN);
            ParamsAst ps = null;
            ps = ParseParamsAST();

            Consume(TokenType.RPAREN);
            return new FuncStmt(flag, retType, id, ps);
        }

        /// <summary>
        /// VarDeclarator
        ///     Id (ASSIGN Expr)? (COMMA VarDeclarator)?
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private VarStmt ParseVarDeclarator(TypeExpr type, AccessFlag flag = null)
        {
            Expr declarator = ParseExpr(true);
            VarStmt vars;
            if (declarator is IdExpr idExpr)
            {
                vars = new VarStmt(flag, type, idExpr.Id, null);
            }
            else
            {
                vars = new VarStmt(flag, type, ((IdExpr)declarator.Expr1).Id, declarator.Expr2);
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
        ///     Params*
        /// </summary>
        /// <returns></returns>
        private ParamsAst ParseParamsAST()
        {
            if (!Check(TokenType.RPAREN))
            {
                return new ParamsAst(ParseParams());
            }
            else
            {
                return new ParamsAst(null);
            }
        }

        /// <summary>
        /// Params
        ///     TypeExpr Id (COMMA Params)?
        /// </summary>
        /// <returns></returns>
        private VarStmt ParseParams()
        {
            TypeExpr type = ParseTypeExpr();
            IdExpr id = ParseId();
            VarStmt vars = new VarStmt(null, type, id.Id, null);

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
        /// VarOrExprStmt
        ///     VarStmt | ExprStmt
        /// </summary>
        /// <returns></returns>
        private Stmt ParseVarOrExprStmt()
        {
            Stmt ret;

            if (Check(LexicalRules.TypeTokens) || CheckAfterCompoundId(0, TokenType.ID))
            {
                // System.String str，表达式中不会出现这种情况
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
        ///     WHILE LPAREN Expr RPAREN (SELICOLON | BlockStmt)
        ///     FOR LPAREN (VarOrExprStmt | SEMICOLON) Expr? SEMICOLON ExprList? RPAREN (SELICOLON | BlockStmt)
        /// </summary>
        /// <returns></returns>
        private LoopStmt ParseLoopStmt()
        {
            BlockStmt body = null;
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
                    body = ParseBlockStmt();
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
                    body = ParseBlockStmt();
                }
                return LoopStmt.MakeFor(init, cond, step, body);
            }
        }

        /// <summary>
        /// IfStmt
        ///     IF LPAREN Expr RPAREN BlockStmt (ELSE BlockStmt | IfStmt)?
        /// </summary>
        /// <returns></returns>
        private IfStmt ParseIfStmt()
        {
            Consume(TokenType.IF);
            Consume(TokenType.LPAREN);
            Expr cond = ParseExpr();
            Consume(TokenType.RPAREN);
            BlockStmt then = ParseBlockStmt();
            Stmt otherwise = null;
            if (Check(TokenType.ELSE))
            {
                Consume(TokenType.ELSE);
                if (Check(TokenType.LBRACES))
                {
                    otherwise = ParseBlockStmt();
                }
                else
                {
                    otherwise = ParseIfStmt();
                }
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
