﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using XiVM.Errors;

namespace XiVM.Xir
{
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

        private uint Magic { set; get; }
        public BinaryInt[] IntConstants { set; get; }
        public BinaryDouble[] DoubleConstants { set; get; }
        public BinaryString[] StringConstants { set; get; }
        public BinaryClass[] Classes { set; get; }
        public BinaryFunction[] Functions { set; get; }
        public BinaryFunction Entry { set; get; }
    }
}