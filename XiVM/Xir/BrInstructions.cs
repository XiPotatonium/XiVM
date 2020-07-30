using System;

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

        public void AddRetT(VariableType retType)
        {
            Instruction inst;

            if (retType == null)
            {
                inst = new Instruction()
                {
                    OpCode = InstructionType.RET
                };
            }
            else
            {
                inst = new Instruction()
                {
                    OpCode = retType.Tag switch
                    {
                        VariableTypeTag.BYTE => InstructionType.RETB,
                        VariableTypeTag.INT => InstructionType.RETI,
                        VariableTypeTag.DOUBLE => InstructionType.RETD,
                        VariableTypeTag.ADDRESS => InstructionType.RETA,
                        _ => throw new NotImplementedException(),
                    }
                };
            }

            CurrentBasicBlock.Instructions.AddLast(inst);
        }

        #endregion
    }
}
