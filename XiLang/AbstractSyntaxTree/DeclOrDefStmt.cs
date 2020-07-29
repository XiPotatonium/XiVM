namespace XiLang.AbstractSyntaxTree
{
    /// <summary>
    /// 带定义的声明也是声明
    /// </summary>
    public abstract class DeclarationStmt : Stmt
    {
        public TypeExpr Type { set; get; }
        public string Id { private set; get; }

        public DeclarationStmt(TypeExpr type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}
