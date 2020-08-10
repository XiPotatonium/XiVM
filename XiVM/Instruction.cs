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

        POPB = 0x08,
        POPI = 0x09,
        POPD = 0x0A,
        POPA = 0x0B,

        DUP = 0x10,
        DUP2 = 0x11,

        LOCAL = 0x18,
        CONST = 0x19,
        STATIC = 0x1A,
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

        SETEQI = 0x70,
        SETNEI = 0x71,
        SETLTI = 0x72,
        SETLEI = 0x73,
        SETGTI = 0x74,
        SETGEI = 0x75,

        JMP = 0x80,
        JCOND = 0x81,

        CALL = 0x90,
        RET = 0x94,

        PUTC = 0xA0,
        PUTS = 0xA1,
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
        public bool IsRet => OpCode == InstructionType.RET;


        public override string ToString()
        {
            return OpCode switch
            {
                InstructionType.NOP => "NOP",
                InstructionType.PUSHB => $"PUSHB {Params[0]}",
                InstructionType.PUSHI => $"PUSHI {BitConverter.ToInt32(Params)}",
                InstructionType.PUSHD => $"PUSHD {BitConverter.ToDouble(Params)}",
                InstructionType.PUSHA => $"PUSHA {BitConverter.ToUInt32(Params)}",
                InstructionType.POPB => "POPB",
                InstructionType.POPI => "POPI",
                InstructionType.POPD => "POPD",
                InstructionType.POPA => "POPA",
                InstructionType.DUP => "DUP",
                InstructionType.DUP2 => "DUP2",
                InstructionType.LOCAL => $"LOCAL {BitConverter.ToInt32(Params)}",
                InstructionType.CONST => $"CONST {BitConverter.ToInt32(Params)}",
                InstructionType.STATIC => $"STATIC {BitConverter.ToInt32(Params)}",
                InstructionType.NEW => throw new NotImplementedException(),
                InstructionType.LOADB => "LOADB",
                InstructionType.LOADI => "LOADI",
                InstructionType.LOADD => "LOADD",
                InstructionType.LOADA => "LOADA",
                InstructionType.STOREB => "STOREB",
                InstructionType.STOREI => "STOREI",
                InstructionType.STORED => "STORED",
                InstructionType.STOREA => "STOREA",
                InstructionType.ALOADB => "ALOADB",
                InstructionType.ALOADI => "ALOADI",
                InstructionType.ALOADD => "ALOADD",
                InstructionType.ALOADA => "ALOADA",
                InstructionType.ASTOREB => "ASTOREB",
                InstructionType.ASTOREI => "ASTOREI",
                InstructionType.ASTORED => "ASTORED",
                InstructionType.ASTOREA => "ASTOREA",
                InstructionType.ADDI => "ADDI",
                InstructionType.CALL => $"CALL {BitConverter.ToInt32(Params)}",
                InstructionType.RET => "RET",
                InstructionType.SUBI => "SUBI",
                InstructionType.MULI => "MULI",
                InstructionType.DIVI => "DIVI",
                InstructionType.MOD => "MOD",
                InstructionType.NEGI => "NEGI",
                InstructionType.SETEQI => "SETEQI",
                InstructionType.SETNEI => "SETNEI",
                InstructionType.SETLTI => "SETLTI",
                InstructionType.SETLEI => "SETLEI",
                InstructionType.SETGTI => "SETGTI",
                InstructionType.SETGEI => "SETGEI",
                InstructionType.I2D => "I2D",
                InstructionType.D2I => "D2I",
                InstructionType.JMP => $"JMP {BitConverter.ToInt32(Params)}",
                InstructionType.JCOND => $"JCOND {BitConverter.ToInt32(Params)} {BitConverter.ToInt32(Params, sizeof(int))}",
                InstructionType.PUTC => "PUTC",
                InstructionType.PUTS => "PUTS",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
