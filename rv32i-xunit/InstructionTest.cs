using Xunit;

namespace RV32I.Tests
{
    public class InstructionTests
    {
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