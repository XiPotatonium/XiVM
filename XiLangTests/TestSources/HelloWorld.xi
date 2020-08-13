import System;

class Program {
	static void Main(String[] args) {
		args = new String[1 * 2];
		args[0] = "Hello ";
		args[1] = "World!";
		System.IO.Write(args[0]);
		System.IO.Write(args[1]);
		System.IO.PutChar(10);
	}
}