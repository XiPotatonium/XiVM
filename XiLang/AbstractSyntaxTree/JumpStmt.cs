namespace XiLang.AbstractSyntaxTree
{
    public enum JumpType
    {
        CONTINUE, BREAK, RETURN
    }

    public class JumpStmt : Stmt
    {
        public JumpType Type { set; get; }
        public Expr ReturnVal { set; get; }

        protected override AST[] JsonChildren()
        {
            return new AST[] { ReturnVal };
        }

        protected override string JsonName()
        {
            return Type switch
            {
                JumpType.CONTINUE => "continue",
                JumpType.BREAK => "break",
                JumpType.RETURN => "return",
                _ => "UNK",
            };
        }
    }
}
