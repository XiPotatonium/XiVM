# XiVM

## TODO

* new 关键字
* ref 关键字
* XiLang中char字面量以及文本的字面量中的转义字符问题
* XiLang常量表达式中，加法支持字符串拼接
* XiLang顶层BUG修复
* XiVM中对于类型的处理（主要是Size）还比较混乱
* 系统库函数，首先支持控制台输出，目前的控制台输出是hack的，如何很好地实现多模块以及import
* XiVM中对Array的处理是有问题的，Array应该像string一样是一个系统库类型
* 为了支持函数引用，可能需要另一种Call，为了支持重载，可能需要扩展函数查询
* XiVM的浮点数运算以及完善的比较运算

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

原先计算栈顶为

... |

将参数value push进计算栈

... | value(N) |

#### POP(N)

* POP 0X08
* POP4 0X09
* POP8 0X0A

原先计算栈顶为

... | value(N) |

将计算栈中大小为N的空间Pop出去，POP的N就是1

... |

#### DUP(N)

* DUP 0X10
* DUP4 0X11
* DUP8 0X12

原先计算栈顶为

... | value(N) |

将计算栈中大小为N的空间复制并Push进计算栈，DUP的N就是1

... | value(N) | value(N) |

#### LOCALA

* LOCALA offset(int) 0x18

原先计算栈顶为

... |

当前栈帧offset为offset的栈地址Push进计算栈，offset为正数

... | addr(uint) |

#### GLOBALA

* GLOBALA offset(int) 0x19

原先计算栈顶为

... |

全局栈帧offset为offset的栈地址Push进计算栈，offset为正数。因此XiVM实际上不支持嵌套函数

... | addr(uint) |

#### LOADT

* LOADB 0X20
* LOADI 0X21
* LOADD 0X22
* LOADA 0X23

原先计算栈顶为

... | addr(uint) |

load会从栈顶的地址位置加载一个T类型的值，Push进计算栈

... | value(T) |

#### STORET

* STOREB 0X28
* STOREI 0X29
* STORED 0X2A
* STOREA 0X2B

原先计算栈顶为

... | value(T) | addr(uint) |

store会从栈顶的地址位置加载uint类型的addr，将T类型的value存储到这个地址

... |

#### ADDT

* ADDI 0X30

原先计算栈顶为

... | lhs(T) | rhs(T) |

加法

... | res(T) |

#### SUBT

* SUBI 0X34

原先计算栈顶为

... | lhs(T) | rhs(T) |

减法

... | res(T) |

#### MULT

* MULI 0X38

原先计算栈顶为

... | lhs(T) | rhs(T) |

乘法

... | res(T) |

#### DIVT

* DIVI 0X3C

原先计算栈顶为

... | lhs(T) | rhs(T) |

除法

... | res(T) |

#### MOD

* MOD 0X40

原先计算栈顶为

... | lhs(int) | rhs(int) |

模运算

... | res(int) |

#### NEGT

* NEGI 0X44

原先计算栈顶为

... | value(T) |

取反

... | res(T) |

#### SET(COND)(T)

* SETEQ 0X50

原先计算栈顶为

... | lhs(T) | rhs(T) |

比较。如果cond成立，res为1，否则res为1

... | res(byte) |

#### TIN2TOUT

* I2D 0X60
* D2I 0X61
* B2I 0X62

原先计算栈顶为

... | value(TIN) |

格式转换

... | res(TOUT) |

#### JMP

* JMP offset(int) 0X70

栈不改变。IP改变offset

#### JCOND

* JCOND offset(int) offset1(int) 0X71

原先计算栈顶为

... | cond(byte) |

如果cond为0，IP改变offset1，否则，IP改变offset

... |

#### CALL

* CALL addr(uint) 0X80

执行addr所指向的函数，fptr是函数在函数总表中的Index，VM会负责处理堆栈和IP，不改变计算栈

#### RET

* RET 0X84

不对计算栈做任何修改。虚拟机会执行函数返回的相关工作。

#### PRINT(T)

* PRINTI 0XA0

原先计算栈顶为

... | value(T) |

将栈顶的值输出到控制台

... |


### 函数调用规范

#### 参数传递

函数Call之前，计算栈如下

... | argN | ... | arg0 | fptr(uint) |

参数应当倒序进入计算栈

Callee开始执行后需要将传入的参数存储到堆栈中。规定函数栈帧的起始空间就是参数且参数顺序排放。
这样Callee只需要不断执行Get参数地址，Store参数就可以将计算栈中的参数值顺序放置到函数栈帧中。

#### 函数栈帧

```
... | OldBP(int) | RetFuncIndex(int) | RetIP(int) | ...栈帧内容... |
    ^                                                              ^
    BP                                                             SP
```

