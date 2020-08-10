using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.ConstantTable;
using XiVM.Errors;

namespace XiVM
{
    /// <summary>
    /// 字节码中的Module
    /// </summary>
    [Serializable]
    public class BinaryModule : ModuleHeader
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
        public ClassConstantInfo[] ClassPool { set; get; }
        public MethodConstantInfo[] MethodPool { set; get; }
        public FieldConstantInfo[] FieldPool { set; get; }
        public byte[][] Code { set; get; }

        public IList<string> StringPoolList => StringPool;
        public IList<ClassConstantInfo> ClassPoolList => ClassPool;
        public IList<MethodConstantInfo> MethodPoolList => MethodPool;
        public IList<FieldConstantInfo> FieldPoolList => FieldPool;
    }

    public interface ModuleHeader
    {
        int ModuleNameIndex { get; }
        IList<string> StringPoolList { get; }
        IList<ClassConstantInfo> ClassPoolList { get; }
        IList<MethodConstantInfo> MethodPoolList { get; }
        IList<FieldConstantInfo> FieldPoolList { get; }
    }

    public class Module : ModuleHeader
    {
        public int ModuleNameIndex { private set; get; }
        public string Name => StringPool.ElementList[ModuleNameIndex - 1];
        public List<ClassType> Classes { private set; get; } = new List<ClassType>();
        /// <summary>
        /// 对methods的索引，仅仅为了导出为二进制方便，Methods和MethodPool对应
        /// </summary>
        public List<Method> Methods { private set; get; } = new List<Method>();
        public ConstantTable<string> StringPool { private set; get; } = new ConstantTable<string>();
        public ConstantTable<ClassConstantInfo> ClassPool { private set; get; } = new ConstantTable<ClassConstantInfo>();
        public ConstantTable<MethodConstantInfo> MethodPool { private set; get; } = new ConstantTable<MethodConstantInfo>();
        public ConstantTable<FieldConstantInfo> FieldPool { private set; get; } = new ConstantTable<FieldConstantInfo>();

        public IList<string> StringPoolList => StringPool.ElementList;
        public IList<ClassConstantInfo> ClassPoolList => ClassPool.ElementList;
        public IList<MethodConstantInfo> MethodPoolList => MethodPool.ElementList;
        public IList<FieldConstantInfo> FieldPoolList => FieldPool.ElementList;

        public Module(string name)
        {
            ModuleNameIndex = StringPool.Add(name);
        }

        public BinaryModule ToBinary()
        {
            return new BinaryModule
            {
                ModuleNameIndex = ModuleNameIndex,
                StringPool = StringPool.ToArray(),
                ClassPool = ClassPool.ToArray(),
                MethodPool = MethodPool.ToArray(),
                FieldPool = FieldPool.ToArray(),
                Code = Methods.Select(m => m?.ToBinary()).ToArray()
            };
        }
    }

    internal class VMModule
    {
        /// <summary>
        /// 映射表，从原来的index映射到address
        /// </summary>
        public List<uint> StringPoolLink { set; get; }
        public Dictionary<uint, VMClass> Classes { set; get; }
        public ClassConstantInfo[] ClassPool { set; get; }
        public MethodConstantInfo[] MethodPool { set; get; }
        /// <summary>
        /// 映射表，从MethodPool到MethodIndexTable
        /// </summary>
        public List<int> MethodPoolLink { set; get; }
        public FieldConstantInfo[] FieldPool { set; get; }
        public List<uint> FieldPoolLink { set; get; }
    }
}
