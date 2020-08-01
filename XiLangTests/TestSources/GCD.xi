/// ´òÓ¡int
void dispNumber(int n) {
	if (n < 0) {
		n = -n;
		putchar('-');
	} else if (n == 0) {
		return;
	}
	
	dispNumber(n / 10);
	putchar(n % 10 + '0');
}

void main(string[] args) {
    dispNumber(gcd(36, 27));
	putchar(10);	// »»ĞĞ
}

int gcd(int a, int b) {
    if (b == 0) {
        return a;
    }
    return gcd(b, a % b);
}
