# RV32I emulator in CSharp

## About

* C# version of [RV32I emulator in Go](https://github.com/sokoide/rv32i-go)

## How to run

```bash
dotnet run --project demo-exe -- --demo
dotnet run --project demo-exe -- -s ./data/sample-binary-001.txt -e 0x0c -l trace
# or
make

# .net 8 test
make demo8
# .net 6 test
make demo6
```

## Quick Benchmark on M1 Mac mini

* rv32i-go built by go1.22.3: 1.7s
* rv32i-cs on .net 8.0.6:     0.9s
* rv32i-cs on .net 6.0.31:    1.3s

## Note

* Assembler is not included. Please use it in [RV32I emulator in Go](https://github.com/sokoide/rv32i-go)
