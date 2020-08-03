using System.Collections.Generic;

namespace XiVM
{
    internal static class Heap
    {
        private static LinkedList<BinaryModule> Modules { get; } = new LinkedList<BinaryModule>();
        private static LinkedList<HeapData> HeapData { get; } = new LinkedList<HeapData>();
        private static HashSet<string> StringConstantPool { get; } = new HashSet<string>();

        public static void AddModule(BinaryModule module)
        {
            Modules.AddLast(module);
            for (int i = 0; i < module.StringLiterals.Length; ++i)
            {
                if (StringConstantPool.TryGetValue(module.StringLiterals[i], out string actual))
                {
                    module.StringLiterals[i] = actual;  // 让数组中的字面量指向常量池
                }
                else
                {
                    StringConstantPool.Add(module.StringLiterals[i]);
                }
            }
        }
    }

    internal struct HeapData
    {
        /// <summary>
        /// 头部的类信息(类的Index)
        /// </summary>
        public static readonly int MiscDataSize = sizeof(int);

        public byte[] Data { private set; get; }

        public HeapData(uint size)
        {

            Data = new byte[size];
        }
    }
}
