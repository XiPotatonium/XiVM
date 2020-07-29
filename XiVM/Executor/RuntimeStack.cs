using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Executor
{
    /// <summary>
    /// 运行时堆栈，用于存储Activation Record，使用链表式堆栈
    /// </summary>
    class RuntimeStack
    {
        public RuntimeStackFrame Global { private set; get; }
        public RuntimeStackFrame Current { private set; get; }

        public void Push(int size)
        {
            if (Current == null)
            {
                Global = Current = new RuntimeStackFrame(null, size);
            }
            else
            {
                Current = new RuntimeStackFrame(Current, size);
            }
        }

        public void Pop()
        {
            if (Current == Global)
            {
                Current = Global = null;
            }
            else
            {
                Current = Current.Previous;
            }
        }
    }

    class RuntimeStackFrame
    {
        public RuntimeStackFrame Previous { private set; get; }
        public int Depth { private set; get; }
        public byte[] Data { private set; get; }

        public RuntimeStackFrame(RuntimeStackFrame previous, int size)
        {
            Previous = previous;
            Depth = previous == null ? 0 : previous.Depth + 1;
            Data = new byte[size];
        }
    }
}
