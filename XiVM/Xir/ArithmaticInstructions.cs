namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public VariableType AddAddI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.ADDI
            });
            return VariableType.IntType;
        }

        public VariableType AddSubI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SUBI
            });
            return VariableType.IntType;
        }

        public VariableType AddMulI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.MULI
            });
            return VariableType.IntType;
        }

        public VariableType AddDivI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DIVI
            });
            return VariableType.IntType;
        }

        public VariableType AddMod()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.MOD
            });
            return VariableType.IntType;
        }

        public VariableType AddNegI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.NEGI
            });
            return VariableType.IntType;
        }

        #region CMP

        public VariableType AddSetEqI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETEQI
            });
            return VariableType.ByteType;
        }

        #endregion


        #region Convert
        public VariableType AddI2D()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETEQI
            });
            return VariableType.DoubleType;
        }

        public VariableType AddD2I()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.D2I
            });
            return VariableType.IntType;
        }

        public VariableType AddB2I()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.B2I
            });
            return VariableType.IntType;
        }


        #endregion

    }
}
