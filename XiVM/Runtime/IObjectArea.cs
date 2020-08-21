using System.Collections.Generic;

namespace XiVM.Runtime
{
    internal interface IObjectArea
    {
        public Dictionary<uint, HeapData> DataMap { get; }
        public int Size { get; }
        public int MaxSize { get; }
        public HeapData Malloc(int size);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr">相对地址</param>
        /// <returns></returns>
        public HeapData GetData(uint addr);
    }
}
