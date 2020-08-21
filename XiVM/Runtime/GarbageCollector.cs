using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XiVM.Runtime
{
    internal static class GarbageCollector
    {
        private static Stopwatch GCWatch { get; } = new Stopwatch();
        public static long GCTotalTime { private set; get; } = 0;
        public static long GCMaxTime { private set; get; } = 0;
        public static int GCCount { private set; get; } = 0;
        /// <summary>
        /// 注意它的单位是MB
        /// </summary>
        public static double FreedSize { private set; get; } = 0;

        public static void CollectGarbage()
        {
            GCWatch.Start();

            // 从stack出发
            foreach (var thread in ThreadManager.Threads)
            {
                // 要保证GC执行期间线程全部停止
                for (uint i = 0; i < thread.Stack.SP; ++i)
                {
                    if (thread.Stack.Slots[i].DataTag == SlotDataTag.ADDRESS)
                    {
                        MarkObject(thread.Stack.GetAddress(i));
                    }
                }
            }

            // 从类的静态区出发
            foreach (var staticClassData in StaticArea.Singleton.DataMap.Values)
            {
                uint addr = MemoryMap.MapToAbsolute(staticClassData.Offset, MemoryTag.STATIC);
                ModuleLoader.Classes.TryGetValue(addr, out VMClass vmClass);
                foreach (var staticField in vmClass.StaticFields)
                {
                    if (staticField.Type.Tag == VariableTypeTag.ADDRESS)
                    {
                        // 是地址
                        MarkObject(BitConverter.ToUInt32(staticClassData.Data, staticField.Offset));
                    }
                }
            }

            // 回收
            LinkedListNode<HeapData> cur = Heap.Singleton.Data.First;
            while (cur != null)
            {
                if ((cur.Value.GCInfo & (uint)GCTag.GCMark) == 0)
                {
                    // 不可达对象，删除
                    LinkedListNode<HeapData> tmp = cur;
                    cur = cur.Next;
                    Heap.Singleton.DataMap.Remove(tmp.Value.Offset);
                    Heap.Singleton.Data.Remove(tmp);
                    Heap.Singleton.Size -= tmp.Value.Data.Length;
                    FreedSize += tmp.Value.Data.Length / 1024;
                }
                else
                {
                    // 清除GCMark
                    cur.Value.GCInfo = cur.Value.GCInfo & (~(uint)GCTag.GCMark);
                    cur = cur.Next;
                }
            }

            GCWatch.Stop();
            GCTotalTime += GCWatch.ElapsedMilliseconds;
            if (GCWatch.ElapsedMilliseconds > GCMaxTime)
            {
                GCMaxTime = GCWatch.ElapsedMilliseconds;
            }
            GCCount++;
        }

        private static void MarkObject(uint addr)
        {
            MemoryTag tag = MemoryMap.MapToOffset(addr, out addr);
            if (tag == MemoryTag.HEAP)
            {
                HeapData data = Heap.Singleton.GetData(addr);
                data.GCInfo |= (uint)GCTag.GCMark;
                uint typeInfo = data.TypeInfo;
                if ((data.GCInfo & (uint)GCTag.ArrayMark) != 0)
                {
                    // 是数组
                    if (typeInfo == (uint)VariableTypeTag.ADDRESS)
                    {
                        // 是地址数组
                        int len = BitConverter.ToInt32(data.Data, HeapData.MiscDataSize);
                        for (int i = 0; i < len; i++)
                        {
                            MarkObject(BitConverter.ToUInt32(data.Data, HeapData.ArrayOffsetMap(VariableType.AddressType.Size, i)));
                        }
                    }
                }
                else
                {
                    // 是普通对象
                    ModuleLoader.Classes.TryGetValue(typeInfo, out VMClass vmClass);
                    foreach (var field in vmClass.Fields)
                    {
                        if (field.Type.Tag == VariableTypeTag.ADDRESS)
                        {
                            // 是地址
                            MarkObject(BitConverter.ToUInt32(data.Data, field.Offset));
                        }
                    }
                }
            }
        }
    }
}
