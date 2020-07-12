namespace XiVM.Symbol
{
    internal abstract class Symbol
    {
        public string Name { private set; get; }

        public Symbol(string name)
        {
            Name = name;
        }
    }
}
