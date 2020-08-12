using System;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddCall(int methodIndex)
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.CALL,
                Params = BitConverter.GetBytes(methodIndex)
            });
        }

        public void AddPutC()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUTC
            });
        }

        public void AddPutI()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUTI
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
