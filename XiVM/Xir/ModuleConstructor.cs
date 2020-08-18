using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.ConstantTable;
using XiVM.Runtime;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public Module Module { private set; get; }
        public string Name => Module.Name;
        public List<Class> Classes => Module.Classes;

        public BasicBlock CurrentBasicBlock { set; get; }
        public Method CurrentMethod => CurrentBasicBlock?.Function;
        public Class CurrentClass => CurrentMethod?.Parent;
        private LinkedList<Instruction> CurrentInstructions => CurrentBasicBlock?.Instructions;

        public ConstantTable<string> StringPool => Module.StringPool;
        public ConstantTable<ClassConstantInfo> ClassPool => Module.ClassPool;
        public List<Method> Methods => Module.Methods;
        public ConstantTable<MethodConstantInfo> MethodPool => Module.MethodPool;
        public ConstantTable<FieldConstantInfo> FieldPool => Module.FieldPool;

        public ModuleConstructor(string name)
        {
            Module = new Module(name);
        }

        public void Dump(string dirName, bool dumpXir = true)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                dirName = ".";
            }

            BinaryModule binaryModule = Module.ToBinary();

            using (FileStream fs = new FileStream(Path.Combine(dirName, $"{Name}.xibc"), FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fs, binaryModule);
            }

            if (dumpXir)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(dirName, $"{Name}.xir")))
                {
                    sw.WriteLine($".Module {Name}");


                    sw.WriteLine($"\n.StringPool");
                    for (int i = 0; i < binaryModule.StringPool.Length; ++i)
                    {
                        sw.WriteLine("{0, 8}{1}", $"#{i + 1}: ", binaryModule.StringPool[i]);
                    }

                    sw.WriteLine($"\n.ClassPool");
                    for (int i = 0; i < binaryModule.ClassPool.Length; ++i)
                    {
                        sw.WriteLine("{0, 8}{1} {2}", $"#{i + 1}: ",
                            binaryModule.ClassPool[i].Module,
                            binaryModule.ClassPool[i].Name);
                    }

                    sw.WriteLine($"\n.MethodPool");
                    for (int i = 0; i < binaryModule.MethodPool.Length; ++i)
                    {
                        sw.WriteLine("{0, 8}{1} {2} {3} {4}", $"#{i + 1}: ",
                            binaryModule.MethodPool[i].Class,
                            binaryModule.MethodPool[i].Name,
                            binaryModule.MethodPool[i].Descriptor,
                            binaryModule.MethodPool[i].Flag);
                    }

                    sw.WriteLine($"\n.FieldPool");
                    for (int i = 0; i < binaryModule.FieldPool.Length; ++i)
                    {
                        sw.WriteLine("{0, 8}{1} {2} {3} {4}", $"#{i + 1}: ",
                            binaryModule.FieldPool[i].Class,
                            binaryModule.FieldPool[i].Name,
                            binaryModule.FieldPool[i].Descriptor,
                            binaryModule.FieldPool[i].Flag);
                    }

                    foreach (Class classType in Classes)
                    {
                        sw.WriteLine($"\n.Class {classType.Name} {{");
                        sw.WriteLine($"    # ClassPoolIndex {classType.ConstantPoolIndex}");
                        foreach (KeyValuePair<string, List<Method>> methodGroup in classType.Methods)
                        {
                            foreach (Method method in methodGroup.Value)
                            {
                                sw.WriteLine($"\n    .Method {method.Name} {method.Descriptor} {{");
                                sw.WriteLine($"        # MethodPoolIndex {method.ConstantPoolIndex}");
                                sw.WriteLine($"        # LocalTypeIndexTable: ");
                                foreach (var localInfo in binaryModule.Methods[method.ConstantPoolIndex - 1].LocalDescriptorIndex)
                                {
                                    sw.WriteLine($"            # {localInfo}");
                                }
                                int pc = 0;
                                foreach (BasicBlock bb in method.BasicBlocks)
                                {
                                    foreach (Instruction inst in bb.Instructions)
                                    {
                                        sw.WriteLine("{0, 8}{1}", $"{pc}: ", inst);
                                        pc += 1;
                                        if (inst.Params != null)
                                        {
                                            pc += inst.Params.Length;
                                        }
                                    }
                                }
                                sw.WriteLine($"    }} \t// {method.Name}");
                            }
                        }
                        sw.WriteLine($"}} \t// {classType.Name}");
                    }
                }
            }
        }

        #region Class Construction

        /// <summary>
        /// 创建一个Class，会自动为静态构造函数添加一个bb
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Class AddClass(string name)
        {
            Class ret = new Class(Module, ClassPool.Add(
                new ClassConstantInfo(Module.ModuleNameIndex, StringPool.TryAdd(name))));
            Classes.Add(ret);

            // 静态构造函数
            ret.StaticInitializer = AddMethod(ret, "(sinit)",
                null, new List<VariableType>(),
                new AccessFlag() { IsStatic = true });
            AddBasicBlock(ret.StaticInitializer);
            return ret;
        }

        public Field AddClassField(Class classType, string name, VariableType type, AccessFlag flag)
        {
            int index = FieldPool.Add(new FieldConstantInfo(
                classType.ConstantPoolIndex,
                StringPool.TryAdd(name),
                StringPool.TryAdd(type.GetDescriptor()),
                flag.Flag));
            return classType.AddField(name, type, flag, index);
        }

        public Method AddMethod(Class classType, string name, VariableType retType, List<VariableType> ps, AccessFlag flag)
        {
            MethodDeclarationInfo declarationInfo = new MethodDeclarationInfo(retType, ps,
                StringPool.TryAdd(MethodDeclarationInfo.GetDescriptor(retType, ps)));
            int index = MethodPool.Add(new MethodConstantInfo(
                classType.ConstantPoolIndex,
                StringPool.TryAdd(name),
                StringPool.TryAdd(declarationInfo.GetDescriptor()),
                flag.Flag));
            Method method = classType.AddMethod(name, declarationInfo, flag, index);
            Methods.Add(method);
            return method;
        }

        /// <summary>
        /// 在构造class的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的class）时使用
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public int AddClassPoolInfo(ClassType classType)
        {
            return ClassPool.TryAdd(new ClassConstantInfo(
                StringPool.TryAdd(classType.ModuleName), StringPool.TryAdd(classType.ClassName)));
        }

        /// <summary>
        /// 在构造成员的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的method）时使用
        /// </summary>
        /// <param name="methodType"></param>
        /// <param name="descriptor"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int AddMethodPoolInfo(MemberType methodType, string descriptor, uint flag)
        {
            int index = MethodPool.TryAdd(new MethodConstantInfo(AddClassPoolInfo(methodType.ClassType),
                StringPool.TryAdd(methodType.Name), StringPool.TryAdd(descriptor), flag));
            if (index > Methods.Count)
            {
                // 是新创建的，为了Methods和MethodPool可以匹配，要添加null
                Methods.Add(null);
            }
            return index;
        }

        /// <summary>
        /// 在构造成员的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的field）时使用
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="descriptor"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int AddFieldPoolInfo(MemberType fieldType, string descriptor, uint flag)
        {
            return FieldPool.TryAdd(new FieldConstantInfo(AddClassPoolInfo(fieldType.ClassType),
                StringPool.TryAdd(fieldType.Name), StringPool.TryAdd(descriptor), flag));
        }

        /// <summary>
        /// 在构造成员的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的field）时使用
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="name"></param>
        /// <param name="descriptor"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int AddFieldPoolInfo(ClassType classType, string name, string descriptor, uint flag)
        {
            return FieldPool.TryAdd(new FieldConstantInfo(AddClassPoolInfo(classType),
                StringPool.TryAdd(name), StringPool.TryAdd(descriptor), flag));
        }

        #endregion

        #region Method Construction

        public BasicBlock AddBasicBlock(Method method)
        {
            BasicBlock ret = new BasicBlock(method);
            method.BasicBlocks.AddLast(ret);
            return ret;
        }

        public BasicBlock InsertBeforeBasicBlock(BasicBlock basicBlock)
        {
            LinkedListNode<BasicBlock> node = basicBlock.Function.BasicBlocks.Find(basicBlock);
            BasicBlock ret = new BasicBlock(basicBlock.Function);
            basicBlock.Function.BasicBlocks.AddBefore(node, ret);
            return ret;
        }

        public Variable AddLocalVariable(string name, VariableType type)
        {
            Variable xirVariable;
            if (CurrentMethod.Locals.Count == 0)
            {
                // 第一个局部变量
                xirVariable = new Variable(type) { Offset = Stack.MiscDataSize };
            }
            else
            {
                xirVariable = new Variable(type)
                {
                    Offset = CurrentMethod.Locals[^1].Offset + CurrentMethod.Locals[^1].Type.SlotSize
                };
            }
            CurrentMethod.Locals.Add(xirVariable);

            return xirVariable;
        }

        #endregion
    }
}
