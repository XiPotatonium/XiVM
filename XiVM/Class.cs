using System;
using System.Collections.Generic;

namespace XiVM
{
    [Serializable]
    internal class BinaryClass
    {
    }

    /// <summary>
    /// Class的唯一标识
    /// </summary>
    public class ClassType : VariableType
    {
        public string ClassName { private set; get; }

        internal ClassType(string name)
            : base(VariableTypeTag.ADDRESS)
        {
            ClassName = name;
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

    public class Class
    {
        public ClassType Type { private set; get; }
        public List<Variable> StaticVariables { private set; get; } = new List<Variable>();
        public List<Variable> Variables { set; get; } = new List<Variable>();
        public List<Function> StaticFunctions { private set; get; } = new List<Function>();
        public List<Function> Functions { private set; get; } = new List<Function>();

        internal Class(ClassType type)
        {
            Type = type;
        }
    }
}
