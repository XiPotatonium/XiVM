using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Runtime
{
    internal static class GarbageCollector
    {
        public static void GC(Stack stack)
        {
            CollectFromStack(stack);
            CollectFromMethodArea();

            // 回收
            LinkedListNode<HeapData> cur = Heap.Singleton.Data.First;
            while(cur != null)
            {
                if ((cur.Value.GCInfo & (uint)GCTag.GCMark) == 0)
                {
                    // 不可达对象，删除
                    LinkedListNode<HeapData> tmp = cur;
                    cur = cur.Next;
                    Heap.Singleton.DataMap.Remove(tmp.Value.Offset);
                    Heap.Singleton.Data.Remove(tmp);
                }
                else
                {
                    // 清除GCMark
                    cur.Value.GCInfo = cur.Value.GCInfo & (~(uint)GCTag.GCMark);
                    cur = cur.Next;
                }
            }
        }

        private static void CollectFromStack(Stack stack)
        {
            uint index = 0;
            foreach (var slot in stack.Slots)
            {
                if (slot.DataTag == SlotDataTag.ADDRESS)
                {
                    MarkObject(stack.GetAddress(index));
                }
                ++index;
            }
        }

        private static void CollectFromMethodArea()
        {

        }

        private static void MarkObject(uint addr)
        {

        }
    }
}
