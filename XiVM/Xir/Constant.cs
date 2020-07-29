using System;

namespace XiVM.Xir
{
    [Serializable]
    internal class BinaryInt
    {
        public int Value { set; get; }
    }

    [Serializable]
    internal class BinaryDouble
    {
        public double Value { set; get; }
    }

    [Serializable]
    internal class BinaryString
    {
        public string Value { set; get; }
    }
}
