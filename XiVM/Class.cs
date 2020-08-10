using System;
using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM
{
    [Serializable]
    public class BinaryClassType
    {
        public int ConstantPoolIndex { set; get; }
        public BinaryMethod[] Methods { internal set; get; }
    }

    internal class VMClass
    {
        public VMModule Parent { set; get; }
        public Dictionary<uint, List<VMMethod>> Methods { set; get; }
        public List<VMClassField> StaticFields { set; get; }
        public int StaticFieldSize { set; get; }
        public List<VMClassField> Fields { set; get; }
        public int FieldSize { set; get; }
    }

    /// <summary>
    /// 类的静态信息
    /// </summary>
    public class ClassType : VariableType, IConstantPoolValue
    {
        public Module Parent { private set; get; }

        /// <summary>
        /// 成员变量
        /// </summary>
        public Dictionary<string, ClassField> Fields { private set; get; } = new Dictionary<string, ClassField>();

        /// <summary>
        /// 包括静态和非静态方法
        /// </summary>
        public Dictionary<string, List<Method>> Methods { private set; get; } = new Dictionary<string, List<Method>>();

        public Method StaticInitializer { internal set; get; }
        public int ConstantPoolIndex { get; set; }
        public string Name => Parent.StringPool.ElementList[Parent.ClassPool.ElementList[ConstantPoolIndex - 1].Name - 1];

        internal ClassType(Module module, int index)
            : base(VariableTypeTag.ADDRESS)
        {
            Parent = module;
            ConstantPoolIndex = index;
        }

        /// <summary>
        /// 请使用这个函数添加field
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="flag"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal ClassField AddField(string name, VariableType type, AccessFlag flag, int index)
        {
            if (Methods.ContainsKey(name))
            {
                throw new XiVMError($"Dupilcate Name {name} in class {Name}");
            }

            ClassField field = new ClassField(flag, this, type, index);
            Fields.Add(name, field);
            return field;
        }

        /// <summary>
        /// API使用这个函数添加method
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="flag"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Method AddMethod(string name, MethodType type, AccessFlag flag, int index)
        {
            if (Fields.ContainsKey(name))
            {
                throw new XiVMError($"Dupilcate Name {name} in class {Name}");
            }

            Method function = new Method(type, this, flag, index);
            if (Methods.TryGetValue(name, out List<Method> value))
            {
                foreach (Method m in value)
                {
                    if (m.Type.Equivalent(type))
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
            foreach (VariableType paramType in function.Type.Params)
            {
                offset -= paramType.SlotSize;
                function.Params.Add(new Variable(paramType) { Offset = offset });
            }

            return function;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b is ClassType bType)
            {
                return (Name == bType.Name) && base.Equivalent(b);
            }
            return false;
        }
    }

    /// <summary>
    /// 类对象的信息
    /// </summary>
    //public class ClassInstance : Variable
    //{
    //    public ClassType ClassType => (ClassType)Type;
    //    public int InstanceSize { private set; get; } = 0;

    //    /// <summary>
    //    /// Offset 对于Class Instance无意义
    //    /// 注意类定义完成之前是可以创建对象的，因此在构造函数里不要对ClassType产生任何依赖
    //    /// </summary>
    //    /// <param name="classType"></param>
    //    public ClassInstance(ClassType classType)
    //        : base(classType, 0)
    //    {

    //    }

    //    /// <summary>
    //    /// 调用这个之前务必保证ClassType已经建立完毕
    //    /// 为ClassInstance确定堆上要分配多大空间
    //    /// 对于一般对象而言，就是成员变量空间
    //    /// 但是为了支持数组这样的特殊数据，允许分配额外空间
    //    /// </summary>
    //    /// <param name="additionalSize">局部变量外的额外空间, 单位byte</param>
    //    public void SetSize(int additionalSize = 0)
    //    {

    //    }
    //}

    public class AccessFlag
    {
        public static readonly AccessFlag DefaultFlag = new AccessFlag();

        private static readonly uint StaticFlag = 0x01;

        public uint Flag { set; get; } = 0;

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
        ClassType Parent { set; get; }
        AccessFlag AccessFlag { set; get; }
    }

    public interface IConstantPoolValue
    {
        int ConstantPoolIndex { set; get; }
    }
}
