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
  dotnet run --project rc/rc.csproj
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
- `=` Assignment

### Assignments:

- `var <identifier> = <value>` creates a mutable variable
- `let <identifier> = <value>` crates an immutable variable

### Scopes

- `{ ... }`

### Conditions

- `if <condition>`

### Loops

- `for <identifier> = <lowerBound> to <upperBound>`
- `while <condition>`

### Meta commands:

- `#showTree` Shows syntax tree
- `#cls` Clears the console
- `#reset` Resets the current compilation

## Examples:

For loops
```
» var myVariable = 0
0
» for i = 0 to 100 {
•   myVariable = myVariable + i
• }
5050
```

While loops
```
» var myVariable = 0
0
» while myVariable < 100 {
•   myVariable = myVariable + 1
• }
100
```
