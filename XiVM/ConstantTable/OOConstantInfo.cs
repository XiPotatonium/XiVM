using System;

namespace XiVM.ConstantTable
{
    [Serializable]
    public class ClassConstantInfo
    {
        public int Name { private set; get; }
        public int Module { private set; get; }

        public ClassConstantInfo(int moduleIndex, int nameIndex)
        {
            Module = moduleIndex;
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
        public uint Flag { private set; get; }
        public int Class { private set; get; }
        public int Name { private set; get; }
        public int Type { private set; get; }

        internal MethodConstantInfo(int classIndex, int nameIndex, int typeIndex, uint flag)
        {
            Class = classIndex;
            Name = nameIndex;
            Type = typeIndex;
            Flag = flag;
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
                    Class == info.Class;
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
        public uint Flag { private set; get; }

        internal FieldConstantInfo(int classIndex, int nameIndex, int typeIndex, uint flag)
        {
            Class = classIndex;
            Name = nameIndex;
            Type = typeIndex;
            Flag = flag;
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
                    Class == info.Class;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Class, Name, Type);
        }
    }
}
