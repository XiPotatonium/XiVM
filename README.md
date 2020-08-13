# XiVM

## TODO

* 外部模块的类和域的链接, System.String的构造函数
* 未定义构造函数要生成一个默认的构造函数
* 复杂条件表达式，注意短路作用
* 堆的GC
* 似乎寻址可以仅使用addr + offset的形式，这样的话堆空间的管理可以使用哈希表，查找就是O(1)了
* ref
* XiLang中char字面量和string字面量中的转义字符问题
* XiVM的浮点数运算
* 三元运算符
* 为了支持动态绑定的函数调用，可能需要另一种Call
* 目前import都是线性依赖的，互相依赖的模块还没有实现.(其实只有共同编译未实现，模块的加载、链接两个pass以及编译的类声明、类成员声明、类成员定义三个pass都做好了)

## XiLang

* 使用正则表达式实现词法分析
* 使用TopDown Parser实现语法分析
* 对常量表达式提前估值
* 生成AST并可以格式化为Json

### 实现列表

* 类型
    * 布尔：bool
    * 整数：32位整数int
    * 浮点：64位浮点数double
    * 数组
    * 其他：void, string
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
* 禁止隐式类型提升，8.0 / 4这样的表达式也是非法的
* import，目前仅支持线性依赖，会自动查找同一文件夹下的.xibc。

### Grammar

#### Stmt

```
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
```

#### Expr

```
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

## XiVM

* 栈式虚拟机
* Stack Based Runtime Environment without Local Procedure
* 支持生成.xibc字节码和.xir文本中间码，字节码参考了JVM
* 字节码生成API设计参考了LLVM-C，使用BasicBlock的思想，代码中unreachable code不会再生成

### 堆栈

堆栈本质上是一个变长Slot数组。Slot包含一个数据块（sizeof(int))，包含一个Tag，表明这个slot包含什么数据。
目前仅表示是不是地址，因为要支持未来可能支持的GC。

### 指令集

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

value = arr[index]，暂不清楚是否要兼容string

```
... | arr(addr) | index(int) |
... | value(T) |
```

#### ASTORET

* ASTOREB
* ASTOREI
* ASTORED
* ASTOREA

arr[index] = value，暂不清楚是否要兼容string

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
行为参考[发起调用](#发起调用)

#### RET(T)

* RET

行为参考[函数返回](#函数返回)

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
字符串常量参考[字符串常量池](#字符串常量池)

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

#### PUTC

* PUTC

PUTC将int类型值以字符形式输出到控制台，仅支持ASCII.

```
... | value(T) |
... |
```

#### PUTI

* PUTI

输出int

```
... | value(int) |
... |
```

#### PUTS

* PUTS

通过地址，获取字符串，取其中的数据转化为UTF8格式字符串输出到控制台

```
... | src(addr) |
... |
```


### 字节码结构

注意字节码中常量池的Index都是从1开始的，0表示无常量池信息，和JVM相同。
注意常量池中的类、域、方法可能是该模块引用的别的模块的类、域和方法。

#### 模块信息

字节码以模块为单位，字节码中包含模块名在字符串常量池中的Index

#### 字节码的字符串常量池

UTF字符串

#### 类常量池

模块名Index、类名Index

#### 域常量池

类Index、域名Index、类型Index、修饰符Flag

#### 方法常量池

类Index、方法名Index、类型Index、修饰符Flag

#### 代码

代码区和方法常量池一一对应。对于模块内方法，存放byte[]的指令，以及局部变量类型Index列表，对于模块外的方法，存一个null。

### 函数调用规范

#### 参数传递

函数Call之前，计算栈如下

```
... | ... | arg0 | ... | argN |
    ^                         ^
    FP                        SP
```

#### 发起调用

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

#### 函数运算

栈式虚拟机的计算栈就在堆栈顶，因此临时变量的出现会导致SP不断变化

```
... | arg0 | ... | argN | MiscData | ...Local Vars... | tmp0 | ... |
                        ^                                          ^
                        FP                                         SP
```

#### 返回值

函数返回前，临时变量空间（计算栈）必须只能有返回值（如果无返回值，计算栈为空）

```
... | arg0 | ... | argN | MiscData | ...Local Vars... | value(RETT) |
                        ^                                           ^
                        FP                                          SP
```

#### 函数返回

1. 返回值出栈
2. 依据MiscData恢复调用前的状态
3. 依据Callee的函数，可以知道参数的大小，清空参数部分，堆栈恢复到参数未被eval之前的状态，注意如果是non-static的函数要多pop一个this
4. 返回值重新入栈

```
... | ... | value(RETT) |
    ^                   ^
    FP                  SP
```


### 堆空间

堆空间整个虚拟机全局唯一

堆上的每一段堆数据都以MiscData开头，大小为sizeof(int)，用来确定数据是哪一个类

不同于堆栈，堆空间的基本单位是byte而不是slot

### 方法区

方法区数据和堆空间相同

#### 类的静态Field

一个类分配一个HeapData，所有静态Field都存储在这列，这个HeapData开头并没有MiscData

#### 代码段

存在一个长度1K的数组作为映射表，保证Call的时候可以在O(1)的时间内找到代码位置，但是最多支持1K个方法。
方法的长度依然会算入方法区大小并受到方法区总大小的限制.

#### 字符串常量池

这个常量池和模块的常量池是不同的东西，模块加载后，模块的字符串常量池会链接到这里。
字符串常量池在方法区中，加载模块时会将模块中的常量放到全局常量池中。因此同一个string只会有一份常量，哪怕他们来自不同Module。
加载模块时会动态链接，因此VM在执行CONSTA时依然能够从Index直到字符串常量的堆地址。
(设想)字符串常量的结构和堆上的字符串完全相同，不能被GC。

## 系统库

### IO

#### PutChar

```
void System.IO.PutChar(int ch);
```

#### Write

```
void System.IO.Write(int val);
void System.IO.Write(System.String val);
```

### String
