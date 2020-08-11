using System;
using System.Collections.Generic;
using System.Text;
using XiLang.Errors;
using XiVM;
using XiVM.ConstantTable;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    internal partial class Expr : AST
    {
        #region Static Make

        public static Expr MakeOp(OpType type, Expr lhs, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = lhs
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs, Expr rhs, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = lhs,
                Expr2 = rhs
            };
        }

        public static Expr MakeOp(OpType type, Expr expr1, Expr expr2, Expr expr3, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = expr1,
                Expr2 = expr2,
                Expr3 = expr3
            };
        }

        #endregion

        private static VariableType ExpectLocalOrField(ModuleConstructor constructor, VariableType raw)
        {
            if (raw is MemberType memberType)
            {
                if (memberType.IsField)
                {
                    FieldConstantInfo fieldConstantInfo = constructor.FieldPool.Get(memberType.FieldPoolIndex);
                    constructor.AddGetStaticFieldAddress(memberType.FieldPoolIndex);
                    VariableType ret = VariableType.GetType(
                        constructor.StringPool.Get(fieldConstantInfo.Type));
                    constructor.AddALoadT(ret);
                    return ret;
                }
                else
                {
                    throw new XiLangError($"{memberType} is not a variable");
                }
            }
            return raw;
        }

        public int Line { private set; get; }
        public OpType OpType { set; get; }
        public Expr Expr1 { set; get; }
        public Expr Expr2 { set; get; }
        public Expr Expr3 { set; get; }

        protected Expr() { }
        protected Expr(int line)
        {
            Line = line;
        }

        public override string ASTLabel()
        {
            return OpType switch
            {
                OpType.NEG => "-(neg)",
                OpType.INC => "++",
                OpType.DEC => "++",
                OpType.ADD => "+",
                OpType.SUB => "-",
                OpType.MUL => "*",
                OpType.DIV => "/",
                OpType.MOD => "%",
                OpType.LOG_NOT => "!",
                OpType.LOG_AND => "&&",
                OpType.LOG_OR => "||",
                OpType.BIT_NOT => "~",
                OpType.BIT_AND => "&",
                OpType.BIT_OR => "|",
                OpType.BIT_SL => "<<",
                OpType.BIT_SR => ">>",
                OpType.BIT_XOR => "^",
                OpType.EQ => "==",
                OpType.NE => "!=",
                OpType.GE => ">=",
                OpType.GT => ">",
                OpType.LE => "<=",
                OpType.LT => "<",
                OpType.ASSIGN => "=",
                OpType.ADD_ASSIGN => "+=",
                OpType.SUB_ASSIGN => "-=",
                OpType.MUL_ASSIGN => "*=",
                OpType.DIV_ASSIGN => "/=",
                OpType.MOD_ASSIGN => "%=",
                OpType.AND_ASSIGN => "&",
                OpType.OR_ASSIGN => "|",
                OpType.XOR_ASSIGN => "^",
                OpType.SL_ASSIGN => "<<",
                OpType.SR_ASSIGN => ">>",
                OpType.CONDITIONAL => "?:",
                OpType.CALL => "(Call)",
                OpType.CAST => "(CAST)",
                OpType.CLASS_ACCESS => ".",
                OpType.ARRAY_ACCESS => "[]",
                OpType.NEW => "NEW",
                _ => throw new NotImplementedException(),
            };
        }

        public override AST[] Children()
        {
            return new AST[] { Expr1, Expr2, Expr3 };
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            VariableType expr1Type, expr2Type;

            if (IsConst())
            {
                // 常量表达式，伪造一个常量子节点
                Expr1 = new ConstExpr(Line)
                {
                    Value = EvaluateConstExpr()
                };
                return Expr1.CodeGen(pass);
            }

            switch (OpType)
            {
                case OpType.NEG:
                    expr1Type = ExpectLocalOrField(pass.Constructor, Expr1.CodeGen(pass));
                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return pass.Constructor.AddNegI();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case OpType.INC:
                    throw new NotImplementedException();
                case OpType.DEC:
                    throw new NotImplementedException();
                case OpType.ADD:
                case OpType.SUB:
                case OpType.MUL:
                case OpType.DIV:
                    expr1Type = ExpectLocalOrField(pass.Constructor, Expr1.CodeGen(pass));
                    expr2Type = ExpectLocalOrField(pass.Constructor, Expr2.CodeGen(pass));
                    if (!expr1Type.Equivalent(expr2Type))
                    {
                        throw new TypeError($"{OpType}: {expr1Type} and {expr2Type} is not equivalent", Line);
                    }

                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return OpType switch
                        {
                            OpType.ADD => pass.Constructor.AddAddI(),
                            OpType.SUB => pass.Constructor.AddSubI(),
                            OpType.MUL => pass.Constructor.AddMulI(),
                            OpType.DIV => pass.Constructor.AddDivI(),
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case OpType.MOD:
                    expr1Type = ExpectLocalOrField(pass.Constructor, Expr1.CodeGen(pass));
                    if (!expr1Type.Equivalent(VariableType.IntType))
                    {
                        throw new TypeError($"{OpType} expect int type", Line);
                    }
                    expr2Type = ExpectLocalOrField(pass.Constructor, Expr2.CodeGen(pass));
                    if (!expr2Type.Equivalent(VariableType.IntType))
                    {
                        throw new TypeError($"{OpType} expect int type", Line);
                    }
                    return pass.Constructor.AddMod();
                case OpType.LOG_NOT:
                    throw new NotImplementedException();
                case OpType.LOG_AND:
                    throw new NotImplementedException();
                case OpType.LOG_OR:
                    throw new NotImplementedException();
                case OpType.BIT_NOT:
                case OpType.BIT_AND:
                case OpType.BIT_XOR:
                case OpType.BIT_OR:
                case OpType.BIT_SL:
                case OpType.BIT_SR:
                    // 位操作不在计划之中
                    throw new NotImplementedException();
                case OpType.EQ:
                case OpType.NE:
                case OpType.GE:
                case OpType.GT:
                case OpType.LE:
                case OpType.LT:
                    expr1Type = ExpectLocalOrField(pass.Constructor, Expr1.CodeGen(pass));
                    expr2Type = ExpectLocalOrField(pass.Constructor, Expr2.CodeGen(pass));
                    if (!expr1Type.Equivalent(expr2Type))
                    {
                        throw new TypeError($"{OpType}: {expr1Type} and {expr2Type} is not equivalent", Line);
                    }

                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return OpType switch
                        {
                            OpType.EQ => pass.Constructor.AddSetEqI(),
                            OpType.NE => pass.Constructor.AddSetNeI(),
                            OpType.GE => pass.Constructor.AddSetGeI(),
                            OpType.GT => pass.Constructor.AddSetGtI(),
                            OpType.LE => pass.Constructor.AddSetLeI(),
                            OpType.LT => pass.Constructor.AddSetLtI(),
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    throw new NotImplementedException();
                case OpType.ASSIGN:
                    expr2Type = ExpectLocalOrField(pass.Constructor, Expr2.CodeGen(pass));
                    // TODO 赋值检查
                    expr1Type = Expr1.LeftValueCodeGen(pass);
                    pass.Constructor.AddStoreT(expr2Type);
                    return expr2Type;
                case OpType.ADD_ASSIGN:
                case OpType.SUB_ASSIGN:
                case OpType.MUL_ASSIGN:
                case OpType.DIV_ASSIGN:
                case OpType.MOD_ASSIGN:
                    // 特殊assign优先级低
                    throw new NotImplementedException();
                case OpType.AND_ASSIGN:
                case OpType.OR_ASSIGN:
                case OpType.XOR_ASSIGN:
                case OpType.SL_ASSIGN:
                case OpType.SR_ASSIGN:
                    // 位操作不在计划之中
                    throw new NotImplementedException();
                case OpType.CONDITIONAL:
                    throw new NotImplementedException();
                case OpType.CAST:
                    //expr1Type = ((TypeExpr)Expr1).ToXirType();
                    //expr2Type = Expr2.CodeGen();
                    throw new NotImplementedException();
                case OpType.CALL:
                    // 查找潜在函数集
                    MemberType methodType = (MemberType)Expr1.CodeGen(pass);
                    List<(string descriptor, uint flag)> candidateMethods = Program.GetMethod(methodType);

                    List<VariableType> pTypes = new List<VariableType>();   // 正序
                    Expr3 = Expr2;  // 借用Expr3作临时变量
                    while (Expr3 != null)
                    {
                        pTypes.Add(ExpectLocalOrField(pass.Constructor, Expr3.CodeGen(pass)));
                        Expr3 = (Expr)Expr3.SiblingAST;
                    }
                    Expr3 = null;

                    // 确定对应函数
                    string methodDescriptor = null;
                    uint methodFlag = 0;
                    string actualParamsDescriptor = MethodDeclarationInfo.GetParamsDescriptor(pTypes);
                    foreach ((string candidate, uint candidateFlag) in candidateMethods)
                    {
                        if (MethodDeclarationInfo.CallMatch(candidate, actualParamsDescriptor))
                        {
                            // 目前要求完全匹配，但是不区分地址类型
                            methodDescriptor = candidate;
                            methodFlag = candidateFlag;
                        }
                    }

                    if (methodDescriptor == null)
                    {
                        throw new XiLangError($"No matched method");
                    }

                    pass.Constructor.AddCall(pass.Constructor.AddMethodPoolInfo(
                        methodType.ClassType.ClassPoolIndex,
                        methodType.Name, methodDescriptor, methodFlag));
                    return MethodDeclarationInfo.GetReturnType(methodDescriptor);
                case OpType.CLASS_ACCESS:
                    expr1Type = Expr1.CodeGen(pass);
                    if (expr1Type is ModuleType moduleType)
                    {
                        // 必然是Module.Class
                        Program.AssertClassExistence(moduleType.ModuleName, ((IdExpr)Expr2).Id);
                        return new ClassType(pass.Constructor.AddClassPoolInfo(moduleType.ModuleName, ((IdExpr)Expr2).Id))
                        {
                            ModuleName = moduleType.ModuleName,
                            ClassName = ((IdExpr)Expr2).Id
                        };
                    }
                    else if (expr1Type is ClassType classType)
                    {
                        // Class.Field或者Class.Method

                        IdExpr name = (IdExpr)Expr2;

                        MemberType ret = new MemberType(classType)
                        {
                            Name = name.Id,
                            IsField = false
                        };

                        if (Program.CheckFieldExistence(pass.Constructor, classType, name.Id,
                            out int fieldPoolIndex))
                        {
                            ret.IsField = true;
                            ret.FieldPoolIndex = fieldPoolIndex;
                        }

                        // 不检查是不是method，留到call的时候检查

                        return ret;
                    }
                    throw new NotImplementedException();
                case OpType.ARRAY_ACCESS:
                    throw new NotImplementedException();
                case OpType.NEW:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 期望获得一个左值
        /// 将表达式的结果的地址入栈
        /// </summary>
        protected virtual VariableType LeftValueCodeGen(CodeGenPass pass)
        {
            switch (OpType)
            {
                case OpType.CLASS_ACCESS:
                    throw new NotImplementedException();
                case OpType.ARRAY_ACCESS:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    internal class IdExpr : Expr
    {
        public static IdExpr MakeId(string id, int line)
        {
            return new IdExpr(id, line);
        }

        public string Id { private set; get; }

        public IdExpr(string id, int line)
            : base(line)
        {
            Id = id;
        }

        public override bool IsConst()
        {
            return false;
        }

        public override string ASTLabel()
        {
            return $"(Id){Id}";
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            if (pass.LocalSymbolTable.TryGetSymbol(Id, out Variable variable))
            {
                // 是一个局部变量
                pass.Constructor.AddLocal(variable.Offset);
                pass.Constructor.AddLoadT(variable.Type);
                return variable.Type;
            }


            ClassType classType = new ClassType(pass.Constructor.CurrentClass.ConstantPoolIndex)
            {
                ModuleName = pass.Constructor.Module.Name,
                ClassName = pass.Constructor.CurrentClass.Name
            };
            MemberType member = new MemberType(classType)
            {
                Name = Id,
                IsField = false
            };


            if (pass.Constructor.CurrentClass.Fields.TryGetValue(Id, out ClassField classField))
            {
                //// static或者非static field
                //if (field.AccessFlag.IsStatic)
                //{
                //    pass.Constructor.AddGetStaticFieldAddress(field);
                //}
                //else
                //{
                //    pass.Constructor.AddGetFieldAddress(field);
                //}
                //pass.Constructor.AddALoadT(field.Type);
                // return field.Type;
                member.IsField = true;
                member.FieldPoolIndex = classField.ConstantPoolIndex;
            }
            else if (!pass.Constructor.CurrentClass.Methods.ContainsKey(Id))
            {
                // 不是域和方法，有可能是类或模块
                Class classInfo = pass.Constructor.Classes.Find(c => c.Name == Id);
                if (classInfo != null)
                {
                    return new ClassType(classInfo.ConstantPoolIndex)
                    {
                        ModuleName = pass.Constructor.Module.Name,
                        ClassName = Id
                    };
                }
                else if (Program.ModuleHeaders.ContainsKey(Id))
                {
                    // TODO暂未实现复合名称的模块
                    pass.Constructor.StringPool.TryAdd(Id);
                    return new ModuleType()
                    {
                        ModuleName = Id
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return member;
        }

        protected override VariableType LeftValueCodeGen(CodeGenPass pass)
        {
            if (pass.LocalSymbolTable.TryGetSymbol(Id, out Variable variable))
            {
                // 是一个局部变量
                pass.Constructor.AddLocal(variable.Offset);
                return variable.Type;
            }
            else if (pass.Constructor.CurrentClass.Fields.TryGetValue(Id, out ClassField field))
            {
                // static或者非static field
                if (field.AccessFlag.IsStatic)
                {
                    pass.Constructor.AddGetStaticFieldAddress(field);
                }
                else
                {
                    pass.Constructor.AddGetFieldAddress(field);
                }
                return field.Type;
            }
            else
            {
                // 暂时不支持函数赋值
                throw new NotImplementedException();
            }
        }
    }

    internal class TypeExpr : Expr
    {
        public SyntacticValueType Type { set; get; }
        public bool IsArray { set; get; }
        public List<string> ClassName { set; get; }

        public override string ASTLabel()
        {
            StringBuilder sb = new StringBuilder("<");

            if (Type == SyntacticValueType.CLASS)
            {
                sb.Append(ClassName[0]);
                for (int i = 1; i < ClassName.Count; ++i)
                {
                    sb.Append('.').Append(ClassName[i]);
                }
            }
            else
            {
                sb.Append(Type switch
                {
                    SyntacticValueType.BOOL => "bool",
                    SyntacticValueType.INT => "int",
                    SyntacticValueType.DOUBLE => "float",
                    SyntacticValueType.STRING => "string",
                    SyntacticValueType.VOID => "void",
                    _ => throw new NotImplementedException(),
                });
            }


            sb.Append(IsArray ? "[]>" : ">");

            return sb.ToString();
        }

        public VariableType ToXirType(ModuleConstructor constructor)
        {
            if (IsArray)
            {
                return Type switch
                {
                    SyntacticValueType.BOOL => ArrayType.IntArrayType,
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
                    SyntacticValueType.BOOL => VariableType.IntType,
                    SyntacticValueType.INT => VariableType.IntType,
                    SyntacticValueType.DOUBLE => VariableType.DoubleType,
                    SyntacticValueType.STRING => throw new NotImplementedException(),
                    SyntacticValueType.CLASS => ToClassType(constructor),
                    SyntacticValueType.VOID => null,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private ClassType ToClassType(ModuleConstructor constructor)
        {
            if (ClassName.Count == 1)
            {
                Class classInfo = constructor.Classes.Find(c => c.Name == ClassName[0]);
                if (classInfo != null)
                {
                    return new ClassType(classInfo.ConstantPoolIndex)
                    {
                        ModuleName = constructor.Module.Name,
                        ClassName = ClassName[0]
                    };
                }
                else
                {
                    throw new XiLangError($"Class {constructor.Module.Name}.{ClassName[0]} does not exist");
                }
            }
            else if (ClassName.Count == 2)
            {
                Program.AssertClassExistence(ClassName[0], ClassName[1]);
                return new ClassType(constructor.AddClassPoolInfo(ClassName[0], ClassName[1]))
                {
                    ModuleName = ClassName[0],
                    ClassName = ClassName[1]
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

}
