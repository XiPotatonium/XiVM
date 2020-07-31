using System.Collections.Generic;

namespace XiVM.Xir.Symbol
{

    public class SymbolTable
    {
        private SymbolTableFrame GlobalFrame { get; } = new SymbolTableFrame();
        /// <summary>
        /// Stack新来的在第一个
        /// </summary>
        private LinkedList<SymbolTableFrame> SymbolStack { get; } = new LinkedList<SymbolTableFrame>();
        public int Count => SymbolStack.Count;

        public SymbolTable()
        {
            SymbolStack.AddFirst(GlobalFrame);
        }

        public void Push()
        {
            SymbolStack.AddFirst(new SymbolTableFrame());
        }

        public void Pop()
        {
            SymbolStack.RemoveFirst();
        }

        public bool TryGetValue(string name, out Symbol value)
        {
            foreach (SymbolTableFrame frame in SymbolStack)
            {
                if (frame.TryGetValue(name, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool TryGetValue(string name, out Symbol value, out int levelDiff)
        {
            levelDiff = 0;
            foreach (SymbolTableFrame frame in SymbolStack)
            {
                if (frame.TryGetValue(name, out value))
                {
                    return true;
                }
                levelDiff++;
            }
            value = null;
            return false;
        }

        internal void Add(string key, Symbol value)
        {
            SymbolStack.First.Value.Add(key, value);
        }

        /// <summary>
        /// 顶层栈帧包含key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CurrentContainsKey(string key)
        {
            return SymbolStack.First.Value.ContainsKey(key);
        }
    }

    internal class SymbolTableFrame : Dictionary<string, Symbol>
    {
    }
}
