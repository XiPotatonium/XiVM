using System;
using XiVM;

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

        public override AST[] Children()
        {
            return new AST[] { ReturnVal };
        }

        public override string ASTLabel()
        {
            return Type switch
            {
                JumpType.CONTINUE => "continue",
                JumpType.BREAK => "break",
                JumpType.RETURN => "return",
                _ => "UNK",
            };
        }

        public override VariableType CodeGen()
        {
            switch (Type)
            {
                case JumpType.CONTINUE:
                    throw new NotImplementedException();
                case JumpType.BREAK:
                    throw new NotImplementedException();
                case JumpType.RETURN:
                    if (ReturnVal != null)
                    {
                        VariableType actualReturnType = ReturnVal.CodeGen();
                        VariableType returnType = CodeGenPass.Constructor.CurrentBasicBlock.Function.Type.ReturnType;
                        if (!returnType.Equivalent(actualReturnType))
                        {
                            // TODO 返回值需要类型转换
                            throw new NotImplementedException();
                        }
                        CodeGenPass.Constructor.AddRet();
                    }
                    else
                    {
                        CodeGenPass.Constructor.AddRet();
                    }
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
