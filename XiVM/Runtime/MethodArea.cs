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
        public static readonly int SizeLimit = 0x1000000;



        public static Dictionary<uint, VMModule> Modules { get; } = new Dictionary<uint, VMModule>();
        /// <summary>
        /// 当前占用，由于不回收，当前占用就是历史最高占用
        /// </summary>
        private static int Size { set; get; }
        /// <summary>
        /// key是offset，因为设定上方法区对象不回收，所以不用记录内碎片
        /// </summary>
        private static Dictionary<uint, HeapData> DataMap { get; } = new Dictionary<uint, HeapData>();


        /// <summary>
        /// 支持1k个函数
        /// </summary>
        public static VMMethod[] MethodIndexTable { private set; get; } = new VMMethod[0x400];

        public static Dictionary<string, HeapData> StringPool { get; } = new Dictionary<string, HeapData>();
        public static uint StringProgramAddress { private set; get; }
        public static uint StringMainAddress { private set; get; }
        public static uint StringMainDescriptorAddress { private set; get; }
        public static uint StaticConstructorNameAddress { private set; get; }
        public static uint ConstructorNameAddress { private set; get; }


        static MethodArea()
        {
            // 预先加入这些常量
            // Warning Hardcoding
            StringProgramAddress = AddConstantString("Program");
            StringMainAddress = AddConstantString("Main");
            StringMainDescriptorAddress = AddConstantString("([LSystem.String;)V");
            StaticConstructorNameAddress = AddConstantString("(sinit)");
            ConstructorNameAddress = AddConstantString("(init)");
        }

        private static uint AddConstantString(string value)
        {
            if (!StringPool.TryGetValue(value, out HeapData data))
            {
                // 分配byte数组
                HeapData stringData = MallocArray(sizeof(byte), Encoding.UTF8.GetByteCount(value));
                Encoding.UTF8.GetBytes(value, new Span<byte>(stringData.Data, HeapData.ArrayLengthSize + HeapData.MiscDataSize,
                    stringData.Data.Length - HeapData.ArrayLengthSize - HeapData.MiscDataSize));

                // String对象
                byte[] vs = new byte[HeapData.StringLengthSize + HeapData.MiscDataSize + HeapData.StringDataSize];
                // TODO 头部信息
                // 长度信息
                BitConverter.TryWriteBytes(new Span<byte>(vs, HeapData.MiscDataSize, HeapData.StringLengthSize), value.Length);
                // Data信息
                BitConverter.TryWriteBytes(new Span<byte>(vs, HeapData.MiscDataSize + HeapData.StringLengthSize, HeapData.StringDataSize),
                    MemoryMap.MapToAbsolute(stringData.Offset, MemoryTag.METHOD));

                // 字符串
                data = Malloc(vs);
                StringPool.Add(value, data);
            }
            return MemoryMap.MapToAbsolute(data.Offset, MemoryTag.METHOD);
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
                FieldPoolLink = new List<VMField>(),
                MethodPool = binaryModule.MethodPool,
                MethodPoolLink = new List<VMMethod>()
            };
            HashSet<int> externalModuleNameIndexes = new HashSet<int>();

            // 字符串常量
            foreach (string stringConstant in binaryModule.StringPool)
            {
                // 建立映射
                module.StringPoolLink.Add(AddConstantString(stringConstant));
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
                        StaticFields = new List<VMField>(),
                        StaticFieldSize = sizeof(uint),         // 注意这个uint其实没有任何信息，但是不能Malloc 大小为0的空间
                        Fields = new List<VMField>(),
                        FieldSize = HeapData.MiscDataSize       // 头部信息
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
                    module.FieldPoolLink.Add(null);
                }
                else
                {
                    module.Classes.TryGetValue(module.StringPoolLink[module.ClassPool[fieldInfo.Class - 1].Name - 1],
                        out VMClass vmClass);
                    // 分配方法区空间并且链接地址
                    accessFlag.Flag = fieldInfo.Flag;
                    VariableType fieldType = VariableType.GetType(binaryModule.StringPool[fieldInfo.Descriptor - 1]);
                    VMField vmField;
                    if (accessFlag.IsStatic)
                    {
                        vmField = new VMField(fieldInfo.Flag, fieldType, fieldInfo.Class, vmClass.StaticFieldSize);
                        module.FieldPoolLink.Add(vmField);
                        vmClass.StaticFields.Add(vmField);
                        vmClass.StaticFieldSize += fieldType.Size;
                    }
                    else
                    {
                        vmField = new VMField(fieldInfo.Flag, fieldType, fieldInfo.Class, vmClass.FieldSize);
                        module.FieldPoolLink.Add(vmField);
                        vmClass.Fields.Add(vmField);
                        vmClass.FieldSize += fieldType.Size;
                    }
                }
            }

            // 完成静态空间分配
            foreach (var vmClass in module.ClassPoolLink)
            {
                if (vmClass != null)
                {
                    vmClass.StaticFieldAddress = MemoryMap.MapToAbsolute(Malloc(vmClass.StaticFieldSize).Offset, MemoryTag.METHOD);
                }
            }

            // Method
            int methodIndex = 0;
            foreach ((MethodConstantInfo methodInfo, BinaryMethod binaryMethod) in module.MethodPool.Zip(binaryModule.Methods))
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
                        Flag = new AccessFlag() { Flag = methodInfo.Flag },
                        MethodIndex = methodIndex,
                        DescriptorAddress = module.StringPoolLink[methodInfo.Descriptor - 1],
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
            uint moduleNameAddress, nameAddress, classNameAddress;
            VMModule importedModule;
            VMClass outerClass;
            // 外部类
            for (int i = 0; i < module.ClassPool.Length; ++i)
            {
                if (module.ClassPoolLink[i] != null)
                {
                    continue;
                }

                ClassConstantInfo classInfo = module.ClassPool[i];
                moduleNameAddress = module.StringPoolLink[classInfo.Module - 1];
                Modules.TryGetValue(moduleNameAddress, out importedModule);

                nameAddress = module.StringPoolLink[classInfo.Name - 1];
                if (!importedModule.Classes.TryGetValue(nameAddress, out outerClass))
                {
                    throw new XiVMError($"Outer class not found");
                }
                module.ClassPoolLink[i] = outerClass;
            }

            // 外部域
            for (int i = 0; i < module.FieldPool.Length; ++i)
            {
                if (module.FieldPoolLink[i] != null)
                {
                    continue;
                }

                FieldConstantInfo fieldInfo = module.FieldPool[i];
                moduleNameAddress = module.StringPoolLink[module.ClassPool[fieldInfo.Class - 1].Module - 1];
                Modules.TryGetValue(moduleNameAddress, out importedModule);

                // TODO 可以将VMModule.Classes的结构化信息补全，这样直接在结构化信息中查找效率更高一些
                // 外部函数符号也可以这样优化
                classNameAddress = module.StringPoolLink[module.ClassPool[fieldInfo.Class - 1].Name - 1];
                nameAddress = module.StringPoolLink[fieldInfo.Name - 1];
                uint descriptorAddress = module.StringPoolLink[fieldInfo.Descriptor - 1];
                foreach ((FieldConstantInfo candidateFieldInfo, VMField vmField) in importedModule.FieldPool.Zip(importedModule.FieldPoolLink))
                {
                    // 模块名类名函数名描述符匹配，未比较flag
                    if (moduleNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateFieldInfo.Class - 1].Module - 1] &&
                        classNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateFieldInfo.Class - 1].Name - 1] &&
                        nameAddress == importedModule.StringPoolLink[candidateFieldInfo.Name - 1] &&
                        descriptorAddress == importedModule.StringPoolLink[candidateFieldInfo.Descriptor - 1])
                    {
                        // 建立Link
                        module.FieldPoolLink[i] = vmField;
                        break;
                    }
                }
            }

            // 外部函数符号
            for (int i = 0; i < module.MethodPool.Length; ++i)
            {
                if (module.MethodPoolLink[i] != null)
                {
                    // 已经填上了
                    continue;
                }

                MethodConstantInfo methodInfo = module.MethodPool[i];
                moduleNameAddress = module.StringPoolLink[module.ClassPool[methodInfo.Class - 1].Module - 1];
                Modules.TryGetValue(moduleNameAddress, out importedModule);

                classNameAddress = module.StringPoolLink[module.ClassPool[methodInfo.Class - 1].Name - 1];
                nameAddress = module.StringPoolLink[methodInfo.Name - 1];
                uint descriptorAddress = module.StringPoolLink[methodInfo.Descriptor - 1];
                foreach ((MethodConstantInfo candidateMethodInfo, VMMethod vmMethod) in importedModule.MethodPool.Zip(importedModule.MethodPoolLink))
                {
                    // 模块名类名函数名描述符匹配，未比较flag
                    if (moduleNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateMethodInfo.Class - 1].Module - 1] &&
                        classNameAddress == importedModule.StringPoolLink[importedModule.ClassPool[candidateMethodInfo.Class - 1].Name - 1] &&
                        nameAddress == importedModule.StringPoolLink[candidateMethodInfo.Name - 1] &&
                        descriptorAddress == importedModule.StringPoolLink[candidateMethodInfo.Descriptor - 1])
                    {
                        // 建立Link
                        module.MethodPoolLink[i] = vmMethod;
                        break;
                    }
                }
            }
        }

        public static byte[] GetData(uint addr)
        {
            if (DataMap.TryGetValue(addr, out HeapData data))
            {
                return data.Data;
            }
            else
            {
                throw new XiVMError($"Invalid method area addr {addr}");
            }
        }

        #region Mallocs

        private static HeapData Malloc(int size)
        {
            if (size == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + size > SizeLimit)
            {
                throw new XiVMError("MethodArea overflow");
            }
            HeapData ret = new HeapData((uint)Size, new byte[size]);
            DataMap.Add((uint)Size, ret);
            Size += size;
            return ret;
        }

        private static HeapData Malloc(byte[] data)
        {
            if (data.Length == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + data.Length > SizeLimit)
            {
                throw new XiVMError("MethodArea overflow");
            }

            HeapData ret = new HeapData((uint)Size, data);
            DataMap.Add((uint)Size, ret);
            Size += data.Length;
            return ret;
        }

        /// <summary>
        /// TODO Array的长度信息
        /// </summary>
        /// <param name="elementSize"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private static HeapData MallocArray(int elementSize, int len)
        {
            int size = len * elementSize + HeapData.MiscDataSize + HeapData.ArrayLengthSize;

            HeapData ret = Malloc(size);
            return ret;
        }

        #endregion
    }
}
