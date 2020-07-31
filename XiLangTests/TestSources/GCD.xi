int gcd(int a, int b) {
    if (b == 0) {
        return a;
    }
    return gcd(b, a % b);
}

void main(string[] args) {
    printi(gcd(36, 27));
}