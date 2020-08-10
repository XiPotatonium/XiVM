import System;

class Program {
	static int AValue = 9;
	static int BValue = 3 * 6;

	static void Main() {
		int x = AValue * 3;
		for (int i = 0; i < 10; ) {
			if (i == 2) {
				i = i + 2;
			} else if (i == 5) {
				i = i + 3;
			} else {
				i = i + 1;
			}
			DispNumber(x + i);
			System.IO.PutChar(' ');
		}
		System.IO.PutChar(10);
		x = Gcd(x, BValue);
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
