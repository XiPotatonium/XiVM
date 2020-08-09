﻿using System.Collections.Generic;
using XiVM;

namespace XiLang.Symbol
{
    internal class SymbolTable
    {
        /// <summary>
        /// 栈顶在链表头
        /// </summary>
        public LinkedList<SymbolTableFrame> SymbolStack { get; } = new LinkedList<SymbolTableFrame>();

        public void PushFrame()
        {
            SymbolStack.AddFirst(new SymbolTableFrame());
        }

        public void PopFrame()
        {
            SymbolStack.RemoveFirst();
        }

        public bool TryGetSymbol(string id, out Variable value)
        {
            foreach (SymbolTableFrame item in SymbolStack)
            {
                if (item.TryGetValue(id, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
        }

        public void AddSymbol(string id, Variable value)
        {
            SymbolStack.First.Value.Add(id, value);
        }
    }

    public class SymbolTableFrame : Dictionary<string, Variable>
    {

    }
}
