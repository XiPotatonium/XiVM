using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiVM.ConstantTable;
using XiVM.Errors;

namespace XiVM.Runtime
{
    public class VMExecutor
    {
        private int IP { set; get; }                        // 指令在函数中的IP
        private VMMethod CurrentMethod { set; get; }
        private int MethodIndex => CurrentMethod.MethodIndex;
        private byte[] CurrentInstructions => CurrentMethod.CodeBlock.Value.Data;
        private VMClass CurrentClass => CurrentMethod.Parent;
        private VMModule CurrentModule => CurrentClass.Parent;

        private Stack Stack { get; } = new Stack();

        private VMModule Module { set; get; }
        private List<uint> StringConstants => Module.StringPoolLink;

        internal VMExecutor(VMModule module)
        {
            Module = module;
        }

        public void ExecuteStaticConstructor()
        {
            foreach ((MethodConstantInfo methodInfo, var method) in Module.MethodPool.Zip(Module.MethodPoolLink))
            {
                if (Module.StringPoolLink[methodInfo.Name - 1] == MethodArea.StaticConstructorNameAddress)
                {
                    // 静态构造
                    CurrentMethod = method;
                    Execute();
                }
            }
        }

        public void ExecuteMain()
        {
            // 定位Main函数
            if (!Module.Classes.TryGetValue(MethodArea.StringProgramAddress, out VMClass entryClass))
            {
                throw new XiVMError("Program.Main() not found");
            }

            if (!entryClass.Methods.TryGetValue(MethodArea.StringMainAddress, out List<VMMethod> entryMethodGroup))
            {
                throw new XiVMError("Program.Main() not found");
            }

            // TODO 有string后改成(L)V
            foreach (VMMethod method in entryMethodGroup)
            {
                if (method.DescriptorAddress == MethodArea.StringMainDescriptorAddress)
                {
                    CurrentMethod = method;
                    break;
                }
            }

            if (CurrentMethod == null)
            {
                throw new XiVMError("Program.Main() not found");
            }

            // TODO Main的参数
            Execute();
        }

