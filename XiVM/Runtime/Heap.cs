using System;
using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM.Runtime
{

    internal class Heap : IObjectArea
    {
        public static readonly int SizeLimit = 0x1000000;

        public static Heap Singleton { get; } = new Heap();


        public LinkedList<HeapData> Data { get; } = new LinkedList<HeapData>();
        public Dictionary<uint, HeapData> DataMap { get; } = new Dictionary<uint, HeapData>();
        public int Size { private set; get; }
        public int MaxSize { private set; get; }

        private Heap()
        {
            Size = 0;
            MaxSize = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectClass"></param>
        /// <returns>绝对地址</returns>
        public uint New(VMClass objectClass)
        {
            HeapData data = Malloc(objectClass.FieldSize);

            // 类型信息
            data.TypeInfo = objectClass.StaticFieldAddress;

            return MemoryMap.MapToAbsolute(data.Offset, MemoryTag.HEAP);
        }

        public HeapData Malloc(int size)
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

        private HeapData MallocArray(int elementSize, int len)
        {
            int size = len * elementSize + HeapData.MiscDataSize + HeapData.ArrayLengthSize;
            
            HeapData ret = Malloc(size);

            // 数组长度
            BitConverter.TryWriteBytes(new Span<byte>(ret.Data, HeapData.MiscDataSize, HeapData.ArrayLengthSize), len);
            // GC的数组tag
            BitConverter.TryWriteBytes(new Span<byte>(ret.Data, sizeof(uint), sizeof(uint)), (uint)GCTag.ArrayMark);

            return ret;
        }

        /// <summary>
        /// 对象数组
        /// </summary>
        /// <param name="objectClass"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public uint MallocArray(VMClass objectClass, int len)
        {
            HeapData ret = MallocArray(VariableType.AddressType.Size, len);

            ret.TypeInfo = objectClass.StaticFieldAddress;

            return MemoryMap.MapToAbsolute(ret.Offset, MemoryTag.HEAP);
        }

        /// <summary>
        /// 基础类型数组
        /// </summary>
        /// <param name="typeTag"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public uint MallocArray(VariableTypeTag typeTag, int len)
        {
            HeapData ret = MallocArray(VariableType.GetSize(typeTag), len);

            ret.TypeInfo = (uint)typeTag;

            return MemoryMap.MapToAbsolute(ret.Offset, MemoryTag.HEAP);
        }

        public byte[] GetData(uint addr)
        {
            if (DataMap.TryGetValue(addr, out HeapData data))
            {
                return data.Data;
            }
            else
            {
                throw new XiVMError($"Invalid Heap address {addr}");
            }
        }
    }
}
