using System;
using XiVM.Errors;

namespace XiVM.Runtime
{

    internal class Stack
    {
        /// <summary>
        /// 最大1M个slot
        /// </summary>
        public static readonly int SizeLimit = 0x100000;
        /// <summary>
        /// 函数栈中用于储存调用返回信息的MiscData是FP后的3个Slot
        /// </summary>
        public static readonly int MiscDataSize = 3;

        /// <summary>
        /// Frame Pointer，依据这个寻找局部变量
        /// </summary>
        public int FP { private set; get; }
        /// <summary>
        /// Stack Pointer，函数栈帧顶
        /// </summary>
        public int SP { private set; get; }
        /// <summary>
        /// 最大堆栈占用，诊断信息
        /// </summary>
        public int MaxSP { private set; get; }

        private int Capacity { set; get; }

        public Slot[] Slots { private set; get; }
        public bool Empty => SP <= 0;

        public Stack()
        {
            Capacity = SizeLimit;
            Slots = new Slot[Capacity];
            FP = 0;
            SP = 0;
            MaxSP = 0;
        }

        #region Stack Size Modification

        /// <summary>
        /// 压入函数栈帧
        /// </summary>
        /// <param name="addr">Caller函数地址</param>
        /// <param name="ip">Caller IP</param>
        public void PushFrame(uint addr, int ip)
        {
            int oldSP = SP;
            PushInt(FP);
            PushAddress(addr);
            PushInt(ip);
            FP = oldSP;
        }

        /// <summary>
        /// 弹出函数栈
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="ip"></param>
        public void PopFrame(out uint addr, out int ip)
        {
            if (Empty)
            {
                throw new XiVMError("Cannnot pop empty stack");
            }

            // 恢复寄存器
            int oldBP = Slots[FP].Data;
            addr = (uint)Slots[FP + 1].Data;
            ip = Slots[FP + 2].Data;
            SP = FP;
            FP = oldBP;
        }

        /// <summary>
        /// 新slots的tag会默认设置为OTHER
        /// 只能用这个增加SP
        /// </summary>
        /// <param name="slots">新增的slots</param>
        private void PushN(int slots)
        {
            if (SP + slots > Capacity)
            {
                throw new XiVMError("Stack overflow");
            }

            // 默认tag为other
            for (int i = 0; i < slots; ++i)
            {
                Slots[SP + i].DataTag = SlotDataTag.OTHER;
            }
            SP += slots;

            if (SP > MaxSP)
            {
                MaxSP = SP;
            }
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <param name="n">单位为slot</param>
        private void PopN(int n)
        {
            SP -= n;
            if (SP < FP)
            {
                throw new XiVMError("Stack frame size cannot be negative");
            }
        }

        /// <summary>
        /// 复制粘贴栈顶N个slot
        /// </summary>
        /// <param name="n"></param>
        public void DupN(int n)
        {
            PushN(n);
            System.Array.Copy(Slots, SP - 2 * n, Slots, SP - n, n);
        }

        public void PushInt(int value = 0)
        {
            PushN(1);
            TopInt = value;
        }

        public int PopInt()
        {
            int ret = TopInt;
            PopN(MemoryMap.IntSize);
            return ret;
        }

        public void PushDouble(double value = 0.0)
        {
            PushN(2);
            TopDouble = value;
        }

        public double PopDouble()
        {
            double ret = TopDouble;
            PopN(MemoryMap.DoubleSize);
            return ret;
        }

        public void PushAddress(uint value = 0)
        {
            PushN(1);
            Slots[SP - 1].DataTag = SlotDataTag.ADDRESS;
            TopAddress = value;
        }

        public uint PopAddress()
        {
            uint ret = TopAddress;
            PopN(MemoryMap.AddressSize);
            return ret;
        }

        #endregion

        #region Stack Data Modification

        public int TopInt
        {
            get => Slots[SP - 1].Data;

            set => Slots[SP - 1].Data = value;
        }

        public uint TopAddress
        {
            get => (uint)Slots[SP - 1].Data;

            set => Slots[SP - 1].Data = (int)value;
        }

        /// <summary>
        /// 低位在低地址
        /// </summary>
        public long TopLong
        {
            get => (((long)Slots[SP - 1].Data) << 32) | (Slots[SP - 2].Data & 0x0ffffffffL);

            set
            {
                Slots[SP - 2].Data = (int)value;
                Slots[SP - 1].Data = (int)(value >> 32);
            }
        }

        public double TopDouble
        {
            get => BitConverter.Int64BitsToDouble(TopLong);

            set => TopLong = BitConverter.DoubleToInt64Bits(value);
        }

        public void SetValue(uint addr, int value)
        {
            if (addr >= SP)
            {
                throw new XiVMError("Invalid stack address");
            }
            Slots[addr].Data = value;
        }

        public void SetValue(uint addr, long value)
        {
            if (addr >= SP - 1)
            {
                throw new XiVMError("Invalid stack address");
            }
            Slots[addr].Data = (int)value;
            Slots[addr + 1].Data = (int)(value >> 32);
        }

        public void SetValue(uint addr, double value)
        {
            SetValue(addr, BitConverter.DoubleToInt64Bits(value));
        }

        public void SetValue(uint addr, uint value)
        {
            if (addr >= SP)
            {
                throw new XiVMError("Invalid stack address");
            }
            Slots[addr].Data = (int)value;
        }

        public byte GetByte(uint addr)
        {
            if (addr >= SP)
            {
                throw new XiVMError("Invalid stack address");
            }
            return (byte)Slots[addr].Data;
        }

        public int GetInt(uint addr)
        {
            if (addr >= SP)
            {
                throw new XiVMError("Invalid stack address");
            }
            return Slots[addr].Data;
        }

        public long GetLong(uint addr)
        {
            if (addr >= SP - 1)
            {
                throw new XiVMError("Invalid stack address");
            }
            return (((long)Slots[addr + 1].Data) << 32) | (Slots[addr].Data & 0x0ffffffffL);
        }

        public double GetDouble(uint addr)
        {
            return BitConverter.Int64BitsToDouble(GetLong(addr));
        }

        public uint GetAddress(uint addr)
        {
            if (addr >= SP)
            {
                throw new XiVMError("Invalid stack address");
            }
            return (uint)Slots[addr].Data;
        }

        #endregion


    }
}
