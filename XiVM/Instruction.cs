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

        LOCALA = 0x18,
        GLOBALA = 0x19,
        CONSTA = 0x1A,
        NEW = 0x1B,

        LOADB = 0x20,
        LOADI = 0x21,
        LOADD = 0x22,
        LOADA = 0x23,

        STOREB = 0x28,
        STOREI = 0x29,
        STORED = 0x2A,
        STOREA = 0x2B,

        ALOADB = 0x30,
        ALOADI = 0x31,
        ALOADD = 0x32,
        ALOADA = 0x33,

        ASTOREB = 0x38,
        ASTOREI = 0x39,
        ASTORED = 0x3A,
        ASTOREA = 0x3B,

        ADDI = 0x40,

        SUBI = 0x44,

        MULI = 0x48,

        DIVI = 0x4C,

        MOD = 0x50,

        NEGI = 0x54,

        I2D = 0x60,
        D2I = 0x61,
        B2I = 0x62,

        SETEQI = 0x70,

        JMP = 0x80,
        JCOND = 0x81,

        CALL = 0x90,
        RET = 0x94,
        RETB = 0x95,
        RETI = 0x96,
        RETD = 0x97,
        RETA = 0x98,

        PRINTI = 0xA0,
        PRINTS = 0xA4,
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
        public bool IsBranch => IsRet ||
            OpCode == InstructionType.JMP ||
            OpCode == InstructionType.JCOND;
        public bool IsRet => OpCode == InstructionType.RET ||
            OpCode == InstructionType.RETB ||
            OpCode == InstructionType.RETI ||
            OpCode == InstructionType.RETD ||
            OpCode == InstructionType.RETA;


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
                InstructionType.LOCALA => $"LOCALA {BitConverter.ToInt32(Params)}",
                InstructionType.GLOBALA => $"GLOBALA {BitConverter.ToInt32(Params)}",
                InstructionType.LOADB => "LOADB",
                InstructionType.LOADI => "LOADI",
                InstructionType.LOADD => "LOADD",
                InstructionType.LOADA => "LOADA",
                InstructionType.STOREB => "STOREB",
                InstructionType.STOREI => "STOREI",
                InstructionType.STORED => "STORED",
                InstructionType.STOREA => "STOREA",
                InstructionType.ADDI => "ADDI",
                InstructionType.CALL => $"CALL {BitConverter.ToUInt32(Params)}",
                InstructionType.RET => "RET",
                InstructionType.RETB => "RETB",
                InstructionType.RETI => "RETI",
                InstructionType.RETD => "RETD",
                InstructionType.RETA => "RETA",
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
                InstructionType.JCOND => $"JCOND {BitConverter.ToInt32(Params)} {BitConverter.ToInt32(Params, sizeof(int))}",
                InstructionType.CONSTA => $"CONSTA {BitConverter.ToInt32(Params)}",
                InstructionType.NEW => throw new NotImplementedException(),
                InstructionType.ALOADB => throw new NotImplementedException(),
                InstructionType.ALOADI => throw new NotImplementedException(),
                InstructionType.ALOADD => throw new NotImplementedException(),
                InstructionType.ALOADA => throw new NotImplementedException(),
                InstructionType.ASTOREB => throw new NotImplementedException(),
                InstructionType.ASTOREI => throw new NotImplementedException(),
                InstructionType.ASTORED => throw new NotImplementedException(),
                InstructionType.ASTOREA => throw new NotImplementedException(),
                InstructionType.PRINTS => "PRINTS",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
