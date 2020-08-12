// 单元测试

import System;

class Program {

	static void Main() {
		Demo d = new Demo(1, 12);
		int x = Gcd(27, d.Foo(5));		// Gcd(27, 18)
		System.IO.Write(x);
		System.IO.PutChar(10);
	}

	static int Gcd(int a, int b) {
		if (b == 0) {
			return a;
		}
		return Gcd(b, a % b);
	}
}

class Demo {
	static int Tag;
	int Id;
	int Value = 20;

	Demo(int id, int val) {
		Id = id;
		Value = val;
	}

	int Foo(int a) {
		return Value + a + Id;
	}
}
