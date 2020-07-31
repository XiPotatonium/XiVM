# XiVM

## TODO

* new 关键字
* ref 关键字
* XiLang中char字面量以及文本的字面量中的转义字符问题
* XiLang常量表达式中，加法支持字符串拼接
* XiVM可执行
* XiLang顶层BUG修复

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
    * 字符串
* 分支
    * if语句
* 循环
    * while
    * for：可以在init中定义变量
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
    WHILE LPAREN Expr RPAREN (SELICOLON | Stmt)
    FOR LPAREN (VarOrExprStmt | SEMICOLON) Expr? SEMICOLON ExprList? RPAREN (SELICOLON | Stmt)
IfStmt
    IF LPAREN Expr RPAREN Stmt (ELSE Stmt)?
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

### 指令集

以下说明中的T为类型，可以是B(byte 1字节), I(int 4字节), D(double 8字节), A(Address 4字节)；
N为大小（单位为字节）。

单个指令的使用说明格式: 
```[指令] ([参数(参数的C#类型)] ...) [指令编码]```

#### NOP

* NOP   0x00

 无任何操作

#### PUSHT

* PUSHB value(byte) 0x01
* PUSHI value(int) 0x02
* PUSHD value(double) 0x03
* PUSHA value(uint) 0x04

原先计算栈顶为

... |

将参数value push进计算栈

... | value(N) |

#### POP(N)

* POP 0x08
* POP4 0x09
* POP8 0x0A

原先计算栈顶为

... | value(N) |

将计算栈中大小为N的空间Pop出去，POP的N就是1

... |

#### DUP(N)

* DUP 0x10
* DUP4 0x11
* DUP8 0x12

原先计算栈顶为

... | value(N) |

将计算栈中大小为N的空间复制并Push进计算栈，DUP的N就是1

... | value(N) | value(N) |

#### GETA

* GETA diff(int) offset(int) 0x18

原先计算栈顶为

... |

将堆栈中向前diff个栈帧，offset为offset的栈地址Push进计算栈

... | addr(uint) |

#### LOADT

* LOADB 0x20
* LOADI 0x21
* LOADD 0x22
* LOADA 0x23

原先计算栈顶为

... | addr(uint) |

load会从栈顶的地址位置加载一个T类型的值，Push进计算栈

... | value(T) |

#### STORET

* STOREB 0x30
* STOREI 0x31
* STORED 0x32
* STOREA 0x33

原先计算栈顶为

... | value(T) | addr(uint) |

store会从栈顶的地址位置加载uint类型的addr，将T类型的value存储到这个地址

... |

#### ADDT

* ADDI 0x40

原先计算栈顶为

... | lhs(T) | rhs(T) |

加法

... | res(T) |

#### CALL

* CALL 0x80

原先计算栈顶为

... | fptr(uint) |

执行fptr所指向的函数，fptr是函数在函数总表中的Index，VM会负责处理堆栈和IP，开始执行后的计算栈是

... |

#### RET

* RET 0x81

不对计算栈做任何修改。虚拟机会执行函数返回的相关工作。

### 函数调用规范

#### 参数传递

函数Call之前，计算栈如下

... | argN | ... | arg0 | fptr(uint) |

参数应当倒序进入计算栈

Callee开始执行后需要将传入的参数存储到堆栈中。规定函数栈帧的起始空间就是参数且参数顺序排放。
这样Callee只需要不断执行Get参数地址，Store参数就可以将计算栈中的参数值顺序放置到函数栈帧中。

