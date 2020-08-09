using System;
using XiVM.Errors;

namespace XiVM.Runtime
{
    /// <summary>
    /// 堆地址还是栈地址
    /// </summary>
    internal enum MemoryTag
    {
        STACK, HEAP, METHOD, NULL, INVALID
    }

    internal static class MemoryMap
    {
        /// <summary>
        /// Warning: HardCoding的slot
        /// </summary>
        public static readonly int ByteSize = 1;
        public static readonly int IntSize = 1;
        public static readonly int DoubleSize = 2;
        public static readonly int AddressSize = 1;


        public static readonly uint NullAddress = 0;


        /// <summary>
        /// 从addr映射到Tag空间的res
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static MemoryTag MapFrom(uint addr, out uint res)
        {
            if (addr == NullAddress)
            {
                res = 0;
                return MemoryTag.NULL;
            }

            if (addr < Stack.MaxSize)
            {
                res = addr;
                return MemoryTag.STACK;
            }

            addr = (uint)(addr - Stack.MaxSize);

            if (addr < Heap.MaxSize)
            {
                res = addr;
                return MemoryTag.HEAP;
            }

            addr = (uint)(addr - Heap.MaxSize);

            if (addr < MethodArea.MaxSize)
            {
                res = addr;
                return MemoryTag.METHOD;
            }
            else
            {
                res = uint.MaxValue;
                return MemoryTag.INVALID;
            }
        }


        public static uint MapTo(uint offset, MemoryTag to)
        {
            switch (to)
            {
                case MemoryTag.STACK:
                    if (offset >= Stack.MaxSize)
                    {
                        throw new XiVMError("Cannot map to stack space, exceeds stack max size");
                    }
                    return offset;
                case MemoryTag.HEAP:
                    if (offset >= Heap.MaxSize)
                    {
                        throw new XiVMError("Cannot map to heap space, exceeds heap max size");
                    }
                    return (uint)(offset + Stack.MaxSize);
                case MemoryTag.METHOD:
                    if (offset >= MethodArea.MaxSize)
                    {
                        throw new XiVMError("Cannot map to method area, exceeds method area max size");
                    }
                    return (uint)(offset + Stack.MaxSize + Heap.MaxSize);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
