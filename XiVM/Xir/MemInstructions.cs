using System;
using System.Diagnostics;
using XiVM.Errors;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddGetA(uint levelDiff, int offset)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.GETA
            };
            inst.Params = new byte[sizeof(uint) + sizeof(int)];
            BitConverter.TryWriteBytes(new Span<byte>(inst.Params, 0, sizeof(uint)), levelDiff);
            BitConverter.TryWriteBytes(new Span<byte>(inst.Params, sizeof(uint), sizeof(int)), offset);
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        #region Push

        public void AddPushB(byte value)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.PUSHB,
                Params = new byte[VariableType.ByteSize]
            };
            inst.Params[0] = value;
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        public void AddPushI(int value)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.PUSHI,
                Params = new byte[VariableType.IntSize]
            };
            BitConverter.TryWriteBytes(inst.Params, value);
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        public void AddPushD(double value)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.PUSHD,
                Params = new byte[VariableType.DoubleSize]
            };
            BitConverter.TryWriteBytes(inst.Params, value);
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        /// <summary>
        /// 之应该用于Call的Index或者NULL
        /// </summary>
        /// <param name="value"></param>
        public void AddPushA(uint value)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.PUSHA,
                Params = new byte[VariableType.AddressSize]
            };
            BitConverter.TryWriteBytes(inst.Params, value);
            CurrentBasicBlock.Instructions.AddLast(inst);

        }

        #endregion

        #region Pop

        public void AddPopValue(VariableType valueType)
        {
            int size = valueType.Size;
            while (size >= 8)
            {
                AddPop8();
                size -= 8;
            }

            while (size >= 4)
            {
                AddPop4();
                size -= 4;
            }

            while (size >= 1)
            {
                AddPop();
                size -= 1;
            }
        }

        private void AddPop()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.POP
            });
        }

        private void AddPop4()
        {
            Debug.Assert(VariableType.IntSize == 4);
            Debug.Assert(VariableType.AddressSize == 4);

            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.POP4
            });
        }

        private void AddPop8()
        {
            Debug.Assert(VariableType.DoubleSize == 8);

            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.POP8
            });
        }

        #endregion

        #region Dup

        public void AddDupT(VariableType type)
        {
            switch (type.Tag)
            {
                case VariableTypeTag.BYTE:
                    AddDup();
                    break;
                case VariableTypeTag.INT:
                    AddDup4();
                    break;
                case VariableTypeTag.DOUBLE:
                    AddDup8();
                    break;
                case VariableTypeTag.ADDRESS:
                    AddDup4();
                    break;
                default:
                    throw new XiVMError($"Unknown type {type.Tag} for TLoad");
            }
        }

        private void AddDup()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DUP
            });
        }

        private void AddDup4()
        {
            Debug.Assert(VariableType.IntSize == 4);
            Debug.Assert(VariableType.AddressSize == 4);
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DUP4
            });
        }

        private void AddDup8()
        {
            Debug.Assert(VariableType.DoubleSize == 8);
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DUP8
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

        private void AddLoadB()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADB
            });
        }

        private void AddLoadI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADI
            });

        }

        private void AddLoadD()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.LOADD
            });
        }

        private void AddLoadA()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
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

        private void AddStoreB()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREB
            });
        }

        private void AddStoreI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREI
            });

        }

        private void AddStoreD()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STORED
            });
        }

        private void AddStoreA()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.STOREA
            });
        }

        #endregion
    }
}
