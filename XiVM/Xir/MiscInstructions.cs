using System;
using System.Collections.Generic;
using System.Text;

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
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        public void AddPrintI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PRINTI
            });
        }
    }
}
