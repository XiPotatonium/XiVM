using System.Collections.Generic;

namespace XiVM.Executor
{
    internal class Heap
    {
        private LinkedList<HeapData> RuntimeHeapData { set; get; } = new LinkedList<HeapData>();
    }

    internal class HeapData
    {
        public uint ReferenceCount { set; get; } = 1;
        public byte[] Data { private set; get; }

        public HeapData(uint size)
        {
            Data = new byte[size];
        }
    }
}
