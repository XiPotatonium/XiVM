using System.Collections.Generic;
using System.Text;
using XiVM.Errors;
using XiVM.Xir;

namespace XiVM
{
    internal class VMClass
    {
        public VMModule Parent { set; get; }
        public Dictionary<uint, List<VMMethod>> Methods { set; get; }
        /// <summary>
        /// GC信息
        /// </summary>
        public List<VMField> StaticFields { set; get; }
        public uint StaticFieldAddress { set; get; }
        public int StaticFieldSize { set; get; }
        /// <summary>
        /// GC信息
        /// </summary>
        public List<VMField> Fields { set; get; }
        public int FieldSize { set; get; }
    }

    /// <summary>
    /// 类的静态信息
    /// </summary>
    public class Class : IConstantPoolValue
    {
        public ClassType ClassType { get; }
        public ObjectType ObjectType { get; }

        public Module Parent { private set; get; }

        /// <summary>
        /// 成员变量
        /// </summary>
        public Dictionary<string, Field> Fields { private set; get; } = new Dictionary<string, Field>();

        /// <summary>
        /// 包括静态和非静态方法
        /// </summary>
        public Dictionary<string, List<Method>> Methods { private set; get; } = new Dictionary<string, List<Method>>();

        public Method StaticInitializer { internal set; get; }
        public int ConstantPoolIndex { get; set; }
        public string Name => Parent.StringPool.ElementList[Parent.ClassPool.ElementList[ConstantPoolIndex - 1].Name - 1];

        internal Class(Module module, int index)
        {
            Parent = module;
            ConstantPoolIndex = index;
            ClassType = new ClassType(Parent.Name, Name);
            ObjectType = new ObjectType(ClassType);
        }

        /// <summary>
        /// 请使用这个函数添加field
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="flag"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Field AddField(string name, VariableType type, AccessFlag flag, int index)
        {
            if (Methods.ContainsKey(name))
            {
                throw new XiVMError($"Dupilcate Name {name} in class {Name}");
            }

            Field field = new Field(flag, this, type, index);
            Fields.Add(name, field);
            return field;
        }

        /// <summary>
        /// API使用这个函数添加method
        /// </summary>
        /// <param name="name"></param>
        /// <param name="decl"></param>
        /// <param name="flag"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Method AddMethod(string name, MethodDeclarationInfo decl, AccessFlag flag, int index)
        {
            if (Fields.ContainsKey(name))
            {
                throw new XiVMError($"Dupilcate Name {name} in class {Name}");
            }

            Method function = new Method(decl, this, flag, index);
            if (Methods.TryGetValue(name, out List<Method> value))
            {
                foreach (Method m in value)
                {
                    if (m.Declaration.Equivalent(decl))
                    {
                        // 重复定义
                        throw new XiVMError($"Duplicate definition for {Name}.{name}");
                    }
                }
                value.Add(function);
            }
            else
            {
                Methods.Add(name, new List<Method>() { function });
            }

            // 添加参数
            int offset = 0;
            if (flag.IsStatic)
            {
                function.Params = new Variable[function.Declaration.Params.Count];
                for (int i = function.Declaration.Params.Count - 1; i >= 0; --i)
                {
                    offset -= function.Declaration.Params[i].SlotSize;
                    function.Params[i] = new Variable(function.Declaration.Params[i])
                    {
                        Offset = offset
                    };
                }
            }
            else
            {
                function.Params = new Variable[function.Declaration.Params.Count + 1];
                for (int i = function.Declaration.Params.Count - 1; i >= 0; --i)
                {
                    offset -= function.Declaration.Params[i].SlotSize;
                    function.Params[i + 1] = new Variable(function.Declaration.Params[i])
                    {
                        Offset = offset
                    };
                }
                // 成员方法默认参数this
                offset -= VariableType.AddressType.SlotSize;
                function.Params[0] = new Variable(ObjectType) { Offset = offset };

            }

            return function;
        }

    }

    public class ClassType : VariableType
    {
        public string ModuleName { get; }
        public string ClassName { get; }

        public ClassType(string moduleName, string className) 
            : base(VariableTypeTag.INVALID)
        {
            ModuleName = moduleName;
            ClassName = className;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }
            if (b is ClassType bType)
            {
                return ModuleName == bType.ModuleName &&
                    ClassName == bType.ClassName;
            }
            return false;
        }


    }

    public class ObjectType : VariableType
    {
        public static ObjectType GetObjectType(string descriptor)
        {
            if (descriptor[0] != 'L' || descriptor[^1] != ';')
            {
                throw new XiVMError($"{descriptor} is not a class descriptor");
            }
            descriptor = descriptor[1..^1];
            string[] domains = descriptor.Split('.');
            if (domains.Length != 2)
            {
                throw new XiVMError($"Class descriptor {descriptor} is not in ModuleName.ClassName form");
            }
            return new ObjectType(new ClassType(domains[0], domains[1]));
        }

        /// <summary>
        /// 其实信息有冗余
        /// </summary>
        public string ModuleName => ClassType.ModuleName;
        public string ClassName => ClassType.ClassName;
        public ClassType ClassType { get; }

        public ObjectType(ClassType classType)
            : base(VariableTypeTag.ADDRESS)
        {
            ClassType = classType;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }
            if (b is ObjectType bType)
            {
                return ClassType.Equivalent(bType.ClassType);
            }
            return false;
        }

        public override string GetDescriptor()
        {
            return $"L{ModuleName}.{ClassName};";
        }
    }

    public class MemberType : VariableType
    {

        public ClassType ClassType { set; get; }
        public string Name {  get; }
        /// <summary>
        /// 是否来自一个隐式的this.
        /// </summary>
        public bool FromThis { get; }
        /// <summary>
        /// 是否来自一个object.
        /// </summary>
        public bool FromObject { get; }
        /// <summary>
        /// 即使IsField，也不排除有同名Method
        /// </summary>
        public bool IsField { set; get; }
        public int FieldPoolIndex { set; get; }

        public MemberType(ClassType classType, string name, bool fromThis, bool fromObject) 
            : base(VariableTypeTag.INVALID)
        {
            ClassType = classType;
            Name = name;
            FromThis = fromThis;
            FromObject = fromObject;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }
            if (b is MemberType bType)
            {
                return ClassType.Equivalent(bType.ClassType) &&
                    Name == bType.Name;
            }
            return false;
        }
    }

    public struct AccessFlag
    {
        public static readonly AccessFlag DefaultFlag = new AccessFlag();

        private static readonly uint StaticFlag = 0x01;

        public uint Flag { set; get; }

        public bool IsStatic
        {
            get => (Flag & StaticFlag) != 0;
            set
            {
                if (value)
                {
                    Flag |= StaticFlag;
                }
                else
                {
                    Flag ^= StaticFlag;
                }
            }
        }
    }

    public interface IClassMember : IConstantPoolValue
    {
        Class Parent { set; get; }
        AccessFlag AccessFlag { set; get; }
    }

    public interface IConstantPoolValue
    {
        int ConstantPoolIndex { set; get; }
    }
}
