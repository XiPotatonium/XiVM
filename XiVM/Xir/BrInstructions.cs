using System;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {

        #region JMP

        public void AddJmp(int offset)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.JMP,
                Params = new byte[VariableType.IntSize]
            };
            BitConverter.TryWriteBytes(inst.Params, offset);
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        public void AddJCond(int offset)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.JCOND,
                Params = new byte[VariableType.IntSize]
            };
            BitConverter.TryWriteBytes(inst.Params, offset);
            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        #endregion

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
