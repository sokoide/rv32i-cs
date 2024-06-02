.PHONY: all clean net8 net6 demo8 demo6 make8 make6 test

UNAME=$(shell uname -s)


ifeq ($(UNAME), Darwin)
	SED=sed -i ''
else
	SED=sed -i
endif

all: net8

net8: make8
	dotnet build --configuration Release

net6: make6
	dotnet build --configuration Release

demo8: make8
	dotnet run --framework net8.0 --configuration Release --project demo-exe --  -s ./data/sample-binary-fib.txt -e 0x130

demo6: make6
	dotnet run --framework net6.0 --configuration Release --project demo-exe --  -s ./data/sample-binary-fib.txt -e 0x130

test: make8
	dotnet test --framework net8.0

make8:
	$(SED) 's/net6\.0/net8\.0/g' rv32i-lib/rv32i-lib.csproj
	$(SED) 's/net6\.0/net8\.0/g' demo-exe/demo-exe.csproj

make6:
	$(SED) 's/net8\.0/net6\.0/g' rv32i-lib/rv32i-lib.csproj
	$(SED) 's/net8\.0/net6\.0/g' demo-exe/demo-exe.csproj

clean:
	rm -rf bin obj demo-exe/bin demo-exe/obj rv32i-lib/bin rv32i-lib/obj rv32i-xunit/bin rv32i-xunit/obj