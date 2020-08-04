int a = 9;

void main(string[] argv) {
    int x = a + 1;
    for (int i = 0; ; ) {
        if (i == 10) {
            break;
        }
        dispNumber(x + i);
        putc(' ');
        i = i + 1;
    }
	putc(10);
}

void dispNumber(int n) {
	if (n < 0) {
		n = -n;
		putc('-');
	} else if (n == 0) {
		return;
	}
	
	dispNumber(n / 10);
	putc(n % 10 + '0');
}
