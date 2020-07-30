namespace XiVM.Xir.Symbol
{
    public abstract class Symbol
    {
        public string Name { private set; get; }

        internal Symbol(string name)
        {
            Name = name;
        }
    }
}
