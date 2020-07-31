using System;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using XiVM.Errors;
using XiVM.Xir;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private int IP;                         // 指令在函数中的IP
        private int FunctionIndex;              // 当前运行的函数的Index
        private BinaryFunction CurrentFunction => Functions[FunctionIndex];

        private RuntimeStack RuntimeStack { get; } = new RuntimeStack();
        private RuntimeHeap RuntimeHeap { get; } = new RuntimeHeap();
        private ComputationStack ComputationStack { get; } = new ComputationStack();

        private BinaryFunction[] Functions { set; get; }
        private BinaryConstant[] Constants { set; get; }
        private BinaryClass[] Classes { set; get; }

        internal VMExecutor(BinaryModule binaryModule)
        {
            Functions = binaryModule.Functions;
            Constants = binaryModule.Constants;
            Classes = binaryModule.Classes;
        }

        public void Execute()
        {
            IP = 0;
            FunctionIndex = 0;
            RuntimeStack.Push(CurrentFunction.ARSize, 0, 0);    // 全局的ret ip可以随便填

            while (!RuntimeStack.Empty)
            {
                ExecuteSingle();
            }

            if (!ComputationStack.Empty)
            {
                throw new XiVMError("Computation stack is not empty after execution");
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
                    ComputationStack.Push(VariableType.ByteSize);
                    System.Array.Copy(inst.Params, 0, ComputationStack.Data, ComputationStack.Size - VariableType.ByteSize, VariableType.ByteSize);
                    break;
                case InstructionType.PUSHI:
                    ComputationStack.Push(VariableType.IntSize);
                    System.Array.Copy(inst.Params, 0, ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize);
                    break;
                case InstructionType.PUSHD:
                    ComputationStack.Push(VariableType.DoubleSize);
                    System.Array.Copy(inst.Params, 0, ComputationStack.Data, ComputationStack.Size - VariableType.DoubleSize, VariableType.DoubleSize);
                    break;
                case InstructionType.PUSHA:
                    ComputationStack.Push(VariableType.AddressSize);
                    System.Array.Copy(inst.Params, 0, ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize, VariableType.AddressSize);
                    break;
                case InstructionType.POP:
                    ComputationStack.Pop(1);
                    break;
                case InstructionType.POP4:
                    ComputationStack.Pop(4);
                    break;
                case InstructionType.POP8:
                    ComputationStack.Pop(8);
                    break;
                case InstructionType.DUP:
                    ComputationStack.Push(1);
                    System.Array.Copy(ComputationStack.Data, ComputationStack.Size - 2, ComputationStack.Data, ComputationStack.Size - 1, 1);
                    break;
                case InstructionType.DUP4:
                    ComputationStack.Push(4);
                    System.Array.Copy(ComputationStack.Data, ComputationStack.Size - 8, ComputationStack.Data, ComputationStack.Size - 4, 4);
                    break;
                case InstructionType.DUP8:
                    ComputationStack.Push(8);
                    System.Array.Copy(ComputationStack.Data, ComputationStack.Size - 16, ComputationStack.Data, ComputationStack.Size - 8, 8);
                    break;
                case InstructionType.LOCALA:
                    offset = BitConverter.ToInt32(inst.Params);
                    addr = (uint)RuntimeStack.GetLocalIndex(offset);
                    ComputationStack.Push(VariableType.AddressSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize, VariableType.AddressSize), addr);
                    break;
                case InstructionType.GLOBALA:
                    offset = BitConverter.ToInt32(inst.Params);
                    addr = (uint)RuntimeStack.GetGlobalIndex(offset);
                    ComputationStack.Push(VariableType.AddressSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize, VariableType.AddressSize), addr);
                    break;
                case InstructionType.LOADB:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        ComputationStack.Push(VariableType.ByteSize);
                        ComputationStack.Data[ComputationStack.Size - VariableType.ByteSize] = RuntimeStack.Data[addr];
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADI:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        ComputationStack.Push(VariableType.IntSize);
                        BitConverter.TryWriteBytes(
                            new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize),
                            BitConverter.ToInt32(RuntimeStack.Data, (int)addr));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADD:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        ComputationStack.Push(VariableType.DoubleSize);
                        BitConverter.TryWriteBytes(
                            new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.DoubleSize, VariableType.DoubleSize),
                            BitConverter.ToDouble(RuntimeStack.Data, (int)addr));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.LOADA:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        ComputationStack.Push(VariableType.AddressSize);
                        BitConverter.TryWriteBytes(
                            new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize, VariableType.AddressSize),
                            BitConverter.ToUInt32(RuntimeStack.Data, (int)addr));
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STOREB:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(new Span<byte>(RuntimeStack.Data, (int)addr, VariableType.IntSize), 
                            ComputationStack.Data[ComputationStack.Size - VariableType.ByteSize]);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    ComputationStack.Pop(VariableType.ByteSize);
                    break;
                case InstructionType.STOREI:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    iValue = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(new Span<byte>(RuntimeStack.Data, (int)addr, VariableType.IntSize), iValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STORED:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    dValue = BitConverter.ToDouble(ComputationStack.Data, ComputationStack.Size - VariableType.DoubleSize);
                    ComputationStack.Pop(VariableType.DoubleSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(new Span<byte>(RuntimeStack.Data, (int)addr, VariableType.DoubleSize), dValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.STOREA:
                    addr = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    uValue = BitConverter.ToUInt32(ComputationStack.Data, ComputationStack.Size - VariableType.AddressSize);
                    ComputationStack.Pop(VariableType.AddressSize);
                    if ((addr & 0x10000000) == 0)
                    {
                        // 栈地址
                        BitConverter.TryWriteBytes(new Span<byte>(RuntimeStack.Data, (int)addr, VariableType.AddressSize), uValue);
                    }
                    else
                    {
                        // 堆地址
                        throw new NotImplementedException();
                    }
                    break;
                case InstructionType.ADDI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), lhsi + rhsi);
                    break;
                case InstructionType.SUBI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), lhsi - rhsi);
                    break;
                case InstructionType.MULI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), lhsi * rhsi);
                    break;
                case InstructionType.DIVI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), lhsi / rhsi);
                    break;
                case InstructionType.MOD:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), lhsi % rhsi);
                    break;
                case InstructionType.NEGI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), -lhsi);
                    break;
                case InstructionType.I2D:
                    iValue = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    ComputationStack.Push(VariableType.DoubleSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.DoubleSize, VariableType.DoubleSize), (double)iValue);
                    break;
                case InstructionType.D2I:
                    dValue = BitConverter.ToDouble(ComputationStack.Data, ComputationStack.Size - VariableType.DoubleSize);
                    ComputationStack.Pop(VariableType.DoubleSize);
                    ComputationStack.Push(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), (int)dValue);
                    break;
                case InstructionType.B2I:
                    bValue = ComputationStack.Data[ComputationStack.Size - VariableType.ByteSize];
                    ComputationStack.Pop(VariableType.ByteSize);
                    ComputationStack.Push(VariableType.IntSize);
                    BitConverter.TryWriteBytes(
                        new Span<byte>(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize, VariableType.IntSize), (int)bValue);
                    break;
                case InstructionType.SETEQI:
                    lhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - 2 * VariableType.IntSize);
                    rhsi = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(2 * VariableType.IntSize);
                    ComputationStack.Push(VariableType.ByteSize);
                    ComputationStack.Data[ComputationStack.Size - VariableType.ByteSize] = lhsi == rhsi ? (byte)1 : (byte)0;
                    break;
                case InstructionType.JMP:
                    offset = BitConverter.ToInt32(inst.Params);
                    IP += offset;
                    break;
                case InstructionType.JCOND:
                    offset = BitConverter.ToInt32(inst.Params);
                    offset1 = BitConverter.ToInt32(inst.Params, sizeof(int));
                    bValue = ComputationStack.Data[ComputationStack.Size - VariableType.ByteSize];
                    ComputationStack.Pop(VariableType.ByteSize);
                    IP += bValue == 0 ? offset1 : offset;
                    break;
                case InstructionType.CALL:
                    addr = BitConverter.ToUInt32(inst.Params);
                    if (addr == 0)
                    {
                        throw new XiVMError("Call of NULL function is not allowed");
                    }
                    RuntimeStack.Push(Functions[addr].ARSize, FunctionIndex, IP);
                    FunctionIndex = (int)addr;
                    IP = 0;
                    break;
                case InstructionType.RET:
                    RuntimeStack.Pop(out FunctionIndex, out IP);
                    break;
                case InstructionType.PRINTI:
                    iValue = BitConverter.ToInt32(ComputationStack.Data, ComputationStack.Size - VariableType.IntSize);
                    ComputationStack.Pop(VariableType.IntSize);
                    Console.Write(iValue);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
