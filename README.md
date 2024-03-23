# Rhino

My first attempt at building a compiler. With the help
of [Immo Landwerth's tutorial series](https://www.youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y) on
building a compiler

## Building

- Download and install `.NET Core 8`
- Clone this repo and run the project
  ```
  git clone git@github.com:Lxdovic/rhino.git
  cd rhino/
  dotnet run
  ```

## Features

#### Opperators:
Allows for arithmetic expressions

- `+` Addition
- `-` Subtraction
- `*` Multiplication
- `/` Division
- `(` and `)` Parentheses
- `&&` Logical AND
- `||` Logical OR
- `!` Logical NOT
- `==` Equality
- `!=` Inequality
- `=` Assignment
- `&` Bitwise AND
- `|` Bitwise OR
- `^` Bitwise XOR
- `<<` Bitwise Left Shift
- `>>` Bitwise Right Shift

#### Assignments:
Allows you to declare variables

- `<identifier> = <value>`

```
» a = 10
10
» a ^ 6 
12 
```

#### Diagnostics:
diagnostics hold every errors being thrown

```
» a + 
• 

(line 3:1): ERROR: unexpected token <EndOfFileToken>, expected <IdentifierToken>.

(line 1:1): ERROR: variable 'a' doesn't exist.
    a + 

(line 3:1): ERROR: variable '' doesn't exist.
    
» 

```

#### Meta commands:
Meta commands allows for better developer experience

- `#showTree` Shows syntax tree
- `#cls` Clears the console

```
» #showTree
Showing parse trees.
» a = 10 + 32 / 2
└──CompilationUnit
   ├──AssignmentExpression
   │  ├──IdentifierToken
   │  ├──EqualsToken
   │  └──BinaryExpression
   │     ├──LiteralExpression
   │     │  └──NumberToken 10
   │     ├──PlusToken
   │     └──BinaryExpression
   │        ├──LiteralExpression
   │        │  └──NumberToken 32
   │        ├──SlashToken
   │        └──LiteralExpression
   │           └──NumberToken 2
   └──EndOfFileToken
26
```

