namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {

        public void AddCall()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.CALL,
            });
        }

        #region Ret

        public void AddRet()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.RET
            });
        }

        #endregion
    }
}