        private void Execute()
        {
            IP = 0;
            Stack.PushFrame(0, 0);    // 全局的ret ip可以随便填
            PushLocals();

            uint addr, uValue;
            int iValue, lhsi, rhsi, index, ip;
            double dValue;
            byte[] data;

            while (!Stack.Empty)
            {
                byte opCode = CurrentInstructions[IP++];
                switch ((InstructionType)opCode)
                {
                    case InstructionType.NOP:
                        break;
                    case InstructionType.PUSHB:
                        Stack.PushInt(ConsumeByte());
                        break;
                    case InstructionType.PUSHI:
                        Stack.PushInt(ConsumeInt());
                        break;
                    case InstructionType.PUSHD:
                        Stack.PushDouble(ConsumeDouble());
                        break;
                    case InstructionType.PUSHA:
                        Stack.PushAddress(ConsumeUint());
                        break;
                    case InstructionType.POPB:
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
                    case InstructionType.LOCAL:
                        Stack.PushAddress((uint)(Stack.FP + ConsumeInt()));
                        break;
                    case InstructionType.CONST:
                        Stack.PushAddress(StringConstants[ConsumeInt() - 1]);
                        break;
                    case InstructionType.STATIC:
                        index = ConsumeInt();
                        Stack.PushAddress(CurrentModule.ClassPoolLink[CurrentModule.FieldPool[index - 1].Class - 1].StaticFieldAddress);
                        Stack.PushInt(CurrentModule.FieldPoolLink[index - 1]);
                        break;
                    case InstructionType.NONSTATIC:
                    case InstructionType.NEW:
                        throw new NotImplementedException();
                    case InstructionType.LOADB:
                    case InstructionType.LOADI:
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.PushInt(Stack.GetInt(addr));
                                break;
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushInt(BitConverter.ToInt32(data));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADD:
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.PushDouble(Stack.GetDouble(addr));
                                break;
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushDouble(BitConverter.ToDouble(data));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADA:
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.PushAddress(Stack.GetAddress(addr));
                                break;
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushAddress(BitConverter.ToUInt32(data));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STOREB:
                    case InstructionType.STOREI:
                        addr = Stack.PopAddress();
                        iValue = Stack.TopInt;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.SetValue(addr, iValue);
                                break;
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                BitConverter.TryWriteBytes(data, iValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STORED:
                        addr = Stack.PopAddress();
                        dValue = Stack.TopDouble;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.SetValue(addr, dValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STOREA:
                        addr = Stack.PopAddress();
                        uValue = Stack.TopAddress;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                Stack.SetValue(addr, uValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ALOADB:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushInt(data[index]);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ALOADI:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushInt(BitConverter.ToInt32(data, index));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ALOADD:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushDouble(BitConverter.ToDouble(data, index));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ALOADA:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                Stack.PushAddress(BitConverter.ToUInt32(data, index));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ASTOREB:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        iValue = Stack.TopInt;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                data[index] = (byte)iValue;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ASTOREI:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        iValue = Stack.TopInt;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, index, sizeof(int)), iValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ASTORED:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        dValue = Stack.TopDouble;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, index, sizeof(double)), dValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.ASTOREA:
                        index = Stack.PopInt();
                        addr = Stack.PopAddress();
                        uValue = Stack.TopAddress;
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, index, sizeof(uint)), uValue);
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
                        Stack.PushDouble(iValue);
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
                        Stack.PushInt((InstructionType)opCode switch
                        {
                            InstructionType.SETEQI => lhsi == rhsi ? 1 : 0,
                            InstructionType.SETNEI => lhsi != rhsi ? 1 : 0,
                            InstructionType.SETLTI => lhsi < rhsi ? 1 : 0,
                            InstructionType.SETLEI => lhsi <= rhsi ? 1 : 0,
                            InstructionType.SETGTI => lhsi > rhsi ? 1 : 0,
                            InstructionType.SETGEI => lhsi >= rhsi ? 1 : 0,
                            _ => throw new NotImplementedException(),
                        });
                        break;
                    case InstructionType.JMP:
                        iValue = ConsumeInt();
                        IP += iValue;
                        break;
                    case InstructionType.JCOND:
                        iValue = Stack.PopInt();
                        if (iValue != 0)
                        {   // if
                            iValue = ConsumeInt();
                            IP += sizeof(int);
                        }
                        else
                        {   // else
                            IP += sizeof(int);
                            iValue = ConsumeInt();
                        }
                        IP += iValue;
                        break;
                    case InstructionType.CALL:
                        index = ConsumeInt();

                        Stack.PushFrame(MethodIndex, IP);

                        CurrentMethod = CurrentModule.MethodPoolLink[index - 1];

                        IP = 0;
                        PushLocals();
                        break;
                    case InstructionType.RET:
                        MethodReturn(out index, out ip);
                        CurrentMethod = MethodArea.MethodIndexTable[index];
                        IP = ip;
                        break;
                    case InstructionType.PUTC:
                        iValue = Stack.PopInt();
                        Console.Write((char)iValue);
                        break;
                    case InstructionType.PUTS:
                        // 这个地址应该指向一个StringType
                        addr = Stack.PopAddress();
                        switch (MemoryMap.MapToOffset(addr, out addr))
                        {
                            case MemoryTag.STACK:
                                throw new XiVMError("String should located on the heap");
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr, out uValue);
                                if (uValue != 0)
                                {
                                    throw new XiVMError("Address of PUTS should point to the head of a string");
                                }
                                // TODO 检查MiscData是不是StringType
                                Console.Write(Encoding.UTF8.GetString(data, Heap.MiscDataSize, data.Length - Heap.MiscDataSize));
                                break;
                            case MemoryTag.METHOD:
                                data = MethodArea.GetData(addr);
                                // TODO 检查MiscData是不是StringType
                                Console.Write(Encoding.UTF8.GetString(data, MethodArea.StringMiscDataSize, data.Length - MethodArea.StringMiscDataSize));
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
            foreach (uint descriptorAddress in CurrentMethod.LocalDescriptorAddress)
            {
                switch (MemoryMap.MapToOffset(descriptorAddress, out uint addr))
                {
                    case MemoryTag.METHOD:
                        break;
                    default:
                        throw new XiVMError("Descriptor should be in method area");
                }


                byte[] descriptorData = MethodArea.GetData(addr);
                string descriptor = Encoding.UTF8.GetString(descriptorData,
                    MethodArea.StringMiscDataSize,
                    descriptorData.Length - MethodArea.StringMiscDataSize);

                switch (descriptor)
                {
                    case "B":
                    case "I":
                        Stack.PushInt();
                        break;
                    case "D":
                        Stack.PushDouble();
                        break;
                    case "L":
                        Stack.PushAddress();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void PopParams(string paramsDescriptor)
        {
            for (int i = paramsDescriptor.Length - 1; i >= 0; --i)
            {
                // Warning Hardcoding
                switch (paramsDescriptor[i])
                {
                    case 'B':
                    case 'I':
                        Stack.PopInt();
                        break;
                    case 'D':
                        Stack.PopDouble();
                        break;
                    case 'L':
                        Stack.PopAddress();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void MethodReturn(out int index, out int ip)
        {
            switch (MemoryMap.MapToOffset(CurrentMethod.DescriptorAddress, out uint addr))
            {
                case MemoryTag.METHOD:
                    break;
                default:
                    throw new XiVMError("Descriptor should be in method area");
            }

            byte[] descriptorData = MethodArea.GetData(addr);
            string descriptor = Encoding.UTF8.GetString(descriptorData,
                MethodArea.StringMiscDataSize,
                descriptorData.Length - MethodArea.StringMiscDataSize).Substring(1);
            string[] vs = descriptor.Split(')');

            switch (vs[1][0])
            {
                case 'B':
                case 'I':
                    int iValue = Stack.PopInt();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0]);
                    Stack.PushInt(iValue);
                    break;
                case 'D':
                    double dValue = Stack.PopDouble();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0]);
                    Stack.PushDouble(dValue);
                    break;
                case 'L':
                    uint aValue = Stack.PopAddress();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0]);
                    Stack.PushAddress(aValue);
                    break;
                case 'V':
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0]);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private uint ConsumeUint()
        {
            IP += sizeof(uint);
            return BitConverter.ToUInt32(CurrentInstructions, IP - sizeof(uint));
        }

        private int ConsumeInt()
        {
            IP += sizeof(int);
            return BitConverter.ToInt32(CurrentInstructions, IP - sizeof(int));
        }

        private double ConsumeDouble()
        {
            IP += sizeof(double);
            return BitConverter.ToDouble(CurrentInstructions, IP - sizeof(double));
        }

        private byte ConsumeByte()
        {
            IP += 1;
            return CurrentInstructions[IP - 1];
        }
    }
}
