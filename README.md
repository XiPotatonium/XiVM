# XiVM

## T: TODO

* 未定义构造函数要生成一个默认的构造函数
* 复杂条件表达式，注意短路作用;三元运算符
* 似乎没有必要区分NEWARR和NEWAARR，XiVM不需要从这里知道是什么类型
* XiLang中char字面量和string字面量中的转义字符问题
* XiVM的浮点数运算
* 目前import都是线性依赖的，互相依赖的模块还没有实现.(其实只有共同编译未实现，模块的加载、链接两个pass以及编译的类声明、类成员声明、类成员定义三个pass都做好了)
* ref
* 为了支持动态绑定的函数调用，可能需要另一种Call；为了支持类似中断的call，需要支持call一个地址
* Main函数的参数
* 多线程

---

## L: XiLang

* 使用正则表达式实现词法分析
* 使用TopDown Parser实现语法分析
* 对常量表达式提前估值
* 生成AST并可以格式化为Json
* 超级超级强类型，禁止隐式类型转换

### L.1: 实现列表

* 类型
    * 布尔：bool
    * 整数：32位整数int
    * 浮点：64位浮点数double
    * 数组
    * 其他：void, string(是System.String的别名)
* 运算符
    * 单目运算符
    * 双目运算符
* 注释：C++风格注释
* 字面量
    * 整数：十进制，十六进制
    * 浮点数：十进制
    * 字符串，字符
* 分支
    * if语句，then和otherwise必须被花括号包围，otherwise可以是另一个if
* 循环
    * while，body必须被花括号包围，或者是空(分号)
    * for：可以在init中定义变量，body必须被花括号包围，或者是空
* 类
    * 静态和非静态域
    * 静态和非静态方法：支持重载
* import，目前仅支持线性依赖，会自动查找同一文件夹下的.xibc。

### L.2: Grammar

```
// Stmt
Program
    (ImportStmt)* (ClassStmt)*
ImportStmt
    IMPORT ID SEMICOLON
ClassStmt
    CLASS ID LBRACES DeclarationStmt* RBRACES
DeclarationStmt
    ACCESS_FLAG* TypeExpr FuncDeclarator BlockStmt
    ACCESS_FLAG* TypeExpr VarDeclarator SEMICOLON
FuncDeclarator
    ID? LPAREN ParamsAST RPAREN
VarDeclarator
    ID (ASSIGN Expr)? (COMMA VarDeclarator)?
ParamsAST
    Params*
Params
    TypeExpr ID (COMMA Params)?
Stmt
    LoopStmt | IfStmt | JumpStmt | BlockStmt | VarOrExprStmt
BlockStmt
    LBRACES Stmt* RBRACES
VarOrExprStmt
    VarStmt | ExprStmt
LoopStmt
    WHILE LPAREN Expr RPAREN (SELICOLON | BlockStmt)
    FOR LPAREN (VarOrExprStmt | SEMICOLON) Expr? SEMICOLON ExprList? RPAREN (SELICOLON | BlockStmt)
IfStmt
    IF LPAREN Expr RPAREN BlockStmt (ELSE (BlockStmt | IfStmt))?
JumpStmt
    (CONTINUE | BREAK | RETURN (ListExpr)?) SEMICOLON
ExprStmt
    ListExpr SEMICOLON

// Expr
TypeExpr
    (ANY_TYPE | ID (DOT ID)*) (LBRACKET ConditionalExpr? RBRACKET)?
ListExpr
    Expr [COMMA Expr]*
Expr
    (ConditionalExpr | ID) (ANY_ASSIGN Expr)?
ConditionalExpr
    LogicalOrExpr (QUESTION Expr COLON ConditionalExpr)?
LogicalOrExpr
    LogicalAndExpr (LOG_OR LogicalAndExpr)*
LogicalAndExpr
    BitOrExpr (LOG_AND BitOrExpr)*
BitOrExpr
    BitXorExpr (BIT_OR BitXorExpr)*
BitXorExpr
    BitAndExpr (BIT_XOR BitAndExpr)*
BitAndExpr
    EqExpr (BIT_AND EqExpr)*
EqExpr
    CompExpr ((EQ | NE) CompExpr)*
CompExpr
    ShiftExpr ((LE | LT | GE | GT) ShiftExpr)*
ShiftExpr
    AddExpr ((BIT_SL | BIT_SR) AddExpr)*
AddExpr
    MulExpr ((ADD | SUB) MulExpr)*
MulExpr
    CastExpr ((DIV | MUL | MOD) CastExpr)*
CastExpr
    LPAREN TypeExpr RPAREN CastExpr
    UnaryExpr
UnaryExpr
    (LOG_NOT | SUB) CastExpr | (INC | DEC) UnaryExpr | CallExpr
CallExpr
    PrimaryExpr (LPAREN (Expr (COMMA Expr)*)? RPAREN | DOT ID | LBRACKET Expr RBRACKET)*
PrimaryExpr
    ConstExpr | ID | LPAREN Expr RPAREN | BASE DOT ID | NEW TypeExpr
ConstExpr
    TRUE | FALSE | NULL | DEC_LITERAL | HEX_LITERAL | FLOAT_LITERAL | STR_LITERAL
```

