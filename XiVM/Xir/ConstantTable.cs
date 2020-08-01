using System.Collections.Generic;

namespace XiVM.Xir
{
    public class ConstantTable<T>
    {
        private List<T> ElementList { get; } = new List<T>();
        private Dictionary<T, int> ElementSet { get; } = new Dictionary<T, int>();

        public int Add(T element)
        {
            ElementSet.Add(element, ElementSet.Count);
            ElementList.Add(element);
            return ElementSet.Count - 1;
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
