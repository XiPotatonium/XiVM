﻿using System;
using XiVM.Errors;

namespace XiVM.Runtime
{
    /// <summary>
    /// 堆地址还是栈地址
    /// </summary>
    internal enum MemoryTag
    {
        INVALID, NULL, PRESERVED, STACK, HEAP, STATIC, METHOD
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
        public static MemoryTag MapToOffset(uint addr, out uint res)
        {
            res = addr;
            // 0
            if (res == NullAddress)
            {
                return MemoryTag.NULL;
            }

            res--;          // 减去null
            // Warning 不要轻易改动保留区的大小，PreservedAddressTag依赖于它
            // 1-100
            if (res < Preserved.SizeLimit)
            {
                return MemoryTag.PRESERVED;
            }

            res = (uint)(res - Preserved.SizeLimit);

            if (res < Stack.SizeLimit)
            {
                return MemoryTag.STACK;
            }

            res = (uint)(res - Stack.SizeLimit);

            if (res < Heap.SizeLimit)
            {
                return MemoryTag.HEAP;
            }

            res = (uint)(res - Heap.SizeLimit);

            if (res < StaticArea.SizeLimit)
            {
                return MemoryTag.STATIC;
            }

            res = (uint)(res - StaticArea.SizeLimit);

            if (res < MethodArea.SizeLimit)
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
        /// <param name="from"></param>
        /// <returns></returns>
        public static uint MapToAbsolute(uint offset, MemoryTag from)
        {
            switch (from)
            {
                case MemoryTag.NULL:
                    return NullAddress;
                case MemoryTag.PRESERVED:
                    if (offset >= Preserved.SizeLimit)
                    {
                        throw new XiVMError("Cannot map to preserved space, exceeds max size");
                    }
                    return offset + 1;
                case MemoryTag.STACK:
                    if (offset >= Stack.SizeLimit)
                    {
                        throw new XiVMError("Cannot map to stack space, exceeds stack max size");
                    }
                    return (uint)(offset + 1 + Preserved.SizeLimit);
                case MemoryTag.HEAP:
                    if (offset >= Heap.SizeLimit)
                    {
                        throw new XiVMError("Cannot map to heap space, exceeds heap max size");
                    }
                    return (uint)(offset + 1 + Preserved.SizeLimit + Stack.SizeLimit);
                case MemoryTag.STATIC:
                    if (offset >= StaticArea.SizeLimit)
                    {
                        throw new XiVMError("Cannot map to static area, exceeds static area max size");
                    }
                    return (uint)(offset + 1 + Preserved.SizeLimit + Stack.SizeLimit + Heap.SizeLimit);
                case MemoryTag.METHOD:
                    if (offset >= MethodArea.SizeLimit)
                    {
                        throw new XiVMError("Cannot map to method area, exceeds method area max size");
                    }
                    return (uint)(offset + 1 + Preserved.SizeLimit + Stack.SizeLimit + Heap.SizeLimit + StaticArea.SizeLimit);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