---

## VM: XiVM

* 栈式虚拟机
* Stack Based Runtime Environment without Local Procedure
* 支持生成.xibc字节码和.xir文本中间码，字节码参考了JVM
* 字节码生成API设计参考了LLVM-C，使用BasicBlock的思想，代码中unreachable code不会再生成
* 设计了保留区地址，这些特殊地址类似计算机组成中CPU操纵外设中的外设地址。这些特殊功功能弥补了虚拟机指令集的low coverage的问题。
* XiVM一般不对类型进行检查，在编译期间要保证操作（特别是非基本类型）的有效性
* 实现了简单的GC算法，Mark&Sweep。因为普通Mark&Sweep会有内碎片，因此堆内存分配是Best Fit
* diagnose参数输出诊断信息，包括：
    * 模块总加载时间和依赖模块加载时间
    * 静态构造执行时间和Main函数执行时间
    * Main线程堆栈/堆空间/静态区/方法区的内存占用情况

### VM.1: 指令集

以下说明中的T为类型，可以是B(byte 1字节，1Slot), I(int 4字节，1Slot), D(double 8字节，2Slots), A(addr 4字节，1Slot)；
N为大小（单位为Slot）。
注意计算栈中的byte, int, double, addr均指对应类型的slot。
byte和int的slot实际上不区分，所以byte类型指令和int类型指令在处理栈上数据是完全一样的。

单个指令的使用说明格式(具体编码见enum InstructionType): 
```[指令] ([参数(参数的C#类型)] ...)```

#### NOP

* NOP

 无任何操作

#### DUP(N)

* DUP
* DUP2

将计算栈中大小为N的空间复制并Push进计算栈

```
... | value(N) |
... | value(N) | value(N) |
```

#### PUSHT

* PUSHB value(byte)
* PUSHI value(int)
* PUSHD value(double)
* PUSHA value(uint)

将参数value push进计算栈

```
... |
... | value(T) |
```

#### POP(N)

* POPB
* POPI
* POPD
* POPA

将栈中指定类型的数据Pop出去

```
... | value(T) |
... |
```

#### LOADT

* LOADB
* LOADI
* LOADD
* LOADA

value = *src

```
... | src(addr) |
... | value(T) |
```

#### STORET

* STOREB
* STOREI
* STORED
* STOREA

*dest = value

```
... | value(T) | dest(addr) |
... | value(T) |
```

#### ALOADT

* ALOADB
* ALOADI
* ALOADD
* ALOADA

value = arr[index]

```
... | arr(addr) | index(int) |
... | value(T) |
```

#### ASTORET

* ASTOREB
* ASTOREI
* ASTORED
* ASTOREA

arr[index] = value

```
... | value(T) | arr(addr) | index(int) |
... | value(T) |
```

#### ADDT

* ADDI

加法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### SUBT

* SUBI

减法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### MULT

* MULI

乘法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### DIVT

* DIVI

除法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### MOD

* MOD

模运算

```
... | lhs(int) | rhs(int) |
... | res(int) |
```

#### NEGT

* NEGI

取反

```
... | value(T) |
... | res(T) |
```

#### TIN2TOUT

* I2D
* D2I

类型转换

```
... | value(TIN) |
... | res(TOUT) |
```

#### SET(COND)(T)

* SETEQI
* SETNEI
* SETLTI
* SETLEI
* SETGTI
* SETGEI

比较。如果cond成立，res为1，否则res为1

```
... | lhs(T) | rhs(T) |
... | res(byte) |
```

#### JMP

* JMP target(int)

