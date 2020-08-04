using System.Collections.Generic;
using System.Text;

namespace XiVM.Executor
{
    /// <summary>
    /// 这个堆是整个VM独一份的
    /// </summary>
    internal static class Heap
    {
        public static readonly int MaxHeapSize = 0x100000;  // 64MB

        private static List<Module> Modules { get; } = new List<Module>();
        private static LinkedList<HeapData> HeapData { get; } = new LinkedList<HeapData>();
        private static Dictionary<string, LinkedListNode<HeapData>> StringConstantPool { get; } = new Dictionary<string, LinkedListNode<HeapData>>();

        public static Module AddModule(BinaryModule binaryModule)
        {
            Module module = new Module()
            {
                StringLiterals = new LinkedListNode<HeapData>[binaryModule.StringLiterals.Length],
                Functions = binaryModule.Functions,
                Classes = binaryModule.Classes
            };


            for (int i = 0; i < binaryModule.StringLiterals.Length; ++i)
            {
                if (!StringConstantPool.TryGetValue(binaryModule.StringLiterals[i], out LinkedListNode<HeapData> data))
                {
                    data = HeapData.AddLast(new HeapData(
                        HeapData.Count == 0 ? 0 : HeapData.Last.Value.Offset + (uint)HeapData.Last.Value.Data.Length,
                        binaryModule.StringLiterals[i]));
                    StringConstantPool.Add(binaryModule.StringLiterals[i], data);
                }
                module.StringLiterals[i] = data;
            }

            return module;
        }
    }

    internal struct HeapData
    {
        /// <summary>
        /// 头部的类信息(类的Index)
        /// 暂定：常量池的string没有这个信息
        /// </summary>
        public static readonly int MiscDataSize = sizeof(int);

        public uint Offset { private set; get; }
        public byte[] Data { private set; get; }

        public HeapData(uint offset, uint size)
        {
            Offset = offset;
            Data = new byte[MiscDataSize + size];
        }

        public HeapData(uint offset, string literal)
        {
            Offset = offset;
            Data = Encoding.ASCII.GetBytes(literal);
        }
    }

}
