// 集成测试

import System;

class Program {
	static int AValue = 9;
	static int BValue = 4 * 3;

	static void Main() {
		int x = AValue * 3;
		Demo d = new Demo(1, BValue);
		System.IO.Write(d.Foo(5));
		System.IO.PutChar(10);
		x = Gcd(x, d.Foo(5));		// Gcd(27, 18)
		System.IO.Write(x);
		System.IO.PutChar(10);
		System.IO.Write(d.Value);
		System.IO.PutChar(10);
	}

//	static void LoopTest() {
//		for (int i = 0; i < 10; ) {
//			if (i == 2) {
//				i = i + 2;
//			} else if (i == 5) {
//				i = i + 3;
//			} else {
//				i = i + 1;
//			}
//			DispNumber(AValue + i);
//			System.IO.PutChar(' ');
//		}
//		System.IO.PutChar(10);
//	}

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
