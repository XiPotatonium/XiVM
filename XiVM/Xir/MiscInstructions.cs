using System;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddCall(uint index)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.CALL,
                Params = new byte[VariableType.AddressSize]
            };
            BitConverter.TryWriteBytes(inst.Params, index);
            CurrentInstructions.AddLast(inst);
        }

        public void AddPutC()
        {
            CurrentInstructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PUTC
            });
        }
    }
}
