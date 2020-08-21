// GC测试

import System;

class Program {
    static void Main(string[] args) {
        foo();
        // 可以尝试在new之后启动GC
        string s = new string("Haha1");
        // 在正确的情况下foo中的vs被回收，堆中有两个对象(别忘了System)
        args = new string[10];
        s = new string("Haha2");
        s = new string("Haha3");
        // 堆中有四个对象(Haha1被回收了，Haha2还没有，new的时候还没写回)
    }

    static void foo() {
        string[] vs = new string[20];
        for (int i = 0; i < 20; ) {
            vs[i] = new string("Teststring");
            i = i + 1;
        }
    }
}