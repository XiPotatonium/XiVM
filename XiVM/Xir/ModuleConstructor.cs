using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using XiVM.ConstantTable;
using XiVM.Runtime;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public Module Module { private set; get; }
        public string Name => Module.Name;
        public List<ClassType> Classes => Module.Classes;

        public BasicBlock CurrentBasicBlock { set; get; }
        public Method CurrentMethod => CurrentBasicBlock?.Function;
        public ClassType CurrentClass => CurrentMethod?.Parent;
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
                        sw.WriteLine($"#{i + 1}: {binaryModule.StringPool[i]}");
                    }

                    sw.WriteLine($"\n.ClassPool");
                    for (int i = 0; i < binaryModule.ClassPool.Length; ++i)
                    {
                        sw.WriteLine($"#{i + 1}: {binaryModule.ClassPool[i].Module}" +
                            $" {binaryModule.ClassPool[i].Name}");
                    }

                    sw.WriteLine($"\n.MethodPool");
                    for (int i = 0; i < binaryModule.MethodPool.Length; ++i)
                    {
                        sw.WriteLine($"#{i + 1}: {binaryModule.MethodPool[i].Class} {binaryModule.MethodPool[i].Name}" +
                            $" {binaryModule.MethodPool[i].Type} {binaryModule.MethodPool[i].Local}");
                    }

                    sw.WriteLine($"\n.FieldPool");
                    for (int i = 0; i < binaryModule.FieldPool.Length; ++i)
                    {
                        sw.WriteLine($"#{i + 1}: {binaryModule.FieldPool[i].Class}" +
                            $" {binaryModule.FieldPool[i].Name} {binaryModule.FieldPool[i].Type}");
                    }

                    foreach (ClassType classType in Classes)
                    {
                        sw.WriteLine($"\n.Class {classType.Name} {{\n\t#{classType.ConstantPoolIndex}");
                        foreach (KeyValuePair<string, List<Method>> methodGroup in classType.Methods)
                        {
                            foreach (Method method in methodGroup.Value)
                            {
                                sw.WriteLine($"\n\t.Method {method.Name} {method.Descriptor} {{\n\t\t#{method.ConstantPoolIndex}");
                                foreach (BasicBlock bb in method.BasicBlocks)
                                {
                                    foreach (Instruction inst in bb.Instructions)
                                    {
                                        sw.WriteLine($"\t\t{inst}");
                                    }
                                }
                                sw.WriteLine($"\t}} // {method.Name} {method.Descriptor}");
                            }
                        }
                        sw.WriteLine($"}} // {classType.Name}");
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
        public ClassType AddClassType(string name)
        {
            ClassType ret = new ClassType(Module, ClassPool.Add(
                new ClassConstantInfo(Module.ModuleNameIndex, StringPool.TryAdd(name))));
            Classes.Add(ret);

            // 静态构造函数
            ret.StaticInitializer = AddMethod(ret, "(sinit)",
                AddMethodType(null, new List<VariableType>()),
                AccessFlag.DefaultFlag);
            AddBasicBlock(ret.StaticInitializer);
            return ret;
        }

        public MethodType AddMethodType(VariableType retType, List<VariableType> ps)
        {
            int index = StringPool.TryAdd(MethodType.GetDescriptor(retType, ps));
            return new MethodType(retType, ps, index);
        }

        public ClassField AddClassField(ClassType classType, string name, VariableType type, AccessFlag flag)
        {
            int index = FieldPool.Add(new FieldConstantInfo(
                classType.ConstantPoolIndex,
                StringPool.TryAdd(name),
                StringPool.TryAdd(type.ToString()),
                flag.Flag));
            return classType.AddField(name, type, flag, index);
        }

        public Method AddMethod(ClassType classType, string name, MethodType type, AccessFlag flag)
        {
            int index = MethodPool.Add(new MethodConstantInfo(
                classType.ConstantPoolIndex,
                StringPool.TryAdd(name),
                StringPool.TryAdd(type.ToString()),
                flag.Flag));
            Method method = classType.AddMethod(name, type, flag, index);
            Methods.Add(method);
            return method;
        }

        /// <summary>
        /// 完成函数生成，会将局部变量信息保存
        /// </summary>
        /// <param name="method"></param>
        public void CompleteMethodGeneration(Method method)
        {
            MethodConstantInfo info = MethodPool.ElementList[method.ConstantPoolIndex - 1];
            if (method.Locals.Count == 0)
            {
                info.Local = 0;
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (Variable v in method.Locals)
            {
                sb.Append(v.Type.ToString());
            }
            info.Local = StringPool.TryAdd(sb.ToString());
        }

        /// <summary>
        /// 在构造类的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的类）时使用
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public int AddClassPoolInfo(string moduleName, string className)
        {
            return ClassPool.TryAdd(new ClassConstantInfo(StringPool.TryAdd(moduleName), StringPool.TryAdd(className)));
        }

        /// <summary>
        /// 在构造成员的时候不需要手动调用这个函数
        /// 是在方法代码生成过程中遇到（可能是其他module的method）时使用
        /// </summary>
        /// <param name="classIndex"></param>
        /// <param name="name"></param>
        /// <param name="descriptor"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int AddMethodPoolInfo(int classIndex, string name, string descriptor, uint flag)
        {
            int index = MethodPool.TryAdd(new MethodConstantInfo(classIndex, StringPool.TryAdd(name), StringPool.TryAdd(descriptor), flag));
            if (index > Methods.Count)
            {
                // 是新创建的，为了Methods和MethodPool可以匹配，要添加null
                Methods.Add(null);
            }
            return index;
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

        public uint AddConstantPool(string name)
        {
            throw new NotImplementedException();
        }
    }
}
