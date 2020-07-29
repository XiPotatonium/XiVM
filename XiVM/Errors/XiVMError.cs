using System;

namespace XiVM.Errors
{
    [Serializable]
    public class XiVMError : Exception
    {
        public XiVMError(string message) : base(message)
        {
        }
    }
}
