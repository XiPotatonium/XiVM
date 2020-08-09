using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.ConstantTable
{
    /// <summary>
    /// Index从1开始
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConstantTable<T>
    {
        internal List<T> ElementList { get; } = new List<T>();
        internal Dictionary<T, int> ElementSet { get; } = new Dictionary<T, int>();

        /// <summary>
        /// 强行添加，如果存在，则报错
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int Add(T element)
        {
            ElementSet.Add(element, ElementSet.Count + 1);
            ElementList.Add(element);
            return ElementSet.Count;
        }

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int TryAdd(T element)
        {
            if (ElementSet.TryGetValue(element, out int ret))
            {
                return ret;
            }
            return Add(element);
        }

        public T[] ToArray()
        {
            return ElementList.ToArray();
        }

        public bool TryGetIndex(T element, out int index)
        {
            return ElementSet.TryGetValue(element, out index);
        }
    }
}
