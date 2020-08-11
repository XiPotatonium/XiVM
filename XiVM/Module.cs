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
        public BinaryMethod[] Code { set; get; }

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

    public class ModuleType : VariableType
    {
        public string ModuleName { set; get; }
        public ModuleType() : base(VariableTypeTag.INVALID)
        {

        }

        public override bool Equivalent(VariableType b)
        {
            if (b == null)
            {
                return false;
            }
            if (b is ModuleType bType)
            {
                return ModuleName == bType.ModuleName;
            }
            return false;
        }
    }

    public class Module : ModuleHeader
    {
        public int ModuleNameIndex { private set; get; }
        public string Name => StringPool.ElementList[ModuleNameIndex - 1];
        public List<Class> Classes { private set; get; } = new List<Class>();
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
        /// address，指向方法区字符串常量池中的字符串
        /// </summary>
        public List<uint> StringPoolLink { set; get; }
        /// <summary>
        /// 类名地址->VMClass
        /// </summary>
        public Dictionary<uint, VMClass> Classes { set; get; }
        public ClassConstantInfo[] ClassPool { set; get; }
        public List<VMClass> ClassPoolLink { set; get; }
        public MethodConstantInfo[] MethodPool { set; get; }
        public List<VMMethod> MethodPoolLink { set; get; }
        public FieldConstantInfo[] FieldPool { set; get; }
        /// <summary>
        /// offset，是field在该类的静态field空间的offset
        /// </summary>
        public List<int> FieldPoolLink { set; get; }
    }
}
