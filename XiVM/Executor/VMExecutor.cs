using System;
using XiVM.Errors;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private int IP;                         // 指令在函数中的IP
        private int FunctionIndex;              // 当前运行的函数的Index
        private BinaryFunction CurrentFunction => Functions[FunctionIndex];

        private Stack Stack { get; } = new Stack();

        private BinaryModule Module { set; get; }
        private BinaryFunction[] Functions => Module.Functions;
        private string[] StringConstants => Module.StringLiterals;
        private BinaryClass[] Classes => Module.Classes;

        internal VMExecutor(BinaryModule module)
        {
            Module = module;
        }

        public void Execute()
        {
            IP = 0;
            FunctionIndex = 0;
            Stack.PushFrame(CurrentFunction.LocalSize, 0, 0);    // 全局的ret ip可以随便填

            while (!Stack.Empty)
            {
                ExecuteSingle();
            }
        }

        private void ExecuteSingle()
        {
            BinaryInstruction inst = CurrentFunction.Instructions[IP++];
            uint addr, uValue;
            int iValue, offset, offset1, lhsi, rhsi;
            double dValue;
            byte bValue;
            switch ((InstructionType)inst.OpCode)
            {
                case InstructionType.NOP:
                    break;
                case InstructionType.PUSHB:
                    Stack.PushN(VariableType.ByteSize, inst.Params, 0);
                    break;
                case InstructionType.PUSHI:
                    Stack.PushN(VariableType.IntSize, inst.Params, 0);
                    break;
                case InstructionType.PUSHD:
                    Stack.PushN(VariableType.DoubleSize, inst.Params, 0);
                    break;
                case InstructionType.PUSHA:
                    Stack.PushN(VariableType.AddressSize, inst.Params, 0);
                    break;
                case InstructionType.POP:
                    Stack.PopN(1);
                    break;
                case InstructionType.POP4:
                    Stack.PopN(4);
                    break;
                case InstructionType.POP8:
                    Stack.PopN(8);
                    break;
                case InstructionType.DUP:
                    Stack.DupN(1);
                    break;
                case InstructionType.DUP4:
                    Stack.DupN(4);
                    break;
                case InstructionType.DUP8:
                    Stack.DupN(8);
                    break;
                case InstructionType.LOCALA:
                    offset = BitConverter.ToInt32(inst.Params);
                    addr = (uint)(Stack.FP + offset);
                    Stack.PushN(VariableType.AddressSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.AddressSize), addr);
                    break;
                case InstructionType.GLOBALA:
                    offset = BitConverter.ToInt32(inst.Params);
                    addr = (uint)offset;
                    Stack.PushN(VariableType.AddressSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.AddressSize), addr);
                    break;
                case InstructionType.CONSTA:
                    throw new NotImplementedException();
                case InstructionType.LOADB:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        Stack.PushN(VariableType.ByteSize);
                        Stack.StoreTopByte(Stack.LoadByte((int)addr));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADI:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        Stack.PushN(VariableType.IntSize);
                        BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize),
                            BitConverter.ToInt32(Stack.GetSpan((int)addr, VariableType.IntSize)));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADD:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        Stack.PushN(VariableType.DoubleSize);
                        BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.DoubleSize),
                            BitConverter.ToDouble(Stack.GetSpan((int)addr, VariableType.DoubleSize)));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADA:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        Stack.PushN(VariableType.AddressSize);
                        BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.AddressSize),
                            BitConverter.ToUInt32(Stack.GetSpan((int)addr, VariableType.AddressSize)));
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STOREB:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    bValue = Stack.LoadTopByte();
                    Stack.PopN(VariableType.ByteSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        Stack.StoreByte((int)addr, bValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STOREI:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    iValue = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(Stack.GetSpan((int)addr, VariableType.IntSize), iValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STORED:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    dValue = BitConverter.ToDouble(Stack.GetTopSpan(VariableType.DoubleSize));
                    Stack.PopN(VariableType.DoubleSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(Stack.GetSpan((int)addr, VariableType.DoubleSize), dValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STOREA:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    uValue = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(Stack.GetSpan((int)addr, VariableType.AddressSize), uValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    // TODO 可能会发生GC
                    break;
                case InstructionType.ADDI:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), lhsi + rhsi);
                    break;
                case InstructionType.SUBI:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), lhsi - rhsi);
                    break;
                case InstructionType.MULI:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), lhsi * rhsi);
                    break;
                case InstructionType.DIVI:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), lhsi / rhsi);
                    break;
                case InstructionType.MOD:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), lhsi % rhsi);
                    break;
                case InstructionType.NEGI:
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), -lhsi);
                    break;
                case InstructionType.I2D:
                    iValue = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    Stack.PushN(VariableType.DoubleSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.DoubleSize), (double)iValue);
                    break;
                case InstructionType.D2I:
                    dValue = BitConverter.ToDouble(Stack.GetTopSpan(VariableType.DoubleSize));
                    Stack.PopN(VariableType.DoubleSize);
                    Stack.PushN(VariableType.IntSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), (int)dValue);
                    break;
                case InstructionType.B2I:
                    bValue = Stack.LoadTopByte();
                    Stack.PopN(VariableType.ByteSize);
                    Stack.PushN(VariableType.IntSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), (int)bValue);
                    break;
                case InstructionType.SETEQI:
                    rhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    lhsi = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    Stack.PushN(VariableType.ByteSize);
                    Stack.StoreTopByte(lhsi == rhsi ? (byte)1 : (byte)0);
                    break;
                case InstructionType.JMP:
                    offset = BitConverter.ToInt32(inst.Params);
                    IP += offset;
                    break;
                case InstructionType.JCOND:
                    offset = BitConverter.ToInt32(inst.Params);
                    offset1 = BitConverter.ToInt32(inst.Params, sizeof(int));
                    bValue = Stack.LoadTopByte();
                    Stack.PopN(VariableType.ByteSize);
                    IP += bValue == 0 ? offset1 : offset;
                    break;
                case InstructionType.CALL:
                    addr = BitConverter.ToUInt32(inst.Params);
                    if (addr == 0)
                    {
                        throw new XiVMError("Call of NULL function is not allowed");
                    }
                    Stack.PushFrame(Functions[addr].LocalSize, FunctionIndex, IP);
                    FunctionIndex = (int)addr;
                    IP = 0;
                    break;
                case InstructionType.RET:
                    Stack.PopFrame(CurrentFunction.ParamSize, out FunctionIndex, out IP);
                    break;
                case InstructionType.RETB:
                    bValue = Stack.LoadTopByte();
                    Stack.PopN(VariableType.ByteSize);
                    Stack.PopFrame(CurrentFunction.ParamSize, out FunctionIndex, out IP);
                    Stack.PushN(VariableType.ByteSize);
                    Stack.StoreTopByte(bValue);
                    break;
                case InstructionType.RETI:
                    iValue = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    Stack.PopFrame(CurrentFunction.ParamSize, out FunctionIndex, out IP);
                    Stack.PushN(VariableType.IntSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.IntSize), iValue);
                    break;
                case InstructionType.RETD:
                    dValue = BitConverter.ToDouble(Stack.GetTopSpan(VariableType.DoubleSize));
                    Stack.PopN(VariableType.DoubleSize);
                    Stack.PopFrame(CurrentFunction.ParamSize, out FunctionIndex, out IP);
                    Stack.PushN(VariableType.DoubleSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.DoubleSize), dValue);
                    break;
                case InstructionType.RETA:
                    uValue = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    Stack.PopFrame(CurrentFunction.ParamSize, out FunctionIndex, out IP);
                    Stack.PushN(VariableType.AddressSize);
                    BitConverter.TryWriteBytes(Stack.GetTopSpan(VariableType.AddressSize), uValue);
                    break;
                case InstructionType.PRINTI:
                    iValue = BitConverter.ToInt32(Stack.GetTopSpan(VariableType.IntSize));
                    Stack.PopN(VariableType.IntSize);
                    Console.Write(iValue);
                    break;
                case InstructionType.PRINTS:
                    addr = BitConverter.ToUInt32(Stack.GetTopSpan(VariableType.AddressSize));
                    Stack.PopN(VariableType.AddressSize);
                    Console.Write(StringConstants[(int)addr]);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
