using Xunit;

namespace RV32I.Tests
{
    public class InstructionTests
    {
        class TestDataInstr
        {
            public uint Instr;
            public Instruction Want;
        };

        [Fact]
        public void Test_NewInstruction()
        {

            TestDataInstr[] testData = {
                    // 80000088: 13 01 01 fe   addi    sp, sp, -32
                    new TestDataInstr { Instr = 0xfe010113, Want = new Instruction { Type= InstructionType.I, Imm = 4294967264, Funct7 = 127, Rs2 = 0, Rs1 = 2, Funct3 = 0, Rd = 2, Opcode = (byte)(0xfe010113 & 0b1111111) } },
                    // 8000008c: 23 2e 11 00   sw      ra, 28(sp)
                    new TestDataInstr{ Instr = 0x00112e23, Want = new Instruction { Type = InstructionType.S, Imm = 28, Funct7 = 0, Rs2 = 1, Rs1 = 2, Funct3 = 2, Rd = 28, Opcode = (byte) (0x00112e23 & 0b1111111) } },
                    // 800000b0: e7 80 80 f7   jalr    -136(ra)
                    //  rs2 and funct7 are not used in JALR
                    //  0xFFFFFEF0. It jumps to x[rs1] + 0xFFFFFEF0 == x0 + 0xFFFFFFE0 = -136
                    new TestDataInstr { Instr = 0xf78080e7, Want = new Instruction { Type = InstructionType.I, Imm = 0xFFFFFF78, Funct7 = 123, Rs2 = 24, Rs1 = 1, Funct3 = 0, Rd = 1, Opcode = (byte)(0xf78080e7 & 0b1111111) } },
                    // 80000010: ef 00 00 05  ▸jal▸0x80000060 <riscv32_boot>
                    //  JAL only use rd and imm
                    //  the current PC 0x80000010 + 0x50 = 0x80000060 is the jump target
                    new TestDataInstr { Instr = 0x050000ef, Want = new Instruction { Type = InstructionType.J, Imm = 0x50, Funct7 = 2, Rs2 = 16, Rs1 = 0, Funct3 = 0, Rd = 1, Opcode = 0x050000ef & 0b1111111 } },
                    // 80000084: 67 80 00 00   ret
                    //  ret -> jalr zero, ra, 0
                    new TestDataInstr { Instr = 0x00008067, Want = new Instruction { Type = InstructionType.I, Imm = 0, Funct7 = 0, Rs2 = 0, Rs1 = 1, Funct3 = 0, Rd = 0, Opcode = 0x00008067 & 0b1111111 } },
                    //       28: 63 00 00 00   beqz    zero, 0x28 <.Lline_table_start0+0x28>
                    new TestDataInstr { Instr = 0x00000063, Want = new Instruction { Type = InstructionType.B, Imm = 0, Funct7 = 0, Rs2 = 0, Rs1 = 0, Funct3 = 0, Rd = 0, Opcode = 0x00000063 & 0b1111111 } },
                    // 800000ac: 97 00 00 00   auipc   ra, 0
                    new TestDataInstr { Instr = 0x00000097, Want = new Instruction { Type = InstructionType.U, Imm = 0, Funct7 = 0, Rs2 = 0, Rs1 = 0, Funct3 = 0, Rd = 1, Opcode = 0x00000097 & 0b1111111 } },
                    //       3c: 73 63 76 31   csrrsi  t1, 791, 12
                    new TestDataInstr { Instr = 0x31766373, Want = new Instruction { Type = InstructionType.C, Imm = 0, Funct7 = 24, Rs2 = 23, Rs1 = 12, Funct3 = 6, Rd = 6, Opcode = 0x31766373 & 0b1111111 } },
            };

            foreach (var td in testData)
            {
                var got = new Instruction(td.Instr);
                Assert.Equal(td.Want, got);
            }
        }


        class TestDataSE
        {
            public uint Imm;
            public int Digit;
            public uint Want;
        };

        [Fact]
        public void Test_SignExtension()
        {
            TestDataSE[] tds = {
                new TestDataSE { Imm = 0x0000FFFE, Digit = 15, Want = 0xFFFFFFFE },
                new TestDataSE { Imm = 0x0000FFFF, Digit = 15, Want = 0xFFFFFFFF },
                new TestDataSE { Imm = 0b00001000, Digit = 8, Want = 0b00001000 },
                new TestDataSE { Imm = 0b00001000, Digit = 3, Want = 0b11111111_11111111_11111111_11111000 },
                new TestDataSE { Imm = 0b111111110100, Digit = 11, Want = 0b11111111_11111111_11111111_11110100 },
            };

            foreach (var td in tds)
            {
                var got = Instruction.SignExtension(td.Imm, td.Digit);
                Assert.Equal(td.Want, got);
            }
        }

        [Fact]
        public void GetInstructionType_Returns_U_When_Opcode_Is_0b0110111()
        {
            // Arrange
            var instruction = new Instruction(0b0110111);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.U, result);
        }
        [Fact]
        public void GetInstructionType_Returns_J_When_Opcode_Is_0b1101111()
        {
            // Arrange
            var instruction = new Instruction(0b1101111);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.J, result);
        }
        [Fact]
        public void GetInstructionType_Returns_B_When_Opcode_Is_0b1100011()
        {
            // Arrange
            var instruction = new Instruction(0b1100011);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.B, result);
        }
        [Fact]
        public void GetInstructionType_Returns_I_When_Opcode_Is_0b0000011()
        {
            // Arrange
            var instruction = new Instruction(0b0000011);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.I, result);
        }
        [Fact]
        public void GetInstructionType_Returns_R_When_Opcode_Is_0b0110011()
        {
            // Arrange
            var instruction = new Instruction(0b0110011);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.R, result);
        }
        [Fact]
        public void GetInstructionType_Returns_S_When_Opcode_Is_0b0100011()
        {
            // Arrange
            var instruction = new Instruction(0b0100011);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.S, result);
        }
        [Fact]
        public void GetInstructionType_Returns_F_When_Opcode_Is_0b0001111()
        {
            // Arrange
            var instruction = new Instruction(0b0001111);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.F, result);
        }
        [Fact]
        public void GetInstructionType_Returns_C_When_Opcode_Is_0b1110011()
        {
            // Arrange
            var instruction = new Instruction(0b1110011);
            // Act
            var result = instruction.GetInstructionType();
            // Assert
            Assert.Equal(InstructionType.C, result);
        }
    }
}