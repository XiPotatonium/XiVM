using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public void AddPrintI()
        {
            CurrentBasicBlock.Instructions.AddLast(new Instruction()
            {
                OpCode = InstructionType.PRINTI
            });
        }
    }
}
