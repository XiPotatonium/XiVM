using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiVM.ConstantTable;
using XiVM.Errors;

namespace XiVM.Runtime
{
    internal static class MethodArea
    {
        public static readonly int MaxSize = 0x1000000;
        public static int StringMiscDataSize => Heap.MiscDataSize;



        private static Dictionary<uint, VMModule> Modules { get; } = new Dictionary<uint, VMModule>();
        private static LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();
        private static int Size { set; get; }


        /// <summary>
        /// 支持1k个函数
        /// </summary>
        public static VMMethod[] MethodIndexTable { private set; get; } = new VMMethod[0x400];

        public static Dictionary<string, LinkedListNode<HeapData>> StringPool { get; } = new Dictionary<string, LinkedListNode<HeapData>>();
        public static uint StringProgramAddress { private set; get; }
        public static uint StringMainAddress { private set; get; }
        public static uint StringMainDescriptorAddress { private set; get; }
        public static uint StaticConstructorNameAddress { private set; get; }
        public static uint ConstructorNameAddress { private set; get; }


        static MethodArea()
        {
            // 预先加入这些常量
            // Warning Hardcoding
            StringProgramAddress = TryAddConstantString("Program");
            StringMainAddress = TryAddConstantString("Main");
            StringMainDescriptorAddress = TryAddConstantString("()V");
            StaticConstructorNameAddress = TryAddConstantString("(sinit)");
            ConstructorNameAddress = TryAddConstantString("(init)");
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
            return MemoryMap.MapToAbsolute(data.Value.Offset, MemoryTag.METHOD);
        }


        public static VMModule AddModule(BinaryModule binaryModule)
        {
            VMModule module = new VMModule()
            {
                StringPoolLink = new List<uint>(),
                Classes = new Dictionary<uint, VMClass>(),
                ClassPool = binaryModule.ClassPool,
                ClassPoolLink = new List<VMClass>(),
                FieldPool = binaryModule.FieldPool,
                FieldPoolLink = new List<int>(),
                MethodPool = binaryModule.MethodPool,
                MethodPoolLink = new List<VMMethod>()
            };
            HashSet<int> externalModuleNameIndexes = new HashSet<int>();

            // 字符串常量
            foreach (string stringConstant in binaryModule.StringPool)
            {
                // 建立映射
                module.StringPoolLink.Add(TryAddConstantString(stringConstant));
            }
            Modules.Add(module.StringPoolLink[binaryModule.ModuleNameIndex - 1], module);

            // 类
            foreach (ClassConstantInfo classInfo in module.ClassPool)
            {
                int moduleNameIndex = classInfo.Module;
                if (moduleNameIndex != binaryModule.ModuleNameIndex)
                {
                    // 外部域
                    externalModuleNameIndexes.Add(moduleNameIndex);
                    // 占位
                    module.ClassPoolLink.Add(null);
                }
                else
                {
                    VMClass vmClass = new VMClass()
                    {
                        Parent = module,
                        Methods = new Dictionary<uint, List<VMMethod>>(),
                        StaticFields = new List<VMClassField>(),
                        StaticFieldSize = 0,
                        Fields = new List<VMClassField>(),
                        FieldSize = 0
                    };
                    module.Classes.Add(module.StringPoolLink[classInfo.Name - 1], vmClass);
                    module.ClassPoolLink.Add(vmClass);
                }
            }

            // Field
            AccessFlag accessFlag = new AccessFlag();
            foreach (FieldConstantInfo fieldInfo in module.FieldPool)
            {
                int moduleNameIndex = module.ClassPool[fieldInfo.Class - 1].Module;
                if (moduleNameIndex != binaryModule.ModuleNameIndex)
                {
                    // 外部域
                    externalModuleNameIndexes.Add(moduleNameIndex);
                    // 占位
                    module.FieldPoolLink.Add(0);
                }
                else
                {
                    module.Classes.TryGetValue(module.StringPoolLink[module.ClassPool[fieldInfo.Class - 1].Name - 1],
                        out VMClass vmClass);
                    // 分配方法区空间并且链接地址
                    accessFlag.Flag = fieldInfo.Flag;
                    VariableType fieldType = VariableType.GetType(binaryModule.StringPool[fieldInfo.Type - 1]);
                    if (accessFlag.IsStatic)
                    {
                        module.FieldPoolLink.Add(vmClass.StaticFieldSize);
                        vmClass.StaticFields.Add(new VMClassField(fieldInfo.Flag, fieldType, vmClass.StaticFieldSize));
                        vmClass.StaticFieldSize += fieldType.Size;
                    }
                    else
                    {
                        // TODO 记录非静态field，便于生成对象
                    }
                }
            }

            // 完成静态空间分配
            foreach (var vmClass in module.ClassPoolLink)
            {
                if (vmClass != null)
                {
                    vmClass.StaticFieldAddress = MemoryMap.MapToAbsolute(Malloc(vmClass.StaticFieldSize), MemoryTag.METHOD);
                }
            }

            // Method
            int methodIndex = 0;
            foreach ((MethodConstantInfo methodInfo, BinaryMethod binaryMethod) in module.MethodPool.Zip(binaryModule.Code))
            {
                int moduleNameIndex = module.ClassPool[methodInfo.Class - 1].Module;
                if (moduleNameIndex != binaryModule.ModuleNameIndex)
                {
                    // 外部方法
                    externalModuleNameIndexes.Add(moduleNameIndex);
                    // 占位
                    module.MethodPoolLink.Add(null);
                }
                else
                {
                    module.Classes.TryGetValue(module.StringPoolLink[module.ClassPool[methodInfo.Class - 1].Name - 1],
                        out VMClass vmClass);
                    // 寻找方法索引空位
                    for (; methodIndex < MethodIndexTable.Length && MethodIndexTable[methodIndex] != null; ++methodIndex) ;
                    if (methodIndex == MethodIndexTable.Length)
                    {
                        throw new XiVMError("Code segment overflow");
                    }

                    // 构造VMMethod
                    VMMethod vmMethod = new VMMethod()
                    {
                        Parent = vmClass,
                        MethodIndex = methodIndex,
                        DescriptorAddress = module.StringPoolLink[methodInfo.Type - 1],
                        LocalDescriptorAddress = binaryMethod.LocalDescriptorIndex.Select(i => module.StringPoolLink[i - 1]).ToList(),
                        CodeBlock = Malloc(binaryMethod.Instructions)
                    };

                    MethodIndexTable[methodIndex] = vmMethod;

                    // 将VMMethod添加到Class中
                    uint methodNameAddr = module.StringPoolLink[methodInfo.Name - 1];
                    if (vmClass.Methods.TryGetValue(methodNameAddr, out List<VMMethod> methodGroup))
                    {
                        methodGroup.Add(vmMethod);
                    }
                    else
                    {
                        vmClass.Methods.Add(methodNameAddr, new List<VMMethod>() { vmMethod });
                    }

                    // 建立Link
                    module.MethodPoolLink.Add(vmMethod);
                }
            }

            // 导入外部模块
            foreach (int externalModuleNameIndex in externalModuleNameIndexes)
            {
                if (!Modules.ContainsKey(module.StringPoolLink[externalModuleNameIndex - 1]))
                {
                    // 导入未导入的模块，图的广度优先遍历
                    AddModule(Program.LoadModule(binaryModule.StringPool[externalModuleNameIndex - 1]));
                }
            }

            // 链接外部符号
            ExternalSymbolResolution(module);

            return module;
        }

