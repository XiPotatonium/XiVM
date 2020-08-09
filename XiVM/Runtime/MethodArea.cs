using System;
using System.Collections.Generic;
using System.Text;
using XiVM.Errors;

namespace XiVM.Runtime
{
    internal static class MethodArea
    {
        public static readonly int MaxSize = 0x1000000;
        public static int StringMiscDataSize => Heap.MiscDataSize;



        private static Dictionary<uint, VMModule> Modules { get; } = new Dictionary<uint, VMModule>();
        private static LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();


        /// <summary>
        /// 支持1k个函数
        /// </summary>
        public static VMMethod[] MethodIndexTable { private set; get; } = new VMMethod[0x400];

        public static Dictionary<string, LinkedListNode<HeapData>> StringPool { get; } = new Dictionary<string, LinkedListNode<HeapData>>();
        public static uint StringProgramAddress { private set; get; }
        public static uint StringMainAddress { private set; get; }
        public static uint StringMainDescriptorAddress { private set; get; }


        static MethodArea()
        {
            // 预先加入这两个常量，因为执行器会执行void Program.Main()，免得再找一遍
            StringProgramAddress = TryAddConstantString("Program");
            StringMainAddress = TryAddConstantString("Main");
            StringMainDescriptorAddress = TryAddConstantString("()V");
        }

        private static uint TryAddConstantString(string value)
        {
            if (!StringPool.TryGetValue(value, out LinkedListNode<HeapData> data))
            {
                byte[] vs = new byte[StringMiscDataSize + Encoding.UTF8.GetByteCount(value)];
                Encoding.UTF8.GetBytes(value, new Span<byte>(vs, StringMiscDataSize, vs.Length - StringMiscDataSize));
                data = new LinkedListNode<HeapData>(new HeapData(
                    Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                    vs));
                Data.AddLast(data);
                StringPool.Add(value, data);
            }
            return MemoryMap.MapTo(data.Value.Offset, MemoryTag.METHOD);
        }


        public static VMModule AddModule(BinaryModule binaryModule)
        {
            VMModule module = new VMModule()
            {
                StringPool = new List<uint>(),
                Classes = new Dictionary<uint, VMClass>(),
                ClassConstantInfos = binaryModule.ClassConstantInfos,
                MemberConstantInfos = binaryModule.MemberConstantInfos
            };

            // 字符串常量
            foreach (string stringConstant in binaryModule.StringPool)
            {
                // 建立映射
                module.StringPool.Add(TryAddConstantString(stringConstant));
            }

            int i = 0;
            foreach (BinaryClassType binaryClass in binaryModule.Classes)
            {
                VMClass vmClass = new VMClass()
                {
                    Parent = module,
                    Methods = new Dictionary<uint, List<VMMethod>>()
                };

                foreach (BinaryMethod binaryMethod in binaryClass.Methods)
                {
                    // 寻找方法索引空位
                    for (; i < MethodIndexTable.Length && MethodIndexTable[i] != null; ++i) ;
                    if (i == MethodIndexTable.Length)
                    {
                        throw new XiVMError("Code segment overflow");
                    }

                    // 构造VMMethod
                    VMMethod vmMethod = new VMMethod()
                    {
                        Parent = vmClass,
                        MethodIndex = i,
                        DescriptorAddress = module.StringPool[module.MemberConstantInfos
                            [binaryMethod.ConstantPoolIndex - 1].Type - 1],
                        LocalDescriptorAddress = binaryMethod.LocalDescriptorIndex == 0 ?
                            MemoryMap.NullAddress : module.StringPool[binaryMethod.LocalDescriptorIndex - 1],
                        CodeBlock = Data.AddLast(new HeapData(
                            Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                            binaryMethod.Instructions))
                    };

                    MethodIndexTable[i] = vmMethod;

                    // 将VMMethod添加到Class中
                    uint methodNameAddr = module.StringPool[
                        module.MemberConstantInfos[binaryMethod.ConstantPoolIndex - 1].Name - 1];
                    if (vmClass.Methods.TryGetValue(methodNameAddr, out List<VMMethod> methodGroup))
                    {
                        methodGroup.Add(vmMethod);
                    }
                    else
                    {
                        vmClass.Methods.Add(methodNameAddr, new List<VMMethod>() { vmMethod });
                    }
                }
                module.Classes.Add(module.StringPool[module.ClassConstantInfos
                        [binaryClass.ConstantPoolIndex - 1].Name - 1], vmClass);
            }

            Modules.Add(module.StringPool[binaryModule.ModuleNameIndex - 1], module);

            return module;
        }

        public static bool TryGetMethod(uint moduleName, uint className, uint name, uint descriptor, out VMMethod method)
        {
            if (Modules.TryGetValue(moduleName, out VMModule module))
            {
                if (module.Classes.TryGetValue(className, out VMClass vmClass))
                {
                    if (vmClass.Methods.TryGetValue(name, out List<VMMethod> candidateMethods))
                    {
                        foreach (VMMethod candidate in candidateMethods)
                        {
                            if (candidate.DescriptorAddress == descriptor)
                            {
                                method = candidate;
                                return true;
                            }
                        }
                    }
                }
            }
            method = null;
            return false;
        }

        public static byte[] GetData(uint addr, out uint offset)
        {
            LinkedListNode<HeapData> cur = Data.First;
            while (cur != null)
            {
                if (addr < cur.Value.Offset)
                {
                    break;
                }
                else if (addr < cur.Value.Offset + cur.Value.Data.Length)
                {
                    offset = addr - cur.Value.Offset;
                    return cur.Value.Data;
                }
                cur = cur.Next;
            }
            offset = 0;
            return null;
        }
    }
}
