using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    /// <summary>
    /// 带定义的声明也是声明
    /// </summary>
    public abstract class DeclarationStmt : Stmt
    {
        public TypeExpr Type { set; get; }
        public string Id { private set; get; }
        /// <summary>
        /// 如果是局部变量这种没有access flag的，填null
        /// </summary>
        public AccessFlag AccessFlag { private set; get; }

        public DeclarationStmt(AccessFlag flag, TypeExpr type, string id)
        {
            AccessFlag = flag;
            Type = type;
            Id = id;
            if (AccessFlag == null)
            {
                // 避免是null
                AccessFlag = AccessFlag.DefaultFlag;
            }
        }
    }
}
