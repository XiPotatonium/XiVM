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
        private int AccessLinkValue { set; get; }

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

        /// <summary>
        /// Without Local Procedure设计
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="isGlobal">如果找到了符号，是不是在Global找到的</param>
        /// <returns></returns>
        public bool TryGetValue(string name, out Symbol value, out bool isGlobal)
        {
            isGlobal = false;
            foreach (SymbolTableFrame frame in SymbolStack)
            {
                if (frame.TryGetValue(name, out value))
                {
                    if (frame == GlobalFrame)
                    {
                        isGlobal = true;
                    }
                    return true;
                }
            }
            value = null;
            return false;
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
