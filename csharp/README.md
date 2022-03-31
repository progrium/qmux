# qmux for C#

> WIP

## Setup

`qmux` for C# requires `dotnet`. Install `dotnet` manually or via package
manager using instructions below.

### Package Manager Installation

| Platform | Command                      |
| -------- | ---------------------------- |
| macos    | `brew install dotnet --cask` |
| win11    | `choco install dotnet`       |
| win11    | `winget install dotnet`      |
| apt      | `apt install dotnet`         |

### Manual Installation

Download the `dotnet` installer
[for your platform here](https://dotnet.microsoft.com/en-us/download).

## Build

```bash
dotnet build ./qmux/qmux.csproj
```

## Run tests

```bash
dotnet test ./qmux.tests/qmux.tests.csproj
```
