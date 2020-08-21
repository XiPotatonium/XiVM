using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XiVM.ConstantTable;
using XiVM.Errors;

namespace XiVM.Runtime
{
    internal class MethodArea : IObjectArea
    {
        public static readonly int SizeLimit = 0x1000000;

        public static MethodArea Singleton { get; } = new MethodArea();

        /// <summary>
        /// 当前占用，由于不回收，当前占用就是历史最高占用
        /// </summary>
        public int Size { private set; get; }
        public int MaxSize => Size;
        /// <summary>
        /// key是offset，因为设定上方法区对象不回收，所以不用记录内碎片
        /// </summary>
        public Dictionary<uint, HeapData> DataMap { get; } = new Dictionary<uint, HeapData>();


        public Dictionary<string, HeapData> StringPool { get; } = new Dictionary<string, HeapData>();
        public uint StringProgramAddress { private set; get; }
        public uint StringMainAddress { private set; get; }
        public uint StringMainDescriptorAddress { private set; get; }
        public uint StaticConstructorNameAddress { private set; get; }
        public uint ConstructorNameAddress { private set; get; }

        private MethodArea()
        {
            // 预先加入这些常量
            // Warning Hardcoding
            StringProgramAddress = AddConstantString("Program");
            StringMainAddress = AddConstantString("Main");
            StringMainDescriptorAddress = AddConstantString("([LSystem.String;)V");
            StaticConstructorNameAddress = AddConstantString("(sinit)");
            ConstructorNameAddress = AddConstantString("(init)");
        }

        /// <summary>
        /// 注意要和SystemLib中的String的对象格式相同
        /// </summary>
        /// <param name="value"></param>
        /// <returns>绝对地址</returns>
        public uint AddConstantString(string value)
        {
            if (!StringPool.TryGetValue(value, out HeapData data))
            {
                // 分配byte数组
                HeapData stringData = MallocCharArray(Encoding.UTF8.GetByteCount(value));
                Encoding.UTF8.GetBytes(value, new Span<byte>(stringData.Data, HeapData.ArrayLengthSize + HeapData.MiscDataSize,
                    stringData.Data.Length - HeapData.ArrayLengthSize - HeapData.MiscDataSize));

                // String对象
                byte[] vs = new byte[HeapData.MiscDataSize + HeapData.StringLengthSize + HeapData.StringDataSize];
                // 头部信息可以不填，因为MethodArea是内存的边界，GC不会继续walk
                // 长度信息
                BitConverter.TryWriteBytes(new Span<byte>(vs, HeapData.MiscDataSize, HeapData.StringLengthSize), value.Length);
                // Data信息
                BitConverter.TryWriteBytes(new Span<byte>(vs, HeapData.MiscDataSize + HeapData.StringLengthSize, HeapData.StringDataSize),
                    MemoryMap.MapToAbsolute(stringData.Offset, MemoryTag.METHOD));

                // 字符串
                data = Malloc(vs);
                StringPool.Add(value, data);
            }
            return MemoryMap.MapToAbsolute(data.Offset, MemoryTag.METHOD);
        }

        #region Mallocs

        public HeapData Malloc(int size)
        {
            if (size == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + size > SizeLimit)
            {
                throw new XiVMError("MethodArea overflow");
            }
            HeapData ret = new HeapData((uint)Size, new byte[size]);
            DataMap.Add((uint)Size, ret);
            Size += size;
            return ret;
        }

        public HeapData Malloc(byte[] data)
        {
            if (data.Length == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + data.Length > SizeLimit)
            {
                throw new XiVMError("MethodArea overflow");
            }

            HeapData ret = new HeapData((uint)Size, data);
            DataMap.Add((uint)Size, ret);
            Size += data.Length;
            return ret;
        }

        /// <summary>
        /// 构建字符串的char数组
        /// </summary>
        /// <param name="len">byte长度</param>
        /// <returns></returns>
        private HeapData MallocCharArray(int len)
        {
            int size = len * sizeof(byte) + HeapData.MiscDataSize + HeapData.ArrayLengthSize;

            HeapData ret = Malloc(size);

            // 长度信息
            BitConverter.TryWriteBytes(new Span<byte>(ret.Data, HeapData.MiscDataSize, HeapData.ArrayLengthSize), len);
            // 头部信息可以不填，因为MethodArea是内存的边界，GC不会继续walk

            return ret;
        }

        public byte[] GetData(uint addr)
        {
            if (DataMap.TryGetValue(addr, out HeapData data))
            {
                return data.Data;
            }
            else
            {
                throw new XiVMError($"Invalid MethodArea address {addr}");
            }
        }

        #endregion
    }
}
