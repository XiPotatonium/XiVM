using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using XiVM.Errors;

namespace XiVM.Runtime
{
    public class VMExecutor
    {
        private int IP { set; get; }                        // 指令在函数中的IP
        private VMMethod CurrentMethod { set; get; }
        private int MethodIndex => CurrentMethod.MethodIndex;
        private byte[] CurrentInstructions => CurrentMethod.CodeBlock.Data;
        private VMClass CurrentClass => CurrentMethod.Parent;
        private VMModule CurrentModule => CurrentClass.Parent;
        private List<uint> StringConstants => CurrentModule.StringPoolLink;

        private Stack Stack { get; } = new Stack();

        private VMModule MainModule { set; get; }

        internal VMExecutor(VMModule module)
        {
            MainModule = module;
        }

        public void ExecuteStaticConstructor()
        {
            foreach (var vmModule in MethodArea.Modules.Values)
            {
                foreach (var vmClass in vmModule.Classes.Values)
                {
                    if (!vmClass.IsStaticConstructorExecuted)
                    {
                        // 最多执行一遍
                        vmClass.IsStaticConstructorExecuted = true;
                        vmClass.Methods.TryGetValue(MethodArea.StaticConstructorNameAddress, out List<VMMethod> sinit);
                        CurrentMethod = sinit[0];
                        Execute();
                    }
                }
            }
            CurrentMethod = null;
        }

        public void ExecuteMain()
        {
            // 定位Main函数
            if (!MainModule.Classes.TryGetValue(MethodArea.StringProgramAddress, out VMClass entryClass))
            {
                throw new XiVMError("Program.Main() not found");
            }

            if (!entryClass.Methods.TryGetValue(MethodArea.StringMainAddress, out List<VMMethod> entryMethodGroup))
            {
                throw new XiVMError("Program.Main() not found");
            }


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
                throw new XiVMError("Program.Main(System.String) not found");
            }

            // TODO Main的参数，暂时push了一个null
            Stack.PushAddress();
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
            VMField vmField;

