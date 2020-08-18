using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM.Runtime
{

    internal static class Heap
    {
        public static readonly int MaxSize = 0x1000000;


        private static LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();
        internal static uint Size { private set; get; } = 0;

        public static uint Malloc(int size)
        {
            if (size == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
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

        public static uint MallocArray(int elementSize, int len)
        {
            int size = len * elementSize + HeapData.MiscDataSize + HeapData.ArrayLengthSize;
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

        /// <summary>
        /// TODO 考虑用哈希表
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static byte[] GetData(uint addr)
        {
            LinkedListNode<HeapData> cur = Data.First;
            while (cur != null)
            {
                if (addr < cur.Value.Offset)
                {
                    break;
                }
                else if (addr == cur.Value.Offset)
                {
                    return cur.Value.Data;
                }
                cur = cur.Next;
            }
            throw new XiVMError($"Invalid heap addr {addr}");
        }
    }

    internal class HeapData
    {
        /// <summary>
        /// 类型信息以及GC信息
        /// </summary>
        public static readonly int MiscDataSize = 2 * sizeof(int);
        /// <summary>
        /// 字符串头部长度信息
        /// </summary>
        public static int StringLengthSize = sizeof(int);
        /// <summary>
        /// 字符串指向的数组
        /// </summary>
        public static int StringDataSize = sizeof(uint);
        /// <summary>
        /// 数组头部长度信息
        /// </summary>
        public static int ArrayLengthSize = sizeof(int);

        public static int ArrayOffsetMap(int elementSize, int index)
        {
            return index * elementSize + MiscDataSize + ArrayLengthSize;
        }

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
