using System;
using System.Collections.Generic;
using System.Text;
using XiVM.Errors;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private int IP;                         // 指令在函数中的IP
        private int FunctionIndex;              // 当前运行的函数的Index
        private BinaryFunction CurrentFunction => Functions[FunctionIndex];

        private Stack Stack { get; } = new Stack();

        private VMModule Module { set; get; }
        private BinaryFunction[] Functions => Module.Functions;
        private LinkedListNode<HeapData>[] StringConstants => Module.StringLiterals;
        private BinaryClassType[] Classes => Module.Classes;

        internal VMExecutor(VMModule module)
        {
            Module = module;
        }

        public void Execute()
        {
            IP = 0;
            FunctionIndex = 0;
            Stack.PushFrame(0, 0);    // 全局的ret ip可以随便填
            PushLocals();

            uint addr, uValue;
            int iValue, lhsi, rhsi, index;
            double dValue;
            byte bValue;

            while (!Stack.Empty)
            {
                BinaryInstruction inst = CurrentFunction.Instructions[IP++];
                switch ((InstructionType)inst.OpCode)
                {
                    case InstructionType.NOP:
                        break;
                    case InstructionType.PUSHB:
                        Stack.PushByte(inst.Params[0]);
                        break;
                    case InstructionType.PUSHI:
                        Stack.PushInt(BitConverter.ToInt32(inst.Params));
                        break;
                    case InstructionType.PUSHD:
                        Stack.PushDouble(BitConverter.ToDouble(inst.Params));
                        break;
                    case InstructionType.PUSHA:
                        Stack.PushAddress(BitConverter.ToUInt32(inst.Params));
                        break;
                    case InstructionType.POPB:
                        Stack.PopByte();
                        break;
                    case InstructionType.POPI:
                        Stack.PopInt();
                        break;
                    case InstructionType.POPD:
                        Stack.PopDouble();
                        break;
                    case InstructionType.POPA:
                        Stack.PopAddress();
                        break;
                    case InstructionType.DUP:
                        Stack.DupN(1);
                        break;
                    case InstructionType.DUP2:
                        Stack.DupN(2);
                        break;
                    case InstructionType.LOCALA:
                        Stack.PushAddress((uint)(Stack.FP + BitConverter.ToInt32(inst.Params)));
                        break;
                    case InstructionType.GLOBALA:
                        Stack.PushAddress((uint)BitConverter.ToInt32(inst.Params));
                        break;
                    case InstructionType.CONSTA:
                        Stack.PushAddress(
                            MemMap.MapTo(StringConstants[BitConverter.ToUInt32(inst.Params)].Value.Offset, MemTag.HEAP));
                        break;
                    case InstructionType.LOADB:
                        addr = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.PushByte(Stack.GetByte(addr));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADI:
                        addr = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.PushInt(Stack.GetInt(addr));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADD:
                        addr = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.PushDouble(Stack.GetDouble(addr));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADA:
                        addr = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.PushAddress(Stack.GetAddress(addr));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STOREB:
                        addr = Stack.PopAddress();
                        bValue = Stack.PopByte();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.SetValue(addr, bValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STOREI:
                        addr = Stack.PopAddress();
                        iValue = Stack.PopInt();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.SetValue(addr, iValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STORED:
                        addr = Stack.PopAddress();
                        dValue = Stack.PopDouble();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.SetValue(addr, dValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STOREA:
                        addr = Stack.PopAddress();
                        uValue = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                Stack.SetValue(addr, uValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ADDI:
                        rhsi = Stack.PopInt();
                        Stack.TopInt = Stack.TopInt + rhsi;
                        break;
                    case InstructionType.SUBI:
                        rhsi = Stack.PopInt();
                        Stack.TopInt = Stack.TopInt - rhsi;
                        break;
                    case InstructionType.MULI:
                        rhsi = Stack.PopInt();
                        Stack.TopInt = Stack.TopInt * rhsi;
                        break;
                    case InstructionType.DIVI:
                        rhsi = Stack.PopInt();
                        Stack.TopInt = Stack.TopInt / rhsi;
                        break;
                    case InstructionType.MOD:
                        rhsi = Stack.PopInt();
                        Stack.TopInt = Stack.TopInt % rhsi;
                        break;
                    case InstructionType.NEGI:
                        Stack.TopInt = -Stack.TopInt;
                        break;
                    case InstructionType.I2D:
                        iValue = Stack.PopInt();
                        Stack.PushDouble((double)iValue);
                        break;
                    case InstructionType.D2I:
                        dValue = Stack.PopDouble();
                        Stack.PushInt((int)dValue);
                        break;
                    case InstructionType.SETEQI:
                    case InstructionType.SETNEI:
                    case InstructionType.SETLTI:
                    case InstructionType.SETLEI:
                    case InstructionType.SETGTI:
                    case InstructionType.SETGEI:
                        rhsi = Stack.PopInt();
                        lhsi = Stack.PopInt();
                        Stack.PushByte((InstructionType)inst.OpCode switch
                        {
                            InstructionType.SETEQI => lhsi == rhsi ? (byte)1 : (byte)0,
                            InstructionType.SETNEI => lhsi != rhsi ? (byte)1 : (byte)0,
                            InstructionType.SETLTI => lhsi < rhsi ? (byte)1 : (byte)0,
                            InstructionType.SETLEI => lhsi <= rhsi ? (byte)1 : (byte)0,
                            InstructionType.SETGTI => lhsi > rhsi ? (byte)1 : (byte)0,
                            InstructionType.SETGEI => lhsi >= rhsi ? (byte)1 : (byte)0,
                            _ => throw new NotImplementedException(),
                        });
                        break;
                    case InstructionType.JMP:
                        IP += BitConverter.ToInt32(inst.Params);
                        break;
                    case InstructionType.JCOND:
                        bValue = Stack.PopByte();
                        IP += bValue == 0 ? BitConverter.ToInt32(inst.Params, sizeof(int)) : BitConverter.ToInt32(inst.Params);
                        break;
                    case InstructionType.CALL:
                        addr = BitConverter.ToUInt32(inst.Params);
                        if (addr == 0)
                        {
                            throw new XiVMError("Call of NULL function is not allowed");
                        }
                        Stack.PushFrame(FunctionIndex, IP);
                        FunctionIndex = (int)addr;
                        IP = 0;
                        PushLocals();
                        break;
                    case InstructionType.RET:
                        index = FunctionIndex;
                        Stack.PopFrame(out FunctionIndex, out IP);
                        PopParams(index);
                        break;
                    case InstructionType.RETB:
                        index = FunctionIndex;
                        bValue = Stack.PopByte();
                        Stack.PopFrame(out FunctionIndex, out IP);
                        PopParams(index);
                        Stack.PushByte(bValue);
                        break;
                    case InstructionType.RETI:
                        index = FunctionIndex;
                        iValue = Stack.PopInt();
                        Stack.PopFrame(out FunctionIndex, out IP);
                        PopParams(index);
                        Stack.PushInt(iValue);
                        break;
                    case InstructionType.RETD:
                        index = FunctionIndex;
                        dValue = Stack.PopDouble();
                        Stack.PopFrame(out FunctionIndex, out IP);
                        PopParams(index);
                        Stack.PushDouble(dValue);
                        break;
                    case InstructionType.RETA:
                        index = FunctionIndex;
                        uValue = Stack.PopAddress();
                        Stack.PopFrame(out FunctionIndex, out IP);
                        PopParams(index);
                        Stack.PushAddress(uValue);
                        break;
                    case InstructionType.PUTC:
                        iValue = Stack.PopInt();
                        Console.Write((char)iValue);
                        break;
                    case InstructionType.PUTS:
                        // 这个地址应该指向一个StringType
                        addr = Stack.PopAddress();
                        switch (MemMap.MapFrom(addr, out addr))
                        {
                            case MemTag.STACK:
                                throw new XiVMError("String should located on the heap");
                            case MemTag.HEAP:
                                LinkedListNode<HeapData> data = Heap.GetHeapData(addr, out index);
                                // TODO 检查MiscData是不是StringType
                                Console.Write(Encoding.ASCII.GetString(data.Value.Data, 
                                    index + HeapData.MiscDataSize + sizeof(int),
                                    BitConverter.ToInt32(new Span<byte>(data.Value.Data, index + HeapData.MiscDataSize, sizeof(int)))));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void PushLocals()
        {
            foreach (var localType in CurrentFunction.LocalTypes)
            {
                // Warning mask是hard coding
                // 局部变量也会初始化
                switch ((VariableTypeTag)(localType & 0x04))
                {
                    case VariableTypeTag.BYTE:
                        Stack.PushByte();
                        break;
                    case VariableTypeTag.INT:
                        Stack.PushInt();
                        break;
                    case VariableTypeTag.DOUBLE:
                        Stack.PushDouble();
                        break;
                    case VariableTypeTag.ADDRESS:
                        Stack.PushAddress();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void PopParams(int calleeIndex)
        {
            foreach (var paramType in Functions[calleeIndex].ParamTypes)
            {
                // Warning mask是hard coding
                switch ((VariableTypeTag)(paramType & 0x04))
                {
                    case VariableTypeTag.BYTE:
                        Stack.PopByte();
                        break;
                    case VariableTypeTag.INT:
                        Stack.PopInt();
                        break;
                    case VariableTypeTag.DOUBLE:
                        Stack.PopDouble();
                        break;
                    case VariableTypeTag.ADDRESS:
                        Stack.PopAddress();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
