using System;

namespace XiVM.ConstantTable
{
    [Serializable]
    public class ClassConstantInfo
    {
        public int Name { private set; get; }
        public int Module { private set; get; }

        internal ClassConstantInfo(int module, int nameIndex)
        {
            Module = module;
            Name = nameIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is ClassConstantInfo info)
            {
                return Name == info.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }


    [Serializable]
    public class MethodConstantInfo
    {
        public int Class { private set; get; }
        public int Name { private set; get; }
        public int Type { private set; get; }
        /// <summary>
        /// 仅在函数生成完毕后有效
        /// </summary>
        public int Local { set; get; }

        internal MethodConstantInfo(int classIndex, int nameIndex, int typeIndex)
        {
            Class = classIndex;
            Name = nameIndex;
            Type = typeIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MethodConstantInfo info)
            {
                return Name == info.Name &&
                    Class == info.Class &&
                    Type == info.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Class, Name, Type);
        }
    }

    [Serializable]
    public class FieldConstantInfo
    {
        public int Class { private set; get; }
        public int Name { private set; get; }
        public int Type { private set; get; }

        internal FieldConstantInfo(int classIndex, int nameIndex, int typeIndex)
        {
            Class = classIndex;
            Name = nameIndex;
            Type = typeIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MethodConstantInfo info)
            {
                return Name == info.Name &&
                    Class == info.Class &&
                    Type == info.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Class, Name, Type);
        }
    }
}
