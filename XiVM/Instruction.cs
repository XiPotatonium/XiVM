using System;

namespace XiVM
{
    public enum InstructionType
    {
        NOP = 0x00,

        PUSHB = 0x01,
        PUSHI = 0x02,
        PUSHD = 0x03,
        PUSHA = 0x04,

        POP = 0x08,
        POP4 = 0x09,
        POP8 = 0x0A,

        DUP = 0x10,
        DUP4 = 0x11,
        DUP8 = 0x12,

        GETA = 0x18,

        LOADB = 0x20,
        LOADI = 0x21,
        LOADD = 0x22,
        LOADA = 0x23,

        STOREB = 0x30,
        STOREI = 0x31,
        STORED = 0x32,
        STOREA = 0x33,

        ADDI = 0x40,

        CALL = 0x80,
        RET = 0x81
    }

    [Serializable]
    public class BinaryInstruction
    {
        public byte OpCode { set; get; }
        public byte[] Params { set; get; }
    }

    public class Instruction
    {
        public InstructionType OpCode { set; get; }
        public byte[] Params { set; get; }
    }
}
