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
    }
}
