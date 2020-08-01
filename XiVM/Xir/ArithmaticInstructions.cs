namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public VariableType AddAddI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.ADDI
            });
            return VariableType.IntType;
        }

        public VariableType AddSubI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SUBI
            });
            return VariableType.IntType;
        }

        public VariableType AddMulI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.MULI
            });
            return VariableType.IntType;
        }

        public VariableType AddDivI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.DIVI
            });
            return VariableType.IntType;
        }

        public VariableType AddMod()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.MOD
            });
            return VariableType.IntType;
        }

        public VariableType AddNegI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.NEGI
            });
            return VariableType.IntType;
        }

        #region CMP

        public VariableType AddSetEqI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETEQI
            });
            return VariableType.ByteType;
        }

        public VariableType AddSetNeI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETNEI
            });
            return VariableType.ByteType;
        }

        public VariableType AddSetLtI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETLTI
            });
            return VariableType.ByteType;
        }

        public VariableType AddSetLeI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETLEI
            });
            return VariableType.ByteType;
        }

        public VariableType AddSetGtI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETGTI
            });
            return VariableType.ByteType;
        }

        public VariableType AddSetGeI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETGEI
            });
            return VariableType.ByteType;
        }

        #endregion


        #region Convert
        public VariableType AddI2D()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.SETEQI
            });
            return VariableType.DoubleType;
        }

        public VariableType AddD2I()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.D2I
            });
            return VariableType.IntType;
        }

        public VariableType AddB2I()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.B2I
            });
            return VariableType.IntType;
        }


        #endregion

    }
}
