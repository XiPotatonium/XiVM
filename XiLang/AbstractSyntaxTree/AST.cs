using XiLang.Errors;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        /// <summary>
        /// 会同时CodeGen兄弟节点
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public static VariableType CodeGen(AST ast)
        {
            VariableType ret = null;
            while (ast != null)
            {
                ret = ast.CodeGen();
                if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch == true)
                {
                    break;
                }
                ast = ast.SiblingAST;
            }
            return ret;
        }

        public AST SiblingAST;

        public abstract string ASTLabel();
        public abstract AST[] Children();

        /// <summary>
        /// 不会CodeGen兄弟
        /// </summary>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public abstract VariableType CodeGen();

        /// <summary>
        /// 基本类型的隐式类型提升
        /// 这个是不是应该放到XiVM里面?
        /// </summary>
        public void TryImplicitCast(VariableType expectType, VariableType actualType)
        {
            if (expectType.Equivalent(actualType))
            {
                return;
            }
            if (expectType.Tag == VariableTypeTag.DOUBLE && actualType.Tag == VariableTypeTag.INT)
            {
                Constructor.AddI2D();
            }
            else if (expectType.Tag == VariableTypeTag.INT && actualType.Tag == VariableTypeTag.BYTE)
            {   // XiVM不需要任何操作
                return;
            }

            throw new XiLangError($"Need Explicit cast to cast from {actualType.Tag} to {expectType.Tag}");
        }
    }
}
