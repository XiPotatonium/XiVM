using System.Collections.Generic;
using XiVM.Errors;

namespace XiVM.ConstantTable
{
    /// <summary>
    /// Index从1开始
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConstantTable<T>
    {
        internal List<T> ElementList { get; } = new List<T>();
        internal Dictionary<T, int> ElementTable { get; } = new Dictionary<T, int>();

        /// <summary>
        /// 强行添加，如果存在，则报错
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int Add(T element)
        {
            ElementTable.Add(element, ElementTable.Count + 1);
            ElementList.Add(element);
            return ElementTable.Count;
        }

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int TryAdd(T element)
        {
            if (ElementTable.TryGetValue(element, out int ret))
            {
                return ret;
            }
            return Add(element);
        }

        public T[] ToArray()
        {
            return ElementList.ToArray();
        }

        public int GetIndex(T element)
        {
            if (!ElementTable.TryGetValue(element, out int index))
            {
                throw new XiVMError("CostantPool info not found");
            }
            return index;
        }

        public bool TryGetIndex(T element, out int index)
        {
            return ElementTable.TryGetValue(element, out index);
        }

        /// <summary>
        /// 不需要index - 1
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get(int index)
        {
            return ElementList[index - 1];
        }

        public bool Contains(T element)
        {
            return ElementTable.ContainsKey(element);
        }
    }
}
