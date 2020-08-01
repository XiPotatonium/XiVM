using System;
using XiVM.Errors;

namespace XiVM.Executor
{
    internal class Stack
    {
        public static readonly int MaxStackSize = 0x100000;
        public static readonly int MinStackSize = 0x400;
        public static readonly int MiscDataSize = 3 * sizeof(int);

        /// <summary>
        /// Frame Pointer，依据这个寻找局部变量
        /// </summary>
        public int FP { private set; get; }
        /// <summary>
        /// Stack Pointer，函数栈帧顶
        /// </summary>
        public int SP { private set; get; }

        private int Capacity { set; get; }

        private byte[] Data { set; get; }
        public bool Empty => FP < 0;

        public Stack()
        {
            Capacity = MinStackSize;
            Data = new byte[Capacity];
            FP = -1;
            SP = 0;
        }

        /// <summary>
        /// 压入函数栈帧
        /// </summary>
        /// <param name="localSize">函数局部变量大小，会预分配这些空间</param>
        /// <param name="index">Caller函数地址</param>
        /// <param name="ip">Caller IP</param>
        public void PushFrame(int localSize, int index, int ip)
        {
            int newSP = SP + localSize + 3 * sizeof(int);
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
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP, sizeof(int)), FP);
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP + sizeof(int), sizeof(int)), index);
            BitConverter.TryWriteBytes(new Span<byte>(Data, SP + 2 * sizeof(int), sizeof(int)), ip);
            FP = SP;
            SP = newSP;
        }

        public void PopFrame(int paramSize, out int index, out int ip)
        {
            if (Empty)
            {
                throw new XiVMError("Cannnot pop empty stack");
            }

            // 恢复寄存器
            int oldBP = BitConverter.ToInt32(Data, FP);
            index = BitConverter.ToInt32(Data, FP + sizeof(int));
            ip = BitConverter.ToInt32(Data, FP + 2 * sizeof(int));
            SP = FP;
            FP = oldBP;

            PopN(paramSize);
        }

        public void PushN(int n)
        {
            if (SP + n > Capacity)
            {
                byte[] old = Data;
                Capacity *= 2;
                Data = new byte[Capacity];
                System.Array.Copy(old, Data, SP);
            }
            SP += n;
        }

        public void PopN(int n)
        {
            SP -= n;
            if (SP < FP)
            {
                throw new XiVMError("Stack frame size cannot be negative");
            }

            // Shrink
            if (SP < 4 * Capacity && Capacity > MinStackSize)
            {
                byte[] old = Data;
                Capacity /= 2;
                Data = new byte[Capacity];
                System.Array.Copy(old, Data, SP);
            }
        }

        /// <summary>
        /// Push N个byte并初始化
        /// </summary>
        /// <param name="n"></param>
        /// <param name="source"></param>
        /// <param name="sourceIndex"></param>
        public void PushN(int n, System.Array source, int sourceIndex)
        {
            PushN(n);
            System.Array.Copy(source, sourceIndex, Data, SP - n, n);
        }

        /// <summary>
        /// 复制粘贴栈顶N个byte
        /// </summary>
        /// <param name="n"></param>
        public void DupN(int n)
        {
            PushN(n);
            System.Array.Copy(Data, SP - 2 * n, Data, SP - n, n);
        }

        public Span<byte> GetTopSpan(int size)
        {
            return new Span<byte>(Data, SP - size, size);
        }

        public Span<byte> GetSpan(int addr, int size)
        {
            return new Span<byte>(Data, addr, size);
        }

        public byte LoadByte(int addr)
        {
            return Data[addr];
        }

        public byte LoadTopByte()
        {
            return Data[SP - 1];
        }

        public void StoreByte(int addr, byte value)
        {
            Data[addr] = value;
        }

        public void StoreTopByte(byte value)
        {
            Data[SP - 1] = value;
        }
    }
}
