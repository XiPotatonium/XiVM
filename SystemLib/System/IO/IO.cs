namespace SystemLib.System.IO
{
    public class IO : AbstractClass
    {
        public static readonly string ClassName = "IO";

        internal IO() : base(ClassName)
        {
        }

        internal override void DeclarationGen()
        {
            Methods.Add(new PutChar(this));
            Methods.Add(new WriteInt(this));
            Methods.Add(new WriteString(this));
        }

        internal override void StaticInitializerGen()
        {
            Constructor.AddRet();
        }
    }
}
