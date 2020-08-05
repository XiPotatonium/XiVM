using System;
using System.Collections.Generic;

namespace XiVM
{
    [Serializable]
    internal class BinaryClassType
    {
    }

    /// <summary>
    /// 类的静态信息
    /// </summary>
    public class ClassType : VariableType
    {
        /// <summary>
        /// Class的名字，使用这个判断两个ClassType是否相等
        /// </summary>
        public string ClassName { private set; get; }
        /// <summary>
        /// Class在模块中的Index，模块需要Index来构造对应类的对象
        /// </summary>
        internal int Index { private set; get; }
        /// <summary>
        /// 静态变量
        /// </summary>
        internal List<Variable> StaticVariables { set; get; } = new List<Variable>();
        /// <summary>
        /// 成员变量
        /// </summary>
        internal List<Variable> Variables { set; get; } = new List<Variable>();
        /// <summary>
        /// 静态函数
        /// 默认构造函数应该是StaticFunctions的第一个
        /// </summary>
        internal List<Function> StaticFunctions { set; get; } = new List<Function>();
        /// <summary>
        /// 成员函数
        /// </summary>
        internal List<Function> Functions { set; get; } = new List<Function>();

        public ClassType(string name)
            : base(VariableTypeTag.ADDRESS)
        {
            ClassName = name;
        }

        public Variable AddVariable(VariableType type)
        {
            Variable v = new Variable(type, Variables.Count == 0 ? 0 : Variables[^1].Offset + Variables[^1].Type.SlotSize);
            Variables.Add(v);
            return v;
        }

        public override bool Equivalent(VariableType b)
        {
            if (b is ClassType bType)
            {
                return (ClassName == bType.ClassName) && base.Equivalent(b);
            }
            return false;
        }
    }

    /// <summary>
    /// 类对象的信息
    /// </summary>
    public class ClassInstance : Variable
    {
        public ClassType ClassType => (ClassType)Type;
        public int InstanceSize { private set; get; } = 0;

        /// <summary>
        /// Offset 对于Class Instance无意义
        /// 注意类定义完成之前是可以创建对象的，因此在构造函数里不要对ClassType产生任何依赖
        /// </summary>
        /// <param name="classType"></param>
        public ClassInstance(ClassType classType)
            : base(classType, 0)
        {

        }

        /// <summary>
        /// 调用这个之前务必保证ClassType已经建立完毕
        /// 为ClassInstance确定堆上要分配多大空间
        /// 对于一般对象而言，就是成员变量空间
        /// 但是为了支持数组这样的特殊数据，允许分配额外空间
        /// </summary>
        /// <param name="additionalSize">局部变量外的额外空间</param>
        public void SetSize(int additionalSize = 0)
        {

        }
    }
}
