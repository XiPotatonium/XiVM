namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {

        #region JMP

        public void AddJmp(BasicBlock target)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.JMP,
                Params = new byte[VariableType.IntSize]
            };
            CurrentInstructions.AddLast(inst);
            CurrentBasicBlock.JmpTargets.Add(target);
        }

        public void AddJCond(BasicBlock target1, BasicBlock target2)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.JCOND,
                Params = new byte[VariableType.IntSize * 2]
            };
            CurrentInstructions.AddLast(inst);
            CurrentBasicBlock.JmpTargets.Add(target1);
            CurrentBasicBlock.JmpTargets.Add(target2);
        }

        #endregion

        #region Ret

        public void AddRet()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RET
            });
        }

        public void AddRetB()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RETB
            });
        }

        public void AddRetI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RETI
            });
        }

        public void AddRetD()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RETD
            });
        }

        public void AddRetA()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RETA
            });
        }

        #endregion


    }
}
