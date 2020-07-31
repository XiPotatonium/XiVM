using System;

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
            CurrentBasicBlock.Instructions.AddLast(inst);
            CurrentBasicBlock.JmpTargets.Add(target);
        }

        public void AddJCond(BasicBlock target1, BasicBlock target2)
        {
            Instruction inst = new Instruction()
            {
                OpCode = InstructionType.JCOND,
                Params = new byte[VariableType.IntSize * 2]
            };
            CurrentBasicBlock.Instructions.AddLast(inst);
            CurrentBasicBlock.JmpTargets.Add(target1);
            CurrentBasicBlock.JmpTargets.Add(target2);
        }

        #endregion

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