            while (!Stack.Empty)
            {
                byte opCode = CurrentInstructions[IP++];
                switch ((InstructionType)opCode)
                {
                    case InstructionType.NOP:
                        break;
                    case InstructionType.DUP:
                        Stack.DupN(1);
                        break;
                    case InstructionType.DUP2:
                        Stack.DupN(2);
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                Stack.PushInt(data[HeapData.ArrayOffsetMap(sizeof(byte), index)]);
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                Stack.PushInt(BitConverter.ToInt32(data, HeapData.ArrayOffsetMap(sizeof(int), index)));
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                Stack.PushDouble(BitConverter.ToDouble(data, HeapData.ArrayOffsetMap(sizeof(double), index)));
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                Stack.PushAddress(BitConverter.ToUInt32(data, HeapData.ArrayOffsetMap(sizeof(uint), index)));
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                data[HeapData.ArrayOffsetMap(sizeof(byte), index)] = (byte)iValue;
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, HeapData.ArrayOffsetMap(sizeof(int), index), sizeof(int)), iValue);
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, HeapData.ArrayOffsetMap(sizeof(double), index), sizeof(double)), dValue);
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
                            case MemoryTag.HEAP:
                                data = Heap.GetData(addr);
                                BitConverter.TryWriteBytes(new Span<byte>(data, HeapData.ArrayOffsetMap(sizeof(uint), index), sizeof(uint)), uValue);
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
                        IP = iValue;
                        break;
                    case InstructionType.JCOND:
                        iValue = Stack.PopInt();
                        if (iValue != 0)
                        {   // if
                            IP = ConsumeInt();
                        }
                        else
                        {   // else
                            IP += sizeof(int);
                            IP = ConsumeInt();
                        }
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
                    case InstructionType.LOCAL:
                        iValue = ConsumeInt();
                        Stack.PushAddress(MemoryMap.MapToAbsolute((uint)(Stack.FP + iValue), MemoryTag.STACK));
                        break;
                    case InstructionType.CONST:
                        Stack.PushAddress(StringConstants[ConsumeInt() - 1]);
                        break;
                    case InstructionType.STORESTATIC:
                        index = ConsumeInt();
                        vmField = CurrentModule.FieldPoolLink[index - 1];
                        addr = CurrentModule.ClassPoolLink[vmField.ClassIndex - 1].StaticFieldAddress;
                        data = (MemoryMap.MapToOffset(addr, out addr)) switch
                        {
                            MemoryTag.METHOD => MethodArea.GetData(addr),
                            _ => throw new NotImplementedException(),
                        };
                        switch (vmField.Type.Tag)
                        {
                            case VariableTypeTag.BYTE:
                                data[vmField.Offset] = (byte)Stack.TopInt;
                                break;
                            case VariableTypeTag.INT:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopInt);
                                break;
                            case VariableTypeTag.DOUBLE:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopDouble);
                                break;
                            case VariableTypeTag.ADDRESS:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopAddress);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADSTATIC:
                        index = ConsumeInt();
                        vmField = CurrentModule.FieldPoolLink[index - 1];
                        addr = CurrentModule.ClassPoolLink[vmField.ClassIndex - 1].StaticFieldAddress;
                        data = (MemoryMap.MapToOffset(addr, out addr)) switch
                        {
                            MemoryTag.METHOD => MethodArea.GetData(addr),
                            _ => throw new NotImplementedException(),
                        };
                        switch (vmField.Type.Tag)
                        {
                            case VariableTypeTag.BYTE:
                                Stack.PushInt(data[vmField.Offset]);
                                break;
                            case VariableTypeTag.INT:
                                Stack.PushInt(BitConverter.ToInt32(data, vmField.Offset));
                                break;
                            case VariableTypeTag.DOUBLE:
                                Stack.PushDouble(BitConverter.ToDouble(data, vmField.Offset));
                                break;
                            case VariableTypeTag.ADDRESS:
                                Stack.PushAddress(BitConverter.ToUInt32(data, vmField.Offset));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.STORENONSTATIC:
                        index = ConsumeInt();
                        vmField = CurrentModule.FieldPoolLink[index - 1];
                        addr = Stack.PopAddress();
                        data = (MemoryMap.MapToOffset(addr, out addr)) switch
                        {
                            MemoryTag.HEAP => Heap.GetData(addr),
                            _ => throw new NotImplementedException(),
                        };
                        switch (vmField.Type.Tag)
                        {
                            case VariableTypeTag.BYTE:
                                data[vmField.Offset] = (byte)Stack.TopInt;
                                break;
                            case VariableTypeTag.INT:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopInt);
                                break;
                            case VariableTypeTag.DOUBLE:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopDouble);
                                break;
                            case VariableTypeTag.ADDRESS:
                                BitConverter.TryWriteBytes(new Span<byte>(data, vmField.Offset, vmField.Type.Size),
                                    Stack.TopAddress);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.LOADNONSTATIC:
                        index = ConsumeInt();
                        vmField = CurrentModule.FieldPoolLink[index - 1];
                        addr = Stack.PopAddress();
                        data = (MemoryMap.MapToOffset(addr, out addr)) switch
                        {
                            MemoryTag.HEAP => Heap.GetData(addr),
                            MemoryTag.METHOD => MethodArea.GetData(addr),       // 常量池中的String是在方法区里面的
                            _ => throw new NotImplementedException(),
                        };
                        switch (vmField.Type.Tag)
                        {
                            case VariableTypeTag.BYTE:
                                Stack.PushInt(data[vmField.Offset]);
                                break;
                            case VariableTypeTag.INT:
                                Stack.PushInt(BitConverter.ToInt32(data, vmField.Offset));
                                break;
                            case VariableTypeTag.DOUBLE:
                                Stack.PushDouble(BitConverter.ToDouble(data, vmField.Offset));
                                break;
                            case VariableTypeTag.ADDRESS:
                                Stack.PushAddress(BitConverter.ToUInt32(data, vmField.Offset));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case InstructionType.NEW:
                        iValue = ConsumeInt();
                        Stack.PushAddress(MemoryMap.MapToAbsolute(
                            Heap.Malloc(CurrentModule.ClassPoolLink[iValue - 1].FieldSize).Offset, MemoryTag.HEAP));
                        break;
                    case InstructionType.NEWARR:
                        iValue = Stack.PopInt();
                        Stack.PushAddress(MemoryMap.MapToAbsolute(
                            Heap.MallocArray(VariableType.GetSize((VariableTypeTag)ConsumeByte()), iValue).Offset, MemoryTag.HEAP));
                        break;
                    case InstructionType.NEWAARR:
                        iValue = Stack.PopInt();
                        index = ConsumeInt();   // Warning 未使用的信息，Array类型信息
                        Stack.PushAddress(MemoryMap.MapToAbsolute(
                            Heap.MallocArray(VariableType.AddressType.Size, iValue).Offset, MemoryTag.HEAP));
                        break;
                    case InstructionType.LEN:
                        throw new NotImplementedException();
                    case InstructionType.PUTC:
                        iValue = Stack.PopInt();
                        Console.Write((char)iValue);
                        break;
                    case InstructionType.PUTI:
                        iValue = Stack.PopInt();
                        Console.Write(iValue);
                        break;
                    case InstructionType.PUTS:
                        // 这个地址应该指向一个StringType
                        addr = Stack.PopAddress();
                        Console.Write(GetString(addr));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }


        private string GetString(uint addr)
        {
            byte[] data;
            switch (MemoryMap.MapToOffset(addr, out addr))
            {
                case MemoryTag.HEAP:
                    data = Heap.GetData(addr);
                    break;
                case MemoryTag.METHOD:
                    data = MethodArea.GetData(addr);
                    break;
                default:
                    throw new XiVMError("String not in method area nor heap");
            }

            // data的地址
            addr = BitConverter.ToUInt32(data, HeapData.MiscDataSize + HeapData.StringLengthSize);
            switch (MemoryMap.MapToOffset(addr, out addr))
            {
                case MemoryTag.HEAP:
                    data = Heap.GetData(addr);
                    break;
                case MemoryTag.METHOD:
                    data = MethodArea.GetData(addr);
                    break;
                default:
                    throw new XiVMError("Array not in method area nor heap");
            }

            return Encoding.UTF8.GetString(data, HeapData.MiscDataSize + HeapData.ArrayLengthSize,
                        data.Length - HeapData.MiscDataSize - HeapData.ArrayLengthSize);
        }

        private void PushLocals()
        {
            foreach (uint descriptorAddress in CurrentMethod.LocalDescriptorAddress)
            {
                string descriptor = GetString(descriptorAddress);

                switch (descriptor[0])
                {
                    case 'B':
                    case 'I':
                        Stack.PushInt();
                        break;
                    case 'D':
                        Stack.PushDouble();
                        break;
                    case 'L':
                        Stack.PushAddress();
                        break;
                    case '[':
                        Stack.PushAddress();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void PopParams(string paramsDescriptor, bool isStatic)
        {
            // 后来的参数在栈顶
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
                    case ';':
                        Stack.PopAddress();
                        while (!(paramsDescriptor[i] == 'L' &&
                            (i == 0 || paramsDescriptor[i - 1] == 'B' || paramsDescriptor[i - 1] == 'I' ||
                                paramsDescriptor[i - 1] == 'D' || paramsDescriptor[i - 1] == ';' ||
                                paramsDescriptor[i - 1] == '[')))
                        {
                            --i;
                        }
                        while (i != 0 && paramsDescriptor[i - 1] == '[')
                        {
                            --i;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (!isStatic)
            {
                // non-static method，多pop一个this
                Stack.PopAddress();
            }
        }

        private void MethodReturn(out int index, out int ip)
        {
            string descriptor = GetString(CurrentMethod.DescriptorAddress).Substring(1);
            string[] vs = descriptor.Split(')');

            switch (vs[1][0])
            {
                case 'B':
                case 'I':
                    int iValue = Stack.PopInt();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0], CurrentMethod.Flag.IsStatic);
                    Stack.PushInt(iValue);
                    break;
                case 'D':
                    double dValue = Stack.PopDouble();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0], CurrentMethod.Flag.IsStatic);
                    Stack.PushDouble(dValue);
                    break;
                case 'L':
                    uint aValue = Stack.PopAddress();
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0], CurrentMethod.Flag.IsStatic);
                    Stack.PushAddress(aValue);
                    break;
                case 'V':
                    Stack.PopFrame(out index, out ip);
                    PopParams(vs[0], CurrentMethod.Flag.IsStatic);
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


        public ExecutorDiagnoseInfo GetDiagnoseInfo()
        {
            return new ExecutorDiagnoseInfo()
            {
                MaxSP = Stack.MaxSP,
                CurrentSP = Stack.SP
            };
        }
    }

    public class ExecutorDiagnoseInfo
    {
        public int MaxSP { set; get; }
        public int CurrentSP { set; get; }
    }
}
