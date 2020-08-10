import System;

class Program {
	static int a;

	static void Main() {
//		int x = a + 3;
		int x = 12;
		for (int i = 0; ; ) {
			if (i == 10) {
				break;
			}
			DispNumber(x + i);
			System.IO.PutChar(' ');
			i = i + 1;
		}
		System.IO.PutChar(10);
		x = Gcd(x, 3 * 6);
		Program.DispNumber(x);
		System.IO.PutChar(10);
	}

	static int Gcd(int a, int b) {
		if (b == 0) {
			return a;
		}
		return Gcd(b, a % b);
	}

	static void DispNumber(int n) {
		if (n < 0) {
			n = -n;
			System.IO.PutChar('-');
		} else if (n == 0) {
			return;
		}
	
		DispNumber(n / 10);
		System.IO.PutChar(n % 10 + '0');
	}
}
