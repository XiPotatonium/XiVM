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

        STOREB = 0x28,
        STOREI = 0x29,
        STORED = 0x2A,
        STOREA = 0x2B,

        ADDI = 0x30,

        SUBI = 0x34,

        MULI = 0x38,

        DIVI = 0x3C,

        MOD = 0x40,

        NEGI = 0x44,

        I2D = 0x50,
        D2I = 0x51,
        B2I = 0x52,

        SETEQI = 0x60,

        JMP = 0x70,
        JCOND = 0x71,

        CALL = 0x80,
        RET = 0x84,

        PRINTI = 0xA0
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

        public override string ToString()
        {
            return OpCode switch
            {
                InstructionType.NOP => "NOP",
                InstructionType.PUSHB => $"PUSHB {Params[0]}",
                InstructionType.PUSHI => $"PUSHI {BitConverter.ToInt32(Params)}",
                InstructionType.PUSHD => $"PUSHD {BitConverter.ToDouble(Params)}",
                InstructionType.PUSHA => $"PUSHA {BitConverter.ToUInt32(Params)}",
                InstructionType.POP => "POP",
                InstructionType.POP4 => "POP4",
                InstructionType.POP8 => "POP8",
                InstructionType.DUP => "DUP",
                InstructionType.DUP4 => "DUP4",
                InstructionType.DUP8 => "DUP8",
                InstructionType.GETA => $"GETA {BitConverter.ToInt32(Params)} {BitConverter.ToInt32(Params, sizeof(int))}",
                InstructionType.LOADB => "LOADB",
                InstructionType.LOADI => "LOADI",
                InstructionType.LOADD => "LOADD",
                InstructionType.LOADA => "LOADA",
                InstructionType.STOREB => "STOREB",
                InstructionType.STOREI => "STOREI",
                InstructionType.STORED => "STORED",
                InstructionType.STOREA => "STOREA",
                InstructionType.ADDI => "ADDI",
                InstructionType.CALL => "CALL",
                InstructionType.RET => "RET",
                InstructionType.PRINTI => "PRINTI",
                InstructionType.SUBI => "SUBI",
                InstructionType.MULI => "MULI",
                InstructionType.DIVI => "DIVI",
                InstructionType.MOD => "MOD",
                InstructionType.NEGI => "NEGI",
                InstructionType.SETEQI => "SETEQI",
                InstructionType.I2D => "I2D",
                InstructionType.D2I => "D2I",
                InstructionType.B2I => "B2I",
                InstructionType.JMP => $"JMP {BitConverter.ToInt32(Params)}",
                InstructionType.JCOND => $"JCOND {BitConverter.ToInt32(Params)}",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