        private static void ExternalSymbolResolution(VMModule module)
        {
            // 外部函数符号
            for (int i = 0; i < module.MethodPool.Length; ++i)
            {
                MethodConstantInfo methodInfo = module.MethodPool[i];
                uint moduleNameAddress = module.StringPoolLink[module.ClassPool[methodInfo.Class - 1].Module - 1];
                Modules.TryGetValue(moduleNameAddress, out VMModule importedModule);

                uint classNameAddress = module.StringPoolLink[module.ClassPool[methodInfo.Class - 1].Name - 1];
                uint descriptorAddress = module.StringPoolLink[methodInfo.Type - 1];
                foreach ((MethodConstantInfo candidateMethodInfo, VMMethod vmMethod) in importedModule.MethodPool.Zip(importedModule.MethodPoolLink))
                {
                    // 模块名类名描述符匹配
                    if (moduleNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateMethodInfo.Class - 1].Module - 1] &&
                        classNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateMethodInfo.Class - 1].Name - 1] &&
                        descriptorAddress == importedModule.StringPoolLink[candidateMethodInfo.Type - 1])
                    {
                        // 建立Link
                        module.MethodPoolLink[i] = vmMethod;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// TODO 考虑用哈希表，因为不会有offset
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static byte[] GetData(uint addr)
        {
            LinkedListNode<HeapData> cur = Data.First;
            while (cur != null)
            {
                if (addr < cur.Value.Offset)
                {
                    break;
                }
                else if (addr == cur.Value.Offset)
                {
                    return cur.Value.Data;
                }
                cur = cur.Next;
            }
            return null;
        }

        public static uint Malloc(int size)
        {
            if (Size + size > MaxSize)
            {
                throw new XiVMError("Heap overflow");
            }
            LinkedListNode<HeapData> newData = new LinkedListNode<HeapData>(new HeapData(
                Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                new byte[size]));
            Data.AddLast(newData);
            return newData.Value.Offset;
        }

        public static LinkedListNode<HeapData> Malloc(byte[] data)
        {
            if (Size + data.Length > MaxSize)
            {
                throw new XiVMError("Heap overflow");
            }
            return Data.AddLast(new HeapData(
                Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                data));
        }
    }
}
