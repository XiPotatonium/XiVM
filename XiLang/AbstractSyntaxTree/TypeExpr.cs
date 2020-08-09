using System;
using System.Text;
using XiLang.Errors;
using XiLang.Lexical;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class TypeExpr : Expr
    {
        public SyntacticValueType Type { set; get; }
        public bool IsArray { set; get; }
        public string ClassName { set; get; }

        public override string ASTLabel()
        {
            StringBuilder stringBuilder = new StringBuilder("<");

            stringBuilder.Append(Type switch
            {
                SyntacticValueType.BOOL => "bool",
                SyntacticValueType.INT => "int",
                SyntacticValueType.DOUBLE => "float",
                SyntacticValueType.STRING => "string",
                SyntacticValueType.CLASS => ClassName,
                SyntacticValueType.VOID => "void",
                _ => throw new NotImplementedException(),
            });

            stringBuilder.Append(IsArray ? "[]>" : ">");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Type表达式是一个常量，在设计中不允许有child
        /// </summary>
        /// <returns></returns>
        public override AST[] Children()
        {
            return new AST[0];
        }

        public VariableType ToXirType()
        {
            if (IsArray)
            {
                return Type switch
                {
                    SyntacticValueType.BOOL => ArrayType.ByteArrayType,
                    SyntacticValueType.INT => throw new NotImplementedException(),
                    SyntacticValueType.DOUBLE => throw new NotImplementedException(),
                    SyntacticValueType.STRING => throw new NotImplementedException(),
                    SyntacticValueType.CLASS => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                return Type switch
                {
                    SyntacticValueType.BOOL => VariableType.ByteType,
                    SyntacticValueType.INT => VariableType.IntType,
                    SyntacticValueType.DOUBLE => VariableType.DoubleType,
                    SyntacticValueType.STRING => throw new NotImplementedException(),
                    SyntacticValueType.CLASS => throw new NotImplementedException(),
                    SyntacticValueType.VOID => null,
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}
