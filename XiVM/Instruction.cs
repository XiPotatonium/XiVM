using System;

namespace XiVM
{
    public enum InstructionType
    {
        RET = 0x88,
        RETI = 0x89,
        RETD = 0x8a,
        RETA = 0x8b
    }

    [Serializable]
    public class Instruction
    {
        public static bool IsReturn(Instruction instruction)
        {
            return (instruction.OpCode & 0x88) != 0;
        }

        public byte OpCode { set; get; }
        public byte[] Params { set; get; }
    }
}
