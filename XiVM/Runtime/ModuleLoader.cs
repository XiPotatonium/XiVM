using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XiVM.ConstantTable;
using XiVM.Errors;

namespace XiVM.Runtime
{
    internal static class ModuleLoader
    {
        /// <summary>
        /// key是模块名的addr
        /// </summary>
        public static Dictionary<uint, VMModule> Modules { get; } = new Dictionary<uint, VMModule>();
        /// <summary>
        /// key是class静态区的addr
        /// </summary>
        public static Dictionary<uint, VMClass> Classes { get; } = new Dictionary<uint, VMClass>();
        /// <summary>
        /// key是method代码位置的addr
        /// </summary>
        public static Dictionary<uint, VMMethod> Methods { get; } = new Dictionary<uint, VMMethod>();

        private static Stopwatch LoadWatch { get; } = new Stopwatch();
        private static Stopwatch DependencyLoadWatch { get; } = new Stopwatch();
        /// <summary>
        /// 总加载时间，包括依赖，单位ms
        /// </summary>
        public static long ModuleLoadTime => LoadWatch.ElapsedMilliseconds;
        /// <summary>
        /// 依赖加载时间，单位ms
        /// </summary>
        public static long DependencyLoadTime => DependencyLoadWatch.ElapsedMilliseconds;


        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="binaryModule"></param>
        /// <param name="isDependency">是执行模块还是依赖模块</param>
        /// <returns></returns>
        public static VMModule AddModule(BinaryModule binaryModule, bool isDependency)
        {
            if (!isDependency)
            {
                LoadWatch.Start();
            }

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
                module.StringPoolLink.Add(MethodArea.Singleton.AddConstantString(stringConstant));
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
                        StaticFieldSize = HeapData.MiscDataSize,    // 头部信息
                        Fields = new List<VMField>(),
                        FieldSize = HeapData.MiscDataSize           // 头部信息
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
                    vmClass.StaticFieldAddress = StaticArea.Singleton.MallocClassStaticArea(vmClass);
                    Classes.Add(vmClass.StaticFieldAddress, vmClass);
                }
            }

            // Method
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

                    // 构造VMMethod
                    VMMethod vmMethod = new VMMethod()
                    {
                        Parent = vmClass,
                        Flag = new AccessFlag() { Flag = methodInfo.Flag },
                        DescriptorAddress = module.StringPoolLink[methodInfo.Descriptor - 1],
                        LocalDescriptorAddress = binaryMethod.LocalDescriptorIndex.Select(i => module.StringPoolLink[i - 1]).ToList(),
                        CodeBlock = MethodArea.Singleton.Malloc(binaryMethod.Instructions)
                    };

                    Methods.Add(vmMethod.CodeAddress, vmMethod);

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

            if (!isDependency)
            {
                DependencyLoadWatch.Start();
            }

            // 导入外部模块
            foreach (int externalModuleNameIndex in externalModuleNameIndexes)
            {
                if (!Modules.ContainsKey(module.StringPoolLink[externalModuleNameIndex - 1]))
                {
                    // 导入未导入的模块，图的广度优先遍历
                    AddModule(Program.LoadModule(binaryModule.StringPool[externalModuleNameIndex - 1]), true);
                }
            }

            if (!isDependency)
            {
                DependencyLoadWatch.Stop();
            }

            // 链接外部符号
            ExternalSymbolResolution(module);

            if (!isDependency)
            {
                LoadWatch.Stop();
            }

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
    }
}
