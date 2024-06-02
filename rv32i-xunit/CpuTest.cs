using Xunit;
using RV32I;
namespace RV32I.Tests
{
    public class CpuTests
    {
        [Fact]
        public void Test_Execute()
        {
            uint code;
            uint got;
            Instruction instr;
            bool inc;
            Cpu cpu = new Cpu();
            Emulator emu = new Emulator
            {
                Cpu = cpu,
                Memory = new byte[Emulator.MaxMemory]
            };
            cpu.Emu = emu;

            // Lui --------------------
            cpu.Reset();
            code = Instruction.GenCode(OpName.Lui, 10, 41, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(41 << 12, cpu.X[10]);

            // Auipc --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            code = Instruction.GenCode(OpName.Auipc, 10, 41, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>((41 << 12) + 0x50, cpu.X[10]);

            // Jal --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            code = Instruction.GenCode(OpName.Jal, 11, 1024, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            Assert.Equal<uint>(0x50 + 4, cpu.X[11]);
            // 0x50 - 12
            cpu.PC = 0x50;
            code = Instruction.GenCode(OpName.Jal, 11, -12, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 - 12, cpu.PC);
            Assert.Equal<uint>(0x50 + 4, cpu.X[11]);
            cpu.Reset();
            // 0x50 - 12, x0 not changed
            cpu.PC = 0x50;
            code = Instruction.GenCode(OpName.Jal, 0, -12, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 - 12, cpu.PC);
            Assert.Equal<uint>(0, cpu.X[0]);

            // Jalr --------------------
            cpu.Reset();
            // 0x1000 + 1024
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            code = Instruction.GenCode(OpName.Jalr, 11, 1024, 3);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x1000 + 1024, cpu.PC);
            Assert.Equal<uint>(0x50 + 4, cpu.X[11]);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            code = Instruction.GenCode(OpName.Jalr, 11, -300, 3);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x1000 - 300, cpu.PC);
            Assert.Equal<uint>(0x50 + 4, cpu.X[11]);

            // Beq --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Beq, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1002;
            code = Instruction.GenCode(OpName.Beq, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);

            // Bne --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bne, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1002;
            code = Instruction.GenCode(OpName.Bne, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);

            // Blt --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Blt, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0xffffffff;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Blt, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Blt, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);

            // Bge --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bge, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0xffffffff;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bge, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Bge, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);

            // Bltu --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Bltu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0xffffffff;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Bltu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bltu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);

