# Rhino

My first attempt at building a compiler. With the help
of [Immo Landwerth's tutorial series](https://www.youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y) on
building a compiler

## Features

### 09/03/24: Basic Lexer and Parser

Allows you to make simple arithmetic expressions like `1 + 2 * 3` and `(1 + 2) * 3`.

#### Opperators:

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

#### Examples:

```sh
> 1 + 2 * 3
7
```

```sh
> (1 + 2) * 3
9
```

```sh
> a = 1
1
> b = 2
2
> c = a + b
3
```

```sh
> a = 7
7
> b = 3
3
> a ^ b
4
```

```sh
