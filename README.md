# RV32I emulator in CSharp

## About

* C# version of [RV32I emulator in Go](https://github.com/sokoide/rv32i-go)

## How to run

```
dotnet run --project rv32i-cs.csproj -- --demo
dotnet run --project rv32i-cs.csproj -- -s ./data/sample-binary-001.txt -e 0x0c -l trace
dotnet run --project rv32i-cs.csproj -- -s ./data/sample-binary-001.txt -e 0x0c -l trace
# .net 8 test
dotnet run --framework net8.0 --configuration Release --project rv32i-cs.csproj --  -s ./data/sample-binary-fib.txt -e 0x130
# .net 6 test
dotnet run --framework net6.0 --configuration Release --project rv32i-cs-net6.csproj -- -s ./data/sample-binary-fib.txt -e 0x130

2024-06-02 17:32:46.6467|INFO|rv32i-cs.Program|* Elapsed time: 17215 ms
```

## Quick Benchmark on M1 Mac mini

* rv32i-go built by go1.22.3:  3.4s
* rv32i-cs on .net 8.0.6:     17.2s
* rv32i-cs on .net 6.0.31:    32.3s

## Note

* Assembler is not included. Please use it in [RV32I emulator in Go](https://github.com/sokoide/rv32i-go)
