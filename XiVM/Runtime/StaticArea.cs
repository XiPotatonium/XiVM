using System;
using System.Collections.Generic;
using System.Text;
using XiVM.Errors;

namespace XiVM.Runtime
{
    internal class StaticArea : IObjectArea
    {
        public static readonly int SizeLimit = 0x1000000;

        public static StaticArea Singleton { get; } = new StaticArea();

        public Dictionary<uint, HeapData> DataMap { get; } = new Dictionary<uint, HeapData>();

        public int Size { private set; get; }
        public int MaxSize => Size;

        private StaticArea()
        {
            Size = 0;
        }

        public HeapData Malloc(int size)
        {
            if (size == 0)
            {
                throw new XiVMError("Malloc space of size 0 is not supported");
            }
            if (Size + size > SizeLimit)
            {
                throw new XiVMError("StaticArea overflow");
            }
            HeapData ret = new HeapData((uint)Size, new byte[size]);
            DataMap.Add((uint)Size, ret);
            Size += size;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmClass"></param>
        /// <returns>绝对地址</returns>
        public uint MallocClassStaticArea(VMClass vmClass)
        {
            HeapData ret = Malloc(vmClass.StaticFieldSize);

            // TODO 头部信息暂时不知道填什么

            return MemoryMap.MapToAbsolute(ret.Offset, MemoryTag.STATIC);
        }

        public byte[] GetData(uint addr)
        {
            if (DataMap.TryGetValue(addr, out HeapData data))
            {
                return data.Data;
            }
            else
            {
                throw new XiVMError($"Invalid StaticArea address {addr}");
            }
        }
    }
}
