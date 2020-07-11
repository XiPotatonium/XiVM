namespace XiLang.Symbol
{
    public abstract class Symbol
    {
        public string Name { private set; get; }

        public Symbol(string name)
        {
            Name = name;
        }
    }
}
