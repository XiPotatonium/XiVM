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
    }
}
