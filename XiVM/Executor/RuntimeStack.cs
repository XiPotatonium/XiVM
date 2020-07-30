namespace XiVM.Executor
{
    /// <summary>
    /// 运行时堆栈，用于存储Activation Record，使用链表式堆栈
    /// </summary>
    internal class RuntimeStack
    {
        public RuntimeStackFrame Global { private set; get; }
        public RuntimeStackFrame Current { private set; get; }

        public void Push(int size, uint ip)
        {
            if (Current == null)
            {
                Global = Current = new RuntimeStackFrame(null, size, ip);
            }
            else
            {
                Current = new RuntimeStackFrame(Current, size, ip);
            }
        }

        public uint Pop()
        {
            uint ip = Current.IP;
            if (Current == Global)
            {
                Current = Global = null;
            }
            else
            {
                Current = Current.Previous;
            }
            return ip;
        }
    }

    internal class RuntimeStackFrame
    {
        public RuntimeStackFrame Previous { private set; get; }
        public int Depth { private set; get; }
        public byte[] Data { private set; get; }
        public uint IP { private set; get; }

        public RuntimeStackFrame(RuntimeStackFrame previous, int size, uint ip)
        {
            Previous = previous;
            Depth = previous == null ? 0 : previous.Depth + 1;
            Data = new byte[size];
            IP = ip;
        }
    }
}
