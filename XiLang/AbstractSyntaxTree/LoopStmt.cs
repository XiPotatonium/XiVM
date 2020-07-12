namespace XiLang.AbstractSyntaxTree
{
    public enum LoopType
    {
        WHILE, FOR
    }

    public class LoopStmt : Stmt
    {
        public static LoopStmt MakeFor(Stmt init, Expr cond, Expr step, Stmt body)
        {
            return new LoopStmt()
            {
                Type = LoopType.WHILE,
                Init = init,
                Cond = cond,
                Step = step,
                Body = body
            };
        }

        public static LoopStmt MakeWhile(Expr cond, Stmt body)
        {
            return new LoopStmt()
            {
                Type = LoopType.WHILE,
                Cond = cond,
                Body = body
            };
        }

        public LoopType Type { private set; get; }
        /// <summary>
        /// 只能是VarStmt或者ExprStmt
        /// </summary>
        public Stmt Init { private set; get; }
        public Expr Cond { private set; get; }
        public Expr Step { private set; get; }
        public Stmt Body { private set; get; }


        public override string ASTLabel()
        {
            return Type switch
            {
                LoopType.WHILE => "while",
                LoopType.FOR => "for",
                _ => "UNK",
            };
        }

        public override AST[] Children()
        {
            return new AST[] { Init, Cond, Step, Body };
        }
    }
}
