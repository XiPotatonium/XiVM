import System;

class Program {
	static void Main(string[] args) {
		args = new System.String[1 * 2];
		args[0] = "Hello ";
		args[1] = "World!\n";
		System.String s1 = new string(args[1]);
		System.IO.Write(args[0]);
		System.IO.Write(s1);

		System.IO.Write(string.Empty);
		System.IO.PutChar(10);

		string s2 = new System.String("Hello XiVM!");
		System.IO.Write(s2);
		System.IO.PutChar(10);
	}
}