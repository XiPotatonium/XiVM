using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Executor
{
    /// <summary>
    /// 堆地址还是栈地址
    /// </summary>
    internal enum MemTag
    {
        STACK, HEAP, INVALID
    }

    internal static class MemMap
    {
        /// <summary>
        /// 从addr映射到Tag空间的res
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static MemTag Map(uint addr, out uint res)
        {
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



    }
}
