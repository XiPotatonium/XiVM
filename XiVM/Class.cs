using System;

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

        }
    }

    public class Class
    {
        public ClassType Type { private set; get; }

        internal Class(ClassType type)
        {
            Type = type;
        }
    }
}
