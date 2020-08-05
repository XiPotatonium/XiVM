using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.Errors;
using XiVM.Executor;

namespace XiVM
{
    /// <summary>
    /// 字节码中的Module
    /// </summary>
    [Serializable]
    internal class BinaryModule
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
        public string[] StringLiterals { set; get; }
        public BinaryClassType[] Classes { set; get; }
        public BinaryFunction[] Functions { set; get; }
    }

    /// <summary>
    /// BinaryModule加载之后，已经进行了Link
    /// </summary>
    internal class VMModule
    {
        /// <summary>
        /// TODO 改成HeapData?好了
        /// </summary>
        public LinkedListNode<HeapData>[] StringLiterals { set; get; }
        public BinaryClassType[] Classes { set; get; }
        public BinaryFunction[] Functions { set; get; }
    }
}
