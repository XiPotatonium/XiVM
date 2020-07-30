using System.Collections.Generic;

namespace XiVM.Executor
{
    internal class RuntimeHeap
    {
        private LinkedList<RuntimeHeapData> RuntimeHeapData { set; get; } = new LinkedList<RuntimeHeapData>();
    }

    internal class RuntimeHeapData
    {
        public uint ReferenceCount { set; get; } = 1;
        public byte[] Data { private set; get; }

        public RuntimeHeapData(uint size)
        {
            Data = new byte[size];
        }
    }
}