栈不改变。IP = target

#### JCOND

* JCOND if-target(int) else-target(int) 0X81

如果cond不为0，IP = if-target，否则，IP = else-target

```
... | cond(byte) |
... |
```

#### CALL

* CALL index(int)

Index是模块常量池中MethodPool的index，
行为参考发起调用(VM.2.2: 发起调用)

#### RET(T)

* RET

行为参考函数返回(VM.2.5: 函数返回)

#### LOCAL

* LOCALA offset(int)

获取当前栈帧的某个栈地址并Push进计算栈。offset的为目标地址与FP的距离

```
... |
... | res(addr) |
```

#### CONST

* CONST index(int)

获取当前Module字符串常量池中下标为index的字符串常量的地址.
字符串常量参考字符串常量池(VM.9.2: 字符串常量池)

```
... |
... | res(addr) |
```

#### STORESTATIC

* STORESTATIC index(int)

保存value到当前Module的Field常量池中下标为index的static field中。
XiVM会根据field的类型处理value（不在栈上判断value的type），因此所有赋值判断要在编译阶段完成。

```
... | value(T) |
... | value(T) |
```

#### LOADSTATIC

* LOADSTATIC index(int)

读取当前Module的Field常量池中下标为index的static field的value。
XiVM会根据field的类型读取value.

```
... |
... | value(T) |
```

#### STORENONSTATIC

* STORENONSTATIC index(int)

保存value到当前Module的Field常量池中下标为index的field中。
XiVM会根据field的类型处理value（不在栈上判断value的type），因此所有赋值判断要在编译阶段完成。

```
... | value(T) | object(addr) |
... | value(T) |
```

#### LOADNONSTATIC

* LOADNONSTATIC index(int)

读取object的当前Module的Field常量池中下标为index的field的value。
XiVM会根据field的类型读取value.XiVM不检查object是否有这个field，在编译时检查.

```
... | object(addr) |
... | value(T) |
```

#### NEW

* NEW index(int)

在堆上分配一个当前Module的Class常量池中下标为index的类的对象，返回对象地址

```
... |
... | object(addr) |
```

#### NEWARR

* NEW tag(byte)

在堆上分配一个长度为len的基本类型数组，类型为tag，返回数组地址

```
... | len(int) |
... | arr(addr) |
```

#### NEWAARR

* NEW index(int)

在堆上分配一个长度为len的对象数组，对象的类是当前Module的Class常量池中下标为index的类，返回数组地址

```
... | len(int) |
... | aarr(addr) |
```


### VM.2: 函数调用规范

#### VM.2.1: 参数传递

函数Call之前，计算栈如下

```
... | ... | arg0 | ... | argN |
    ^                         ^
    FP                        SP
```

#### VM.2.2: 发起调用

Call执行之后，会创建函数栈帧，局部变量空间会被创建，修改FP和SP。

```
... | arg0 | ... | argN | MiscData | ...Local Vars... |
                        ^                             ^
                        FP                            SP
```

栈帧的Misc数据用于Ret时恢复堆栈。包括如下内容，OldBP是Call之前的FP，Caller的Index和OldIP用于找到Call指令的下一条指令

```
| OldBP(int) | CallerIndex(int) | OldIP(int) |
```

#### VM.2.3: 函数运算

栈式虚拟机的计算栈就在堆栈顶，因此临时变量的出现会导致SP不断变化

```
... | arg0 | ... | argN | MiscData | ...Local Vars... | tmp0 | ... |
                        ^                                          ^
                        FP                                         SP
```

#### VM.2.4: 返回值

函数返回前，临时变量空间（计算栈）必须只能有返回值（如果无返回值，计算栈为空）

```
... | arg0 | ... | argN | MiscData | ...Local Vars... | value(RETT) |
                        ^                                           ^
                        FP                                          SP
```

#### VM.2.5: 函数返回

1. 返回值出栈
2. 依据MiscData恢复调用前的状态
3. 依据Callee的函数，可以知道参数的大小，清空参数部分，堆栈恢复到参数未被eval之前的状态，注意如果是non-static的函数要多pop一个this
4. 返回值重新入栈

```
... | ... | value(RETT) |
    ^                   ^
    FP                  SP
```

### VM.3: 字节码结构

注意字节码中常量池的Index都是从1开始的，0表示无常量池信息，和JVM相同。
注意常量池中的类、域、方法可能是该模块引用的别的模块的类、域和方法。