            // Bgeu --------------------
            cpu.Reset();
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bgeu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0xffffffff;
            cpu.X[4] = 0x1000;
            code = Instruction.GenCode(OpName.Bgeu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.False(inc);
            Assert.Equal<uint>(0x50 + 1024, cpu.PC);
            cpu.PC = 0x50;
            cpu.X[3] = 0x1000;
            cpu.X[4] = 0x1010;
            code = Instruction.GenCode(OpName.Bgeu, 3, 4, 1024);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.True(inc);
            Assert.Equal<uint>(0x50, cpu.PC);

            // Lb --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 3;
            cpu.Emu.Memory[43] = 1;
            code = Instruction.GenCode(OpName.Lb, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(3, cpu.X[10]);
            cpu.Reset();
            cpu.X[1] = 100;
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[142] = 4;
            cpu.Emu.Memory[143] = 1;
            code = Instruction.GenCode(OpName.Lb, 10, 42, 1);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(4, cpu.X[10]);
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 0xff;
            cpu.Emu.Memory[43] = 1;
            code = Instruction.GenCode(OpName.Lb, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<int>(-1, (int)cpu.X[10]);

            // Lh --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 3;
            cpu.Emu.Memory[43] = 1;
            cpu.Emu.Memory[44] = 1;
            code = Instruction.GenCode(OpName.Lh, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0x0103, cpu.X[10]);
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 0xff;
            cpu.Emu.Memory[43] = 0xff;
            code = Instruction.GenCode(OpName.Lh, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<int>(-1, (int)cpu.X[10]);

            // Lw --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.X[1] = 100;
            cpu.Emu.Memory[142] = 3;
            cpu.Emu.Memory[143] = 1;
            cpu.Emu.Memory[144] = 1;
            cpu.Emu.Memory[145] = 1;
            code = Instruction.GenCode(OpName.Lw, 10, 42, 1);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0x01010103, cpu.X[10]);
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 0xff;
            cpu.Emu.Memory[43] = 0xff;
            cpu.Emu.Memory[44] = 0xff;
            cpu.Emu.Memory[45] = 0xff;
            code = Instruction.GenCode(OpName.Lw, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<int>(-1, (int)cpu.X[10]);

            // Lbu --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 0xff;
            cpu.Emu.Memory[43] = 0;
            code = Instruction.GenCode(OpName.Lbu, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0x000000ff, cpu.X[10]);

            // Lhu --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.Emu.Memory[42] = 0xff;
            cpu.Emu.Memory[43] = 0xff;
            cpu.Emu.Memory[44] = 0;
            code = Instruction.GenCode(OpName.Lhu, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0x0000ffff, cpu.X[10]);

            // Sb --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.X[10] = 0x11223344;
            code = Instruction.GenCode(OpName.Sb, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            got = (uint)(cpu.Emu.Memory[42] | (cpu.Emu.Memory[43] << 8) | (cpu.Emu.Memory[44] << 16) | (cpu.Emu.Memory[45] << 24));
            Assert.Equal<uint>(0x44, got);
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.X[1] = 100;
            cpu.X[10] = 0x11223344;
            code = Instruction.GenCode(OpName.Sb, 10, 42, 1);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            got = (uint)(cpu.Emu.Memory[142] | (cpu.Emu.Memory[143] << 8) | (cpu.Emu.Memory[144] << 16) | (cpu.Emu.Memory[145] << 24));
            Assert.Equal<uint>(0x44, got);

            // Sh --------------------
            cpu.Reset();
            cpu.Emu.Memory = new byte[Emulator.MaxMemory];
            cpu.X[10] = 0x11223344;
            code = Instruction.GenCode(OpName.Sh, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            got = (uint)(cpu.Emu.Memory[42] | (cpu.Emu.Memory[43] << 8) | (cpu.Emu.Memory[44] << 16) | (cpu.Emu.Memory[45] << 24));
            Assert.Equal<uint>(0x3344, got);

            // Sw --------------------
            cpu.Reset();
            cpu.X[10] = 0x11223344;
            code = Instruction.GenCode(OpName.Sw, 10, 42, 0);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            got = (uint)(cpu.Emu.Memory[42] | (cpu.Emu.Memory[43] << 8) | (cpu.Emu.Memory[44] << 16) | (cpu.Emu.Memory[45] << 24));
            Assert.Equal<uint>(0x11223344, got);

            // Addi --------------------
            cpu.Reset();
            code = Instruction.GenCode(OpName.Addi, 10, 0, 42);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(42, cpu.X[10]);
            code = Instruction.GenCode(OpName.Addi, 11, 10, 42);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(84, cpu.X[11]);
            code = Instruction.GenCode(OpName.Addi, 11, 10, -41);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[11]);
            code = Instruction.GenCode(OpName.Addi, 11, 10, -44);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0xfffffffe, cpu.X[11]);

            // Slti --------------------
            cpu.Reset();
            cpu.X[3] = 0;
            cpu.X[4] = 100;
            code = Instruction.GenCode(OpName.Slti, 3, 4, 101);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 100;
            code = Instruction.GenCode(OpName.Slti, 3, 4, 100);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 100;
            code = Instruction.GenCode(OpName.Slti, 3, 4, -1);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[3]);

            // Sltiu --------------------
            cpu.Reset();
            cpu.X[3] = 0;
            cpu.X[4] = 100;
            code = Instruction.GenCode(OpName.Sltiu, 3, 4, 101);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 100;
            code = Instruction.GenCode(OpName.Sltiu, 3, 4, 100);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 100;
            // -1 is 0xffffffff in uint32
            code = Instruction.GenCode(OpName.Sltiu, 3, 4, -1);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[3]);

            // Xori --------------------
            cpu.Reset();
            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm sign exteded to 0b11111111_11111111_11111111_00001111
            code = Instruction.GenCode(OpName.Xori, 3, 4, 0b1111_00001111);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00110011_00110011_00110011_11000011, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm 0b00000000_00000000_00000011_00001111
            code = Instruction.GenCode(OpName.Xori, 3, 4, 0b0011_00001111);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11001100_11001100_11001111_11000011, cpu.X[3]);

            // Ori --------------------
            cpu.Reset();
            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm sign exteded to 0b11111111_11111111_11111111_00001111
            code = Instruction.GenCode(OpName.Ori, 3, 4, 0b1111_00001111);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11111111_11111111_11111111_11001111, cpu.X[3]);

            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm 0b00000000_00000000_00000011_00001111
            code = Instruction.GenCode(OpName.Ori, 3, 4, 0b0011_00001111);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11001100_11001100_11001111_11001111, cpu.X[3]);

