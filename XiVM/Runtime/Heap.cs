using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM.Runtime
{

    internal static class Heap
    {
        public static readonly int SizeLimit = 0x1000000;


        private static LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();
        private static Dictionary<uint, HeapData> DataMap { get; } = new Dictionary<uint, HeapData>();
        internal static int Size { private set; get; } = 0;
        internal static int MaxSize { private set; get; } = 0;

        public static HeapData Malloc(int size)
        {
            if (size == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + size > SizeLimit)
            {
                throw new XiVMError("Heap overflow");
            }

            // Best Fit
            LinkedListNode<HeapData> best = null;
            int bestFragmentSize = 0;
            LinkedListNode<HeapData> cur = Data.First;
            while (cur != null)
            {
                if (cur.Next != null)
                {
                    int fragmentSize = (int)(cur.Next.Value.Offset - (cur.Value.Offset + cur.Value.Data.Length));
                    if (fragmentSize >= size)
                    {
                        // 可以填入
                        if (best == null || bestFragmentSize > fragmentSize)
                        {
                            // best fit
                            best = cur;
                            bestFragmentSize = fragmentSize;
                        }
                    }
                }

                cur = cur.Next;
            }

            HeapData ret = null;
            if (best == null)
            {
                // 未找到内碎片，在末尾添加
                ret = new HeapData(
                    Data.Count == 0 ? 0 : Data.Last.Value.Offset + (uint)Data.Last.Value.Data.Length,
                    new byte[size]);
                Data.AddLast(ret);
            }
            else
            {
                // fit
                ret = new HeapData(best.Value.Offset + (uint)best.Value.Data.Length,
                    new byte[size]);
                Data.AddAfter(best, ret);

            }

            DataMap.Add(ret.Offset, ret);
            Size += size;
            if (Size > MaxSize)
            {
                MaxSize = size;
            }

            return ret;
        }

        /// <summary>
        /// TODO 数组的length信息
        /// </summary>
        /// <param name="elementSize"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static HeapData MallocArray(int elementSize, int len)
        {
            int size = len * elementSize + HeapData.MiscDataSize + HeapData.ArrayLengthSize;

            HeapData ret = Malloc(size);
            return ret;
        }

        public static byte[] GetData(uint addr)
        {
            if (DataMap.TryGetValue(addr, out HeapData data))
            {
                return data.Data;
            }
            else
            {
                throw new XiVMError($"Invalid heap area addr {addr}");
            }
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
