# XiVM

## TODO

* ref和new
* XiLang中char字面量和string字面量中的转义字符问题
* XiLang以及XiVM中对类的支持
* XiVM的浮点数运算
* XiVM中对于类型的处理（主要是Size）还比较混乱
* 系统库函数，首先支持控制台输出，目前的控制台输出是hack的，如何很好地实现多模块以及import
* 为了支持函数引用，可能需要另一种Call，为了支持重载，可能需要扩展函数查询
* 为了支持GC，堆栈可能需要记录数据的tag（即是不是address）

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
    * 三目运算符
* 注释：C++风格注释
* 字面量
    * 整数：十进制，十六进制
    * 浮点数：十进制
    * 字符串，字符
* 分支
    * if语句，then和otherwise必须被花括号包围，otherwise可以是另一个if
* 循环
    * while，body必须被花括号包围，或者是空
    * for：可以在init中定义变量，body必须被花括号包围，或者是空
* 类
    * 成员变量
    * 成员函数

### Grammar

#### Stmt

```
Program
    (GlobalStmt)*
GlobalStmt
    ClassStmt | DeclarationStmt
ClassStmt
    CLASS ID LBRACES DeclarationStmt* RBRACES
DeclarationStmt
    TypeExpr FuncDeclarator BlockStmt
    TypeExpr VarDeclarator SEMICOLON
FuncDeclarator
    ID LPAREN ParamsAST? RPAREN
VarDeclarator
    ID (ASSIGN Expr)? (COMMA VarDeclarator)?
ParamsAST
    Params
Params
    TypeExpr ID (ASSIGN Expr)? (COMMA Params)?
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
    IF LPAREN Expr RPAREN BlockStmt (ELSE BlockStmt | IfStmt)?
JumpStmt
    (CONTINUE | BREAK | RETURN (ListExpr)?) SEMICOLON
ExprStmt
    ListExpr SEMICOLON
```

#### Expr

```
TypeExpr
    ANY_TYPE_SPEC* (ANY_TYPE | ID) (LBRACKET RBRACKET)?
ListExpr
    Expr [COMMA Expr]*
Expr
    (ConditionalExpr | Id) (ANY_ASSIGN Expr)?
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
    ConstExpr | ID | LPAREN Expr RPAREN | BASE DOT ID
ConstExpr
    TRUE | FALSE | NULL | DEC_LITERAL | HEX_LITERAL | FLOAT_LITERAL | STR_LITERAL
```

## XiVM

* 栈式虚拟机
* Stack Based Runtime Environment without Local Procedure
* 支持生成.xibc字节码和.xir文本中间码

### 指令集

以下说明中的T为类型，可以是B(byte 1字节), I(int 4字节), D(double 8字节), A(Address 4字节)；
N为大小（单位为字节）。

单个指令的使用说明格式: 
```[指令] ([参数(参数的C#类型)] ...) [指令编码]```

#### NOP

* NOP   0X00

 无任何操作

#### PUSHT

* PUSHB value(byte) 0X01
* PUSHI value(int) 0X02
* PUSHD value(double) 0X03
* PUSHA value(uint) 0X04

将参数value push进计算栈

```
... |
... | value(N) |
```

#### POP(N)

* POP 0X08
* POP4 0X09
* POP8 0X0A

将计算栈中大小为N的空间Pop出去，POP的N就是1

```
... | value(N) |
... |
```

#### DUP(N)

* DUP 0X10
* DUP4 0X11
* DUP8 0X12

将计算栈中大小为N的空间复制并Push进计算栈，DUP的N就是1

```
... | value(N) |
... | value(N) | value(N) |
```

#### LOCALA

* LOCALA offset(int) 0x18

获取当前栈帧的某个栈地址并Push进计算栈。offset的为目标地址与BP的距离

```
... |
... | addr(uint) |
```

#### GLOBALA

* GLOBALA offset(int) 0x19

