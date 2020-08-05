using System;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddCall(uint index)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.CALL,
                Params = BitConverter.GetBytes(index)
            });
        }

        public void AddPutC()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUTC
            });
        }

        public void AddPutS()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUTS
            });
        }
    }
}
