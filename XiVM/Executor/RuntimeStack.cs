using System;
using XiVM.Errors;

namespace XiVM.Executor
{
    internal class RuntimeStack
    {
        public static readonly int MaxStackSize = 0x100000;

        private int BP { set; get; }
        private int SP { set; get; }
        private int Capacity { set; get; }

        public byte[] Data { private set; get; }
        public bool Empty => BP < 0;

        public RuntimeStack()
        {
            Capacity = 1024;
            Data = new byte[Capacity];
            BP = -1;
            SP = 0;
        }

        public void Push(int size, int index, int ip)
        {
            int newSP = SP + size + 3 * sizeof(int);
            if (newSP > Capacity)
            {
                if (Capacity * 2 > MaxStackSize)
                {
                    throw new XiVMError($"Maximum stack size ({MaxStackSize}) exceeded, wants {Capacity * 2}");
                }
                byte[] newData = new byte[Capacity * 2];
                System.Array.Copy(Data, newData, SP);
                Data = newData;
            }
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP, sizeof(int)), BP);
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP + sizeof(int), sizeof(int)), index);
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP + 2 * sizeof(int), sizeof(int)), ip);
            BP = SP;
            SP = newSP;
        }

        public bool Pop(out int index, out int ip)
        {
            index = ip = 0;
            if (Empty)
            {
                return false;
            }

            int oldBP = BitConverter.ToInt32(Data, BP);
            index = BitConverter.ToInt32(Data, BP + sizeof(int));
            ip = BitConverter.ToInt32(Data, BP + 2 * sizeof(int));
            SP = BP;
            BP = oldBP;

            // TODO 可以有Shrink吗

            return true;
        }

        public int GetLocalIndex(int offset)
        {
            return BP + 3 * sizeof(int) + offset;
        }

        public int GetGlobalIndex(int offset)
        {
            return 3 * sizeof(int) + offset;
        }
    }
}
