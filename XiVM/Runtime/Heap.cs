using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM.Runtime
{

    internal static class Heap
    {
        internal static readonly int MiscDataSize = sizeof(int);
        public static readonly int MaxSize = 0x1000000;


        private static LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();
        internal static uint Size { private set; get; } = 0;

        public static uint Malloc(uint size)
        {
            if (Size + size > MaxSize)
            {
                throw new XiVMError("Heap overflow");
            }
            LinkedListNode<HeapData> newData = new LinkedListNode<HeapData>(new HeapData(
                Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                new byte[size]));
            Data.AddLast(newData);
            return newData.Value.Offset;
        }

        public static byte[] GetData(uint addr, out uint offset)
        {
            LinkedListNode<HeapData> cur = Data.First;
            while (cur != null)
            {
                if (addr < cur.Value.Offset)
                {
                    break;
                }
                else if (addr < cur.Value.Offset + cur.Value.Data.Length)
                {
                    offset = addr - cur.Value.Offset;
                    return cur.Value.Data;
                }
                cur = cur.Next;
            }
            offset = 0;
            return null;
        }
    }

    internal struct HeapData
    {
        /// <summary>
        /// 这个记录的是到空间开头的offset，是相对地址而不是绝对地址
        /// </summary>
        public uint Offset { private set; get; }
        public byte[] Data { private set; get; }

        public HeapData(uint offset, byte[] data)
        {
            Offset = offset;
            Data = data;
        }
    }
}
