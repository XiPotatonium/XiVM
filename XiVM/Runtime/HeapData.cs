using System;

namespace XiVM.Runtime
{
    internal enum GCTag
    {
        GCMark = 1 << 1,        // 可达为1
        ArrayMark = 1 << 2,     // 是数组为1
    }


    internal class HeapData
    {
        /// <summary>
        /// 类型信息以及GC信息
        /// </summary>
        public static readonly int MiscDataSize = 2 * sizeof(uint);
        /// <summary>
        /// 字符串头部长度信息
        /// </summary>
        public static int StringLengthSize = sizeof(int);
        /// <summary>
        /// 字符串指向的数组
        /// </summary>
        public static int StringDataSize = sizeof(uint);
        /// <summary>
        /// 数组头部长度信息
        /// </summary>
        public static int ArrayLengthSize = sizeof(int);

        public static int ArrayOffsetMap(int elementSize, int index)
        {
            return index * elementSize + MiscDataSize + ArrayLengthSize;
        }

        /// <summary>
        /// 这个记录的是到空间开头的offset，是相对地址而不是绝对地址
        /// </summary>
        public uint Offset { private set; get; }
        public byte[] Data { private set; get; }

        public uint TypeInfo
        {
            set => BitConverter.TryWriteBytes(Data, value);
            get => BitConverter.ToUInt32(Data, 0);
        }
        public uint GCInfo
        {
            set => BitConverter.TryWriteBytes(new Span<byte>(Data, sizeof(uint), sizeof(uint)), value);
            get => BitConverter.ToUInt32(Data, sizeof(uint));
        }

        public HeapData(uint offset, byte[] data)
        {
            Offset = offset;
            Data = data;
        }
    }
}
