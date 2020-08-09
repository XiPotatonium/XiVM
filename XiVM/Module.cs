using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.ConstantTable;
using XiVM.Errors;
using XiVM.Runtime;
using XiVM.Xir;

namespace XiVM
{
    /// <summary>
    /// 字节码中的Module
    /// </summary>
    [Serializable]
    public class BinaryModule
    {
        public static BinaryModule Load(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                BinaryModule ret = (BinaryModule)binaryFormatter.Deserialize(fs);

                if (ret.Magic != 0x43303A29)
                {
                    throw new XiVMError("Incorrect magic number");
                }

                return ret;
            }
        }

        private uint Magic { set; get; } = 0x43303A29;
        public int ModuleNameIndex { set; get; }
        public string[] StringPool { set; get; }
        public ClassConstantInfo[] ClassConstantInfos { set; get; }
        public MemberConstantInfo[] MemberConstantInfos { set; get; }
        public BinaryClassType[] Classes { set; get; }
    }

    public class Module
    {
        public int ModuleNameIndex { private set; get; }
        public string Name => StringPool.ElementList[ModuleNameIndex - 1];
        public List<ClassType> Classes { private set; get; } = new List<ClassType>();
        public ConstantTable<string> StringPool { get; } = new ConstantTable<string>();
        public ConstantTable<ClassConstantInfo> ClassPool { get; } = new ConstantTable<ClassConstantInfo>();
        public ConstantTable<MemberConstantInfo> MemberPool { get; } = new ConstantTable<MemberConstantInfo>();

        public Module(string name)
        {
            ModuleNameIndex = StringPool.Add(name);
        }

        public BinaryModule ToBinary()
        {
            return new BinaryModule
            {
                ModuleNameIndex = ModuleNameIndex,
                Classes = Classes.Select(c => c.ToBinary()).ToArray(),
                StringPool = StringPool.ToArray(),
                ClassConstantInfos = ClassPool.ToArray(),
                MemberConstantInfos = MemberPool.ToArray()
            };
        }
    }

    internal class VMModule
    {
        /// <summary>
        /// 映射表，从原来的index映射到address
        /// </summary>
        public List<uint> StringPool { set; get; }
        public Dictionary<uint, VMClass> Classes { set; get; }
        public ClassConstantInfo[] ClassConstantInfos { set; get; }
        public MemberConstantInfo[] MemberConstantInfos { set; get; }
    }
}
