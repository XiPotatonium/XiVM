using System;
using System.Collections.Generic;
using System.Text;
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
        /// TODO 考虑用哈希表，因为不会有offset
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
            return null;
        }
    }

    internal struct HeapData
    {
        /// <summary>
        /// 类型信息
        /// </summary>
        public static readonly int MiscDataSize = sizeof(int);
        /// <summary>
        /// 头部长度信息
        /// </summary>
        private static int StringLengthSize = sizeof(int);

        public static string GetString(byte[] data)
        {
            return Encoding.UTF8.GetString(data, StringLengthSize + MiscDataSize,
                data.Length - StringLengthSize - MiscDataSize);
        }

        public static byte[] StoreString(string value)
        {
            byte[] ret = new byte[StringLengthSize + MiscDataSize + Encoding.UTF8.GetByteCount(value)];
            // TODO 头部信息
            // 长度信息
            BitConverter.TryWriteBytes(new Span<byte>(ret, MiscDataSize, StringLengthSize), value.Length);
            // 字符串
            Encoding.UTF8.GetBytes(value, new Span<byte>(ret, 
                StringLengthSize + MiscDataSize, ret.Length - StringLengthSize - MiscDataSize));
            return ret;
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
