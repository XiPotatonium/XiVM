using System;
using XiVM.Errors;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddGetStaticFieldAddress(ClassField field)
        {
            if (!field.AccessFlag.IsStatic)
            {
                throw new XiVMError("AddGetStaticFieldAddress only accept static field");
            }

            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STATIC,
                Params = BitConverter.GetBytes(field.ConstantPoolIndex)
            });
        }

        public void AddLocal(int offset)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOCAL,
                Params = BitConverter.GetBytes(offset)
            });
        }

        public void AddConst(int index)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.CONST,
                Params = BitConverter.GetBytes(index)
            });
        }

        #region Push

        public void AddPushB(byte value)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUSHB,
                Params = new byte[1] { value }
            });
        }

        public void AddPushI(int value)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUSHI,
                Params = BitConverter.GetBytes(value)
            });
        }

        public void AddPushD(double value)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUSHD,
                Params = BitConverter.GetBytes(value)
            });
        }

        public void AddPushA(uint value)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUSHA,
                Params = BitConverter.GetBytes(value)
            });
        }

        #endregion

        #region Pop

        public void AddPop(VariableType type)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = type.Tag switch
                {
                    VariableTypeTag.BYTE => InstructionType.POPB,
                    VariableTypeTag.INT => InstructionType.POPI,
                    VariableTypeTag.DOUBLE => InstructionType.POPD,
                    VariableTypeTag.ADDRESS => InstructionType.POPA,
                    _ => throw new NotImplementedException(),
                }
            });
        }

        #endregion

        #region Dup

        public void AddDup(VariableType type)
        {
            if (type.SlotSize == 2)
            {
                AddDup2();
            }
            else if (type.SlotSize == 1)
            {
                AddDup();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void AddDup()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DUP
            });
        }

        private void AddDup2()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DUP2
            });
        }

        #endregion

        #region Load

        public void AddLoadT(VariableType type)
        {
            switch (type.Tag)
            {
                case VariableTypeTag.BYTE:
                    AddLoadB();
                    break;
                case VariableTypeTag.INT:
                    AddLoadI();
                    break;
                case VariableTypeTag.DOUBLE:
                    AddLoadD();
                    break;
                case VariableTypeTag.ADDRESS:
                    AddLoadA();
                    break;
                default:
                    throw new XiVMError($"Unknown type {type.Tag} for TLoad");
            }
        }

        internal void AddLoadB()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADB
            });
        }

        internal void AddLoadI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADI
            });

        }

        internal void AddLoadD()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADD
            });
        }

        internal void AddLoadA()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADA
            });
        }

        #endregion

        #region Store

        public void AddStoreT(VariableType type)
        {
            switch (type.Tag)
            {
                case VariableTypeTag.BYTE:
                    AddStoreB();
                    break;
                case VariableTypeTag.INT:
                    AddStoreI();
                    break;
                case VariableTypeTag.DOUBLE:
                    AddStoreD();
                    break;
                case VariableTypeTag.ADDRESS:
                    AddStoreA();
                    break;
                default:
                    throw new XiVMError($"Unknown type {type.Tag} for TLoad");
            }
        }

        internal void AddStoreB()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREB
            });
        }

        internal void AddStoreI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREI
            });

        }

        internal void AddStoreD()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STORED
            });
        }

        internal void AddStoreA()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREA
            });
        }

        #endregion
    }
}
