using System;

namespace XiVM.Executor
{
    internal class ComputationStack
    {
        public byte[] Data { private set; get; }
        public int Capacity { private set; get; }
        public int Size { private set; get; } = 0;

        public ComputationStack()
        {
            Capacity = 64;
            Data = new byte[Capacity];
        }

        public void Push(int n)
        {
            if (Size + n > Capacity)
            {
                byte[] old = Data;
                Capacity *= 2;
                Data = new byte[Capacity];
                Array.Copy(old, Data, Size);
            }
            Size += n;
        }

        public void Pop(int n)
        {
            Size -= n;
            if (Size < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (Size < 3 * Capacity)
            {
                byte[] old = Data;
                Capacity /= 2;
                Data = new byte[Capacity];
                Array.Copy(old, Data, Size);
            }
        }
    }
}
