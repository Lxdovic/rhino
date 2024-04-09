# Rhino

My first attempt at building a compiler. With the help
of [Immo Landwerth's tutorial series](https://www.youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y) on
building a compiler

## Run Locally

- Install .NET >= 8.0
- Clone the project

```bash
  git clone git@github.com:Lxdovic/rhino.git
```

- Go to the project directory

```bash
  cd rhino
```
- Run the project

```bash
  dotnet run --project ./rc/rc.csproj
```

## Running Tests

To run tests, use the following commands

```bash
  cd rhino/
  dotnet test
```

## Features

### Operators:

- `+` Addition
- `-` Subtraction
- `*` Multiplication
- `/` Division
- `(` and `)` Parentheses
- `&&` Logical AND
- `||` Logical OR
- `!` Logical NOT
- `&` Bitwise AND
- `|` Bitwise OR
- `^` Bitwise XOR
- `~` Bitwise NOT
- `<<` Bitwise Left Shift
- `>>` Bitwise Right Shift
- `==` Equality
- `!=` Inequality
- `<` Less than
- `<=` Less than or equal to
- `>` Greater than
- `>=` Greater than or equal to
- `=` Assignment

### Assignments:

- `var <identifier>(: <optional type>) = <value>` creates a mutable variable
- `let <identifier>(: <optional type>) = <value>` crates an immutable variable

### Scopes

- `{ ... }`

### Conditions

- `if <condition>`

### Loops

- `for <identifier> = <lowerBound> to <upperBound>`
- `while <condition>`

### Builtin functions

- `print(<expression>)` prints the expression to the console
- `input()` reads a line from the console
- `random(<min>, <max>)` generates a random number

### Type conversion

- `string(<expression>)` converts the expression to a string
- `int(<expression>)` converts the expression to an integer

### Primitive types

- String
- Int
- Bool

### Meta commands:

- `#showTree` Shows parse tree
- `#showProgram` Shows bound tree
- `#cls` Clears the console
- `#reset` Resets the current compilation

## Examples:

![](./docs/images/odd-even-example.png)
