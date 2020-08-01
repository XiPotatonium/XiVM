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
                        VariableType returnType = Constructor.CurrentFunction.Type.ReturnType;
                        TryImplicitCast(returnType, actualReturnType);  // 可能需要隐式类型转换
                        switch (returnType.Tag)
                        {
                            case VariableTypeTag.BYTE:
                                Constructor.AddRetB();
                                break;
                            case VariableTypeTag.INT:
                                Constructor.AddRetI();
                                break;
                            case VariableTypeTag.DOUBLE:
                                Constructor.AddRetD();
                                break;
                            case VariableTypeTag.ADDRESS:
                                Constructor.AddRetA();
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        Constructor.AddRet();
                    }
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
