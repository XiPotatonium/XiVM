using System;
using XiVM.Errors;

namespace XiVM.Runtime
{
    /// <summary>
    /// 堆地址还是栈地址
    /// </summary>
    internal enum MemoryTag
    {
        INVALID, NULL, PRESERVED, STACK, HEAP, METHOD
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
        /// 目前未使用
        /// </summary>
        public static readonly int PreservedSpace = 99;


        /// <summary>
        /// 从addr映射到Tag空间的res
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static MemoryTag MapToOffset(uint addr, out uint res)
        {
            res = addr;
            // 0
            if (res == NullAddress)
            {
                return MemoryTag.NULL;
            }

            res--;          // 减去null
            // 1-100
            if (res < PreservedSpace)
            {
                return MemoryTag.PRESERVED;
            }

            res = (uint)(res - PreservedSpace);

            if (res < Stack.MaxSize)
            {
                return MemoryTag.STACK;
            }

            res = (uint)(res - Stack.MaxSize);

            if (res < Heap.MaxSize)
            {
                return MemoryTag.HEAP;
            }

            res = (uint)(res - Heap.MaxSize);

            if (res < MethodArea.MaxSize)
            {
                return MemoryTag.METHOD;
            }
            else
            {
                res = uint.MaxValue;
                return MemoryTag.INVALID;
            }
        }


        /// <summary>
        /// 映射到绝对空间
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static uint MapToAbsolute(uint offset, MemoryTag to)
        {
            switch (to)
            {
                case MemoryTag.NULL:
                    return NullAddress;
                case MemoryTag.PRESERVED:
                    if (offset >= PreservedSpace)
                    {
                        throw new XiVMError("Cannot map to preserved space, exceeds max size");
                    }
                    return offset + 1;
                case MemoryTag.STACK:
                    if (offset >= Stack.MaxSize)
                    {
                        throw new XiVMError("Cannot map to stack space, exceeds stack max size");
                    }
                    return (uint)(offset + 1 + PreservedSpace);
                case MemoryTag.HEAP:
                    if (offset >= Heap.MaxSize)
                    {
                        throw new XiVMError("Cannot map to heap space, exceeds heap max size");
                    }
                    return (uint)(offset + 1 + PreservedSpace + Stack.MaxSize);
                case MemoryTag.METHOD:
                    if (offset >= MethodArea.MaxSize)
                    {
                        throw new XiVMError("Cannot map to method area, exceeds method area max size");
                    }
                    return (uint)(offset + 1 + PreservedSpace + Stack.MaxSize + Heap.MaxSize);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
