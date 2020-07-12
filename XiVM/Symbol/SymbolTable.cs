using System.Collections.Generic;

namespace XiVM.Symbol
{
    internal class GlobalOnlySymbolTable<T>
        where T : Symbol
    {
        protected SymbolTableFrame<T> GlobalFrame { get; } = new SymbolTableFrame<T>();

        public virtual bool TryGet(string name, out T value)
        {
            return GlobalFrame.TryGet(name, out value);
        }
    }

    internal class SymbolTable<T> : GlobalOnlySymbolTable<T>
        where T : Symbol
    {
        /// <summary>
        /// 注意Stack是倒着的，新来的在第一个
        /// </summary>
        private LinkedList<SymbolTableFrame<T>> SymbolStack { get; } = new LinkedList<SymbolTableFrame<T>>();

        private SymbolTable()
        {
            SymbolStack.AddFirst(GlobalFrame);
        }

        public void Push()
        {
            SymbolStack.AddFirst(new SymbolTableFrame<T>());
        }

        public void Pop()
        {
            SymbolStack.RemoveFirst();
        }

        public override bool TryGet(string name, out T value)
        {
            foreach (var frame in SymbolStack)
            {
                if (frame.TryGet(name, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
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
    }
}
