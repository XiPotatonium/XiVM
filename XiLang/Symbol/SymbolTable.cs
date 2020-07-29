using System.Collections.Generic;

namespace XiLang.Symbol
{

    internal class SymbolTable<T> where T : Symbol
    {
        private SymbolTableFrame<T> GlobalFrame { get; } = new SymbolTableFrame<T>();
        private LinkedList<SymbolTableFrame<T>> SymbolStack { get; } = new LinkedList<SymbolTableFrame<T>>();

        public SymbolTable()
        {
            SymbolStack.AddLast(GlobalFrame);
        }

        public void Push()
        {
            SymbolStack.AddLast(new SymbolTableFrame<T>());
        }

        public void Pop()
        {
            SymbolStack.RemoveLast();
        }

        public bool TryGet(string name, out T value)
        {
            foreach (SymbolTableFrame<T> frame in SymbolStack)
            {
                if (frame.TryGet(name, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
        }

        public void Add(string key, T value)
        {
            SymbolStack.Last.Value.Add(key, value);
        }
    }

    internal class SymbolTableFrame<T>
        where T : Symbol
    {
        private Dictionary<string, T> Symbols;

        public bool TryGet(string name, out T value)
        {
            return Symbols.TryGetValue(name, out value);
        }

        public void Add(string key, T value)
        {
            Symbols.Add(key, value);
        }
    }
}