            // Andi --------------------
            cpu.Reset();
            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm sign exteded to 0b11111111_11111111_11111111_00001111
            code = Instruction.GenCode(OpName.Andi, 3, 4, 0b1111_00001111);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11001100_11001100_11001100_00001100, cpu.X[3]);
            cpu.X[3] = 0;
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // imm 0b00000000_00000000_00000011_00001111
            code = Instruction.GenCode(OpName.Andi, 3, 4, 0b0011_00001111);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00000000_00000000_00000000_00001100, cpu.X[3]);

            // Slli --------------------
            cpu.Reset();
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // left logical shift 26 bits
            code = Instruction.GenCode(OpName.Slli, 3, 4, 0b11010);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00110000_00000000_00000000_00000000, cpu.X[3]);

            // Srli --------------------
            cpu.Reset();
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            // right logical shift 26 bits
            code = Instruction.GenCode(OpName.Srli, 3, 4, 0b11010);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00000000_00000000_00000000_00110011, cpu.X[3]);

            // Srai --------------------
            cpu.Reset();
            // right arithmetic shift 26 bits - negative
            cpu.X[4] = 0b11001100_11001100_11001100_11001100;
            code = Instruction.GenCode(OpName.Srai, 3, 4, 0b11010);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11111111_11111111_11111111_11110011, cpu.X[3]);

            // right arithmetic shift 26 bits - positive
            cpu.X[4] = 0b01001100_11001100_11001100_11001100;
            code = Instruction.GenCode(OpName.Srai, 3, 4, 0b11010);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00000000_00000000_00000000_00010011, cpu.X[3]);

            // Add --------------------
            cpu.Reset();
            cpu.X[3] = 123;
            cpu.X[4] = 4567;
            code = Instruction.GenCode(OpName.Add, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(4690, cpu.X[1]);

            // overflow
            cpu.X[3] = 0x0002;
            cpu.X[4] = 0xffffffff;
            code = Instruction.GenCode(OpName.Add, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[1]);

            // Sub --------------------
            cpu.Reset();
            cpu.X[3] = 123;
            cpu.X[4] = 23;
            code = Instruction.GenCode(OpName.Sub, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(100, cpu.X[1]);
            // underflow
            cpu.X[3] = 100;
            cpu.X[4] = 102;
            code = Instruction.GenCode(OpName.Sub, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0xfffffffe, cpu.X[1]);

            // Sll --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 4;
            code = Instruction.GenCode(OpName.Sll, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11001100_11001100_11001100_11000000, cpu.X[1]);

            // Slt --------------------
            cpu.Reset();
            cpu.X[3] = 3;
            cpu.X[4] = 4;
            code = Instruction.GenCode(OpName.Slt, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[1]);

            cpu.X[3] = 3;
            cpu.X[4] = 3;
            code = Instruction.GenCode(OpName.Slt, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[1]);

            cpu.X[3] = 0xfffffffe; // -2
            cpu.X[4] = 3;
            code = Instruction.GenCode(OpName.Slt, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[1]);

            cpu.X[3] = 0xfffffffe; // -2
            cpu.X[4] = 0xfffffffd; // -3
            code = Instruction.GenCode(OpName.Slt, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[1]);

            // Sltu --------------------
            cpu.Reset();
            cpu.X[3] = 3;
            cpu.X[4] = 4;
            code = Instruction.GenCode(OpName.Sltu, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(1, cpu.X[1]);
            cpu.X[3] = 3;
            cpu.X[4] = 3;
            code = Instruction.GenCode(OpName.Sltu, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[1]);
            cpu.X[3] = 0xfffffffe; // -2, but testing as uint
            cpu.X[4] = 3;
            code = Instruction.GenCode(OpName.Sltu, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[1]);
            cpu.X[3] = 0xfffffffe; // -2, but testing as uint
            cpu.X[4] = 0xfffffffd; // -3, but testing as uint
            code = Instruction.GenCode(OpName.Sltu, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0, cpu.X[1]);

            // Xor --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 0b11110000_11110000_11110000_11110000;
            code = Instruction.GenCode(OpName.Xor, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00111100_00111100_00111100_00111100, cpu.X[1]);

            // Srl --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 8;
            code = Instruction.GenCode(OpName.Srl, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00000000_11001100_11001100_11001100, cpu.X[1]);

            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            // only the lower 5bits are used -> 4 bit shift
            cpu.X[4] = 0b11111111_11111111_11111111_00000100;
            code = Instruction.GenCode(OpName.Srl, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b00001100_11001100_11001100_11001100, cpu.X[1]);

            // Sra --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 8;
            code = Instruction.GenCode(OpName.Sra, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11111111_11001100_11001100_11001100, cpu.X[1]);

            cpu.X[3] = 0b10001100_11001100_11001100_11001100;
            // only the lower 5bits are used -> 4 bit shift
            cpu.X[4] = 0b11111111_11111111_11111111_00000100;
            code = Instruction.GenCode(OpName.Sra, 1, 3, 4);
            instr = new Instruction(code);
            inc = cpu.Execute(instr);

            Assert.Equal<uint>(0b11111000_11001100_11001100_11001100, cpu.X[1]);

            //  Or --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 0b11110000_11110000_11110000_11110000;
            code = Instruction.GenCode(OpName.Or, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11111100_11111100_11111100_11111100, cpu.X[1]);

            // And --------------------
            cpu.Reset();
            cpu.X[3] = 0b11001100_11001100_11001100_11001100;
            cpu.X[4] = 0b11110000_11110000_11110000_11110000;
            code = Instruction.GenCode(OpName.And, 1, 3, 4);
            instr = new Instruction(code);

            inc = cpu.Execute(instr);
            Assert.Equal<uint>(0b11000000_11000000_11000000_11000000, cpu.X[1]);
        }

        [Fact]
        public void Reset_Resets_Registers_And_PC()
        {
            // Arrange
            var cpu = new Cpu();
            cpu.X[0] = 42;
            cpu.PC = 100;
            // Act
            cpu.Reset();
            // Assert
            Assert.Equal<uint>(0u, cpu.X[0]);
            Assert.Equal<uint>(0u, cpu.PC);
        }
    }
}