获取全局栈帧的某个栈地址并Push进计算栈。offset的为目标地址与全局栈帧BP（就是栈开头）的距离。
因为只有LocalA和GlobalA两个取栈地址的指令，XiVM实际上不支持local procedure

```
... |
... | addr(uint) |
```

#### LOADT

* LOADB 0X20
* LOADI 0X21
* LOADD 0X22
* LOADA 0X23

load会从栈顶的地址位置加载一个T类型的值，Push进计算栈

```
... | addr(uint) |
... | value(T) |
```

#### STORET

* STOREB 0X28
* STOREI 0X29
* STORED 0X2A
* STOREA 0X2B

store会从栈顶的地址位置加载uint类型的addr，将T类型的value存储到这个地址

```
... | value(T) | addr(uint) |
... |
```

#### ADDT

* ADDI 0X40

加法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### SUBT

* SUBI 0X44

减法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### MULT

* MULI 0X48

乘法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### DIVT

* DIVI 0X4C

除法

```
... | lhs(T) | rhs(T) |
... | res(T) |
```

#### MOD

* MOD 0X50

模运算

```
... | lhs(int) | rhs(int) |
... | res(int) |
```

#### NEGT

* NEGI 0X54

取反

```
... | value(T) |
... | res(T) |
```

#### TIN2TOUT

* I2D 0X60
* D2I 0X61
* B2I 0X62

类型转换

```
... | value(TIN) |
... | res(TOUT) |
```

#### SET(COND)(T)

* SETEQI 0X70
* SETNEI 0X71
* SETLTI 0X72
* SETLEI 0X73
* SETGTI 0X74
* SETGEI 0X75

比较。如果cond成立，res为1，否则res为1

```
... | lhs(T) | rhs(T) |
... | res(byte) |
```

#### JMP

* JMP offset(int) 0X80

栈不改变。IP+=offset

#### JCOND

* JCOND offset(int) offset1(int) 0X81

如果cond为0，IP+=offset1，否则，IP+=offset

```
... | cond(byte) |
... |
```

#### CALL

* CALL addr(uint) 0X90

行为参考[发起调用](#发起调用)

#### RET(T)

* RET 0X94
* RETB 0X95
* RETI 0X96
* RETD 0X97
* RETA 0X98

行为参考[函数返回](#函数返回)

#### PUTC

* PUTC 0XA0

将int类型值以字符形式输出到控制台

```
... | value(int) |
... |
```


### 函数调用规范

#### 参数传递

函数Call之前，计算栈如下, 参数应当倒序进入计算栈

```
... | ... | argN | ... | arg0 |
    ^                         ^
    BP                        SP
```

#### 发起调用

Call执行之后，会创建函数栈帧，修改BP和SP。

```
... | argN | ... | arg0 | MiscData | ...Local Vars... |
                        ^                             ^
                        BP                            SP
```

栈帧的Misc数据用于Ret时恢复堆栈。包括如下内容，OldBP是Call之前的BP，Caller的Index和OldIP用于找到Call指令的下一条指令

```
| OldBP(int) | CallerIndex(int) | OldIP |
```

#### 函数运算

栈式虚拟机的计算栈就在堆栈顶，因此临时变量的出现会导致SP不断变化

```
... | argN | ... | arg0 | MiscData | ...Local Vars... | tmp0 | ... |
                        ^                                          ^
                        BP                                         SP
```

#### 返回值

函数返回前，临时变量空间（计算栈）必须只能有返回值（如果无返回值，计算栈为空）

```
... | argN | ... | arg0 | MiscData | ...Local Vars... | value(RETT) |
                        ^                                           ^
                        BP                                          SP
```

#### 函数返回

1. 返回值出栈
2. 依据MiscData恢复调用前的状态
3. 依据Callee的函数，可以知道参数的大小，清空参数部分，堆栈恢复到参数未被eval之前的状态
4. 返回值重新入栈

```
... | ... | value(RETT) |
    ^                   ^
    BP                  SP
```

