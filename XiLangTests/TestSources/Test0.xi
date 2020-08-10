import System;

class Program {
	static int a;

	static void Main() {
//		int x = a + 3;
		int x = 12;
//		for (int i = 0; ; ) {
//			if (i == 10) {
//				break;
//			}
//			dispNumber(x + i);
//			putc(' ');
//			i = i + 1;
//		}
//		putc(10);
		x = gcd(x, 3 * 9);
		System.IO.PutChar('A');
		System.IO.PutChar(10);
	}

	static int gcd(int a, int b) {
		if (b == 0) {
			return a;
		}
		return gcd(b, a % b);
	}

//	static void DispNumber(int n) {
//		if (n < 0) {
//			n = -n;
//			putc('-');
//		} else if (n == 0) {
//			return;
//		}
//	
//		dispNumber(n / 10);
//		putc(n % 10 + '0');
//	}
}
