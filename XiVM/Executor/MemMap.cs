﻿using System;
using XiVM.Errors;

namespace XiVM.Executor
{
    /// <summary>
    /// 堆地址还是栈地址
    /// </summary>
    internal enum MemTag
    {
        STACK, HEAP, NULL, INVALID
    }

    internal static class MemMap
    {
        /// <summary>
        /// Warning: HardCoding的slot
        /// </summary>
        public static readonly int ByteSize = 1;
        public static readonly int IntSize = 1;
        public static readonly int DoubleSize = 2;
        public static readonly int AddressSize = 1;


        /// <summary>
        /// 从addr映射到Tag空间的res
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static MemTag MapFrom(uint addr, out uint res)
        {
            if (addr == 0)
            {
                res = 0;
                return MemTag.NULL;
            }

            if (addr < Stack.MaxStackSize)
            {
                res = addr;
                return MemTag.STACK;
            }

            addr = (uint)(addr - Stack.MaxStackSize);

            if (addr < Heap.MaxHeapSize)
            {
                res = addr;
                return MemTag.HEAP;
            }
            else
            {
                res = uint.MaxValue;
                return MemTag.INVALID;
            }
        }


        public static uint MapTo(uint addr, MemTag to)
        {
            switch (to)
            {
                case MemTag.STACK:
                    if (addr >= Stack.MaxStackSize)
                    {
                        throw new XiVMError("Cannot map be stack space, exceeds stack max size");
                    }
                    return addr;
                case MemTag.HEAP:
                    if (addr >= Heap.MaxHeapSize)
                    {
                        throw new XiVMError("Cannot map be heap space, exceeds heap max size");
                    }
                    return (uint)(addr + Stack.MaxStackSize);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