#### VM.3.1: 模块信息

字节码以模块为单位，字节码中包含模块名在字符串常量池中的Index

#### VM.3.2: 字节码的字符串常量池

UTF字符串

#### VM.3.3: 类常量池

模块名Index、类名Index

#### VM.3.4: 域常量池

类Index、域名Index、类型Index、修饰符Flag

#### VM.3.5: 方法常量池

类Index、方法名Index、类型Index、修饰符Flag

#### VM.3.6: 代码

代码区和方法常量池一一对应。对于模块内方法，存放byte[]的指令，以及局部变量类型Index列表，对于模块外的方法，存一个null。

### VM.4: 地址

绝对地址可以用于确定一个对象或者内建类型的位置.
注意访问一个对象的某个域，一个类的某个静态域，一个数组的某个index均要使用这个对象、类静态区、数组的起始位置的绝对地址与offset二元组的方式来索引，这和操作系统上不一样，
这么设计是为了让内存可以用哈希表优化读取。相对地址该位置在所属内存区域的offset。
关于绝对地址和相对地址的转换可以看XiVM.Runtime.MemoryMap。目前有效的地址区域是NULL，保留区，线程堆栈，堆，静态区，方法区。

### VM.5: 保留区

保留区的设计初衷类似计算机组成中的用CPU操纵外设，可以对这些地址进行LOAD和STORE。这类特殊地址补充了指令集无法涵盖的并且往往是比较Native的功能。

（设想）目前仅实现了IO，这是将这些地址当作外设使用。然而在设计上可以为指令集添加一个特殊指令，call一个地址，这个时候这些保留区内容可以像中断表一样使用，甚至可以支持用户定义中断。

具体地址见XiVM.Runtime.PreservedAddressTag

* NULL 空设备
* STDCHARIO 字符输入输出
* STDTIO 内建类型输入输出
* STDSTRINGIO 字符串输入输出

### VM.6: 堆栈

堆栈本质上是一个变长Slot数组。Slot包含一个数据块（sizeof(int))，包含一个Tag，表明这个slot包含什么数据。
目前仅表示是不是地址，因为要支持未来可能支持的GC。


### VM.7: 堆空间

堆空间整个虚拟机全局唯一

堆上的每一段堆数据都以MiscData开头，大小为sizeof(int)，用来确定数据是哪一个类

不同于堆栈，堆空间的基本单位是byte而不是slot

使用一个链表记录所有对象，用哈希表保证O(1)的读取。内存分配使用best fit O(N)。

### VM.8: 静态区

静态区数据和堆空间相同。
但是因为静态区设计上不会GC，所以内存管理并不需要链表来记录对象，只有一个哈希表，内存分配直接分配在最后，读取和分配都是O(1)。

静态区包含类的静态域。

#### VM.8.1: 类的静态Field

一个类分配一个HeapData，所有静态Field都存储在这列，这个HeapData开头也有MiscData

### VM.9: 方法区

方法区数据和堆空间相同，并且同样不会GC。
方法区和静态区不同的是数据不保证遵守开头是MiscData，或者MiscData里是无效值。
GC时会认为方法区是图的边界。

方法区包含方法的字节码，字符串常量对象（包括储存字符串内容的数组）

#### VM.9.1: 代码段

一个method分配一个HeapData，存储字节码，这个HeapData开头没有MiscData

#### VM.9.2: 字符串常量池

这个常量池和模块的常量池是不同的东西，模块加载后，模块的字符串常量池会链接到这里。
字符串常量池在方法区中，加载模块时会将模块中的常量放到全局常量池中。因此同一个string只会有一份常量，哪怕他们来自不同Module。
加载模块时会动态链接，因此VM在执行CONSTA时依然能够从Index直到字符串常量的堆地址。

字符串常量的结构和堆上的系统库的字符串(SY.2: String)完全相同，除了头部信息没有填。

---

## SY: 系统库

### SY.1: IO

```
class IO {
    // 打印字母
    static void PutChar(int ch);
    // 打印整数
    static void Write(int val);
    // 打印字符串
    static void Write(System.String val);
}
```

### SY.2: String

```
class String {
    // 和C#里一样
    static String Empty;

    // 字符串长度
    int Length;   
    // 保存字符串的数组（UTF格式）
    byte[] Data;    

    // 可以接受字面量
    String(String str);
}
```
