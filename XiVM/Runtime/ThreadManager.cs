using System.Collections.Generic;

namespace XiVM.Runtime
{
    /// <summary>
    /// 具体功能待定，未设计，目前仅用于获取executor
    /// </summary>
    internal static class ThreadManager
    {
        public static List<VMExecutor> Threads { get; } = new List<VMExecutor>();

        public static VMExecutor CreateThread()
        {
            VMExecutor thread = new VMExecutor();
            Threads.Add(thread);
            return thread;
        }

        public static void CollectThreadSpace(VMExecutor thread)
        {
            Threads.Remove(thread);
        }
    }
}
