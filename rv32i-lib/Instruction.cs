using System;

namespace RV32I
{
    public enum InstructionType
    {
        U,
        J,
        B,
        I,
        S,
        R,
        F,
        C
    }

    public enum OpName
    {
        Lui,
        Auipc,
        Jal,
        Jalr,
        Beq,
        Bne,
        Blt,
        Bge,
        Bltu,
        Bgeu,
        Lb,
        Lh,
        Lw,
        Lbu,
        Lhu,
        Sb,
        Sh,
        Sw,
        Addi,
        Slti,
        Sltiu,
        Xori,
        Ori,
        Andi,
        Slli,
        Srli,
        Srai,
        Add,
        Sub,
        Sll,
        Slt,
        Sltu,
        Xor,
        Srl,
        Sra,
        Or,
        And,
        Fence,
        FenceI,
        Ecall,
        Ebreak,
        Csrrw,
        Csrrs,
        Csrrc,
        Csrrwi,
        Csrrsi,
        Csrrci
    }

    public class Instruction
    {
        public InstructionType Type { get; set; }
        public uint Imm { get; set; }
        public byte Funct7 { get; set; }
        public byte Rs2 { get; set; }
        public byte Rs1 { get; set; }
        public byte Funct3 { get; set; }
        public byte Rd { get; set; }
        public byte Opcode { get; set; }

        public Instruction()
        {
            Opcode = 0;
            Rd = 0;
            Funct3 = 0;
            Rs1 = 0;
            Rs2 = 0;
            Funct7 = 0;
            Imm = 0;
            Type = InstructionType.I;
        }

        public override bool Equals(object? obj)
        {
            var t = (Instruction)obj;
            return (Opcode == t.Opcode &&
            Rd == t.Rd &&
            Funct3 == t.Funct3 &&
            Rs1 == t.Rs1 &&
            Rs2 == t.Rs2 &&
            Funct7 == t.Funct7 &&
            Imm == t.Imm &&
            Type == t.Type);
        }

        public Instruction(uint instr)
        {
            Opcode = (byte)(instr & 0b1111111);
            Rd = (byte)((instr >> 7) & 0b11111);
            Funct3 = (byte)((instr >> 12) & 0b111);
            Rs1 = (byte)((instr >> 15) & 0b11111);
            Rs2 = (byte)((instr >> 20) & 0b11111);
            Funct7 = (byte)(instr >> 25);
            Imm = 0;
            Type = GetInstructionType();

            uint imm20, imm101, imm11, imm110, imm1912, imm12, imm105, imm41, imm115, imm40;

            switch (Type)
            {
                case InstructionType.U:
                    Imm = instr & 0b11111111_11111111_11110000_00000000;
                    break;
                case InstructionType.J:
                    imm20 = instr >> 31;
                    imm101 = instr >> 21 & 0b11_11111111;
                    imm11 = instr >> 20 & 0b1;
                    imm1912 = instr >> 12 & 0b11111111;
                    Imm = imm20 << 20 | imm101 << 1 | imm11 << 11 | imm1912 << 12;
                    Imm = SignExtension(Imm, 20);
                    break;
                case InstructionType.B:
                    imm12 = instr >> 31;
                    imm105 = instr >> 25 & 0b111111;
                    imm41 = instr >> 8 & 0b1111;
                    imm11 = instr >> 7 & 0b1;
                    Imm = imm12 << 11 | imm105 << 5 | imm41 << 1 | imm11;
                    Imm = SignExtension(Imm, 12);
                    break;
                case InstructionType.I:
                    if (Opcode == 0b1100111)
                    {
                        // JALR
                        Imm = instr >> 20;
                    }
                    else
                    {
                        imm110 = instr >> 20;
                        Imm = imm110;
                    }
                    Imm = SignExtension(Imm, 11);
                    break;
                case InstructionType.S:
                    imm115 = instr >> 25;
                    imm40 = instr >> 7 & 0b11111;
                    Imm = imm115 << 5 | imm40;
                    Imm = SignExtension(Imm, 11);
                    break;
                case InstructionType.R:
                    Imm = instr >> 25 & 0b11111;
                    break;
            }
        }

        public override string ToString()
        {
            return $"Type: {Type}, Opcode: 0x{Opcode:X2}, Rd: {Rd}, Funct3: 0x{Funct3:X2}, Rs1: {Rs1}, Rs2: {Rs2}, Funct7: 0x{Funct7:X2}, Imm: 0x{Imm:X8}";
        }

        public static uint GetCodeBase(string t, int op1, int op2, int op3)
        {
            uint code = 0;
            uint imm20, imm101, imm11, imm1912, imm12, imm105, imm41;
            uint offset;

            switch (t)
            {
                case "J":
                    imm20 = (uint)(op2 >> 20) & 0b1;
                    imm101 = (uint)(op2 >> 1) & 0b11_11111111;
                    imm11 = (uint)(op2 >> 11) & 0b1;
                    imm1912 = (uint)(op2 >> 12) & 0b11111111;
                    offset = imm20 << 19 | imm101 << 9 | imm11 << 8 | imm1912;
                    code = offset << 12 | (uint)op1 << 7 | 0b1101111;
                    break;
                case "B":
                    imm12 = (uint)(op3 >> 12) & 0b1;
                    imm105 = (uint)(op3 >> 5) & 0b111111;
                    imm41 = (uint)(op3 >> 1) & 0b1111;
                    imm11 = (uint)(op3 >> 11) & 0b1;
                    code = imm12 << 31 | imm105 << 25 | (uint)op2 << 20 | (uint)op1 << 15 | (0b000 << 12) | (imm41 << 8) | (imm11 << 7) | 0b1100011;
                    break;
                default:
                    throw new Exception($"Type {t} not supported");
            }
            return code;
        }

        public static uint GenCode(OpName opn, int op1, int op2, int op3)
        {
            uint code = 0;
            switch (opn)
            {
                case OpName.Lui:
                    code = (uint)op2 << 12 | (uint)(op1 << 7) | 0b0110111;
                    return code;
                case OpName.Auipc:
                    code = (uint)op2 << 12 | (uint)(op1 << 7) | 0b0010111;
                    return code;
                case OpName.Jal:
                    code = GetCodeBase("J", op1, op2, op3);
                    return code;
                case OpName.Jalr:
                    uint offset = (uint)op2 & 0b1111_11111111;
                    code = offset << 20 | (uint)(op3 << 15) | (uint)(op1 << 7) | 0b1100111;
                    return code;
                case OpName.Beq:
                    code = GetCodeBase("B", op1, op2, op3) | (0b000 << 12);
                    return code;
                case OpName.Bne:
                    code = GetCodeBase("B", op1, op2, op3) | (0b001 << 12);
                    return code;
                case OpName.Blt:
                    code = GetCodeBase("B", op1, op2, op3) | (0b100 << 12);
                    return code;
                case OpName.Bge:
                    code = GetCodeBase("B", op1, op2, op3) | (0b101 << 12);
                    return code;
                case OpName.Bltu:
                    code = GetCodeBase("B", op1, op2, op3) | (0b110 << 12);
                    return code;
                case OpName.Bgeu:
                    code = GetCodeBase("B", op1, op2, op3) | (0b111 << 12);
                    return code;
                case OpName.Lb:
                    code = (uint)op2 << 20 | (uint)op3 << 15 | (0b000 << 12) | (uint)op1 << 7 | 0b0000011;
                    return code;
                case OpName.Lh:
                    code = (uint)op2 << 20 | (uint)op3 << 15 | (0b001 << 12) | (uint)op1 << 7 | 0b0000011;
                    return code;
                case OpName.Lw:
                    code = (uint)op2 << 20 | (uint)op3 << 15 | (0b010 << 12) | (uint)op1 << 7 | 0b0000011;
                    return code;
                case OpName.Lbu:
                    code = (uint)op2 << 20 | (uint)op3 << 15 | (0b100 << 12) | (uint)op1 << 7 | 0b0000011;
                    return code;
                case OpName.Lhu:
                    code = (uint)op2 << 20 | (uint)op3 << 15 | (0b101 << 12) | (uint)op1 << 7 | 0b0000011;
                    return code;
                case OpName.Sb:
                    uint imm115 = (uint)op2 >> 5 & 0b1111111;
                    uint imm40 = (uint)op2 & 0b11111;
                    code = imm115 << 25 | (uint)op1 << 20 | (uint)op3 << 15 | (0b000 << 12) | (imm40 << 7) | 0b0100011;
                    return code;
                case OpName.Sh:
                    imm115 = (uint)op2 >> 5 & 0b1111111;
                    imm40 = (uint)op2 & 0b11111;
                    code = imm115 << 25 | (uint)op1 << 20 | (uint)op3 << 15 | (0b001 << 12) | (imm40 << 7) | 0b0100011;
                    return code;
                case OpName.Sw:
                    imm115 = (uint)op2 >> 5 & 0b1111111;
                    imm40 = (uint)op2 & 0b11111;
                    code = imm115 << 25 | (uint)op1 << 20 | (uint)op3 << 15 | (0b010 << 12) | (imm40 << 7) | 0b0100011;
                    return code;
                case OpName.Addi:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Slti:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b010 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Sltiu:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b011 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Xori:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b100 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Ori:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b110 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Andi:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b111 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Slli:
                    uint shamt = (uint)op3 & 0b11111;
                    code = shamt << 20 | (uint)op2 << 15 | (0b001 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Srli:
                    shamt = (uint)op3 & 0b11111;
                    code = shamt << 20 | (uint)op2 << 15 | (0b101 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Srai:
                    shamt = (uint)op3 & 0b11111;
                    code = (0b1 << 30) | shamt << 20 | (uint)op2 << 15 | (0b101 << 12) | (uint)op1 << 7 | 0b0010011;
                    return code;
                case OpName.Add:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Sub:
                    code = (0b01000 << 27) | (uint)op3 << 20 | (uint)op2 << 15 | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Sll:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b001 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Slt:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b010 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Sltu:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b011 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Xor:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b100 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Srl:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b101 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Sra:
                    code = (0b1 << 30) | (uint)op3 << 20 | (uint)op2 << 15 | (0b101 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.Or:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b110 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                case OpName.And:
                    code = (uint)op3 << 20 | (uint)op2 << 15 | (0b111 << 12) | (uint)op1 << 7 | 0b0110011;
                    return code;
                default:
                    throw new Exception($"OpName {opn} not supported");
            }
        }

        public InstructionType GetInstructionType()
        {
            switch (Opcode)
            {
                case 0b0110111:
                case 0b0010111:
                    return InstructionType.U;
                case 0b1101111:
                    return InstructionType.J;
                case 0b1100011:
                    return InstructionType.B;
                case 0b0000011:
                case 0b1100111:
                    return InstructionType.I;
                case 0b0010011:
                    if (Funct3 == 0b001 || Funct3 == 0b101)
                    {
                        return InstructionType.R;
                    }
                    return InstructionType.I;
                case 0b0100011:
                    return InstructionType.S;
                case 0b0110011:
                    return InstructionType.R;
                case 0b0001111:
                    return InstructionType.F;
                case 0b1110011:
                    return InstructionType.C;
                default:
                    throw new Exception($"Opcode 0x{Opcode:X7} not supported");
            }
        }


        public OpName GetOpName()
        {
            switch (Type)
            {
                case InstructionType.U:
                    switch (Opcode)
                    {
                        case 0b0110111:
                            return OpName.Lui;
                        case 0b0010111:
                            return OpName.Auipc;
                        default:
                            throw new Exception($"Opcode: {Opcode:X7} is invalid for {Type}");
                    }
                case InstructionType.J:
                    switch (Opcode)
                    {
                        case 0b1101111:
                            return OpName.Jal;
                        default:
                            throw new Exception($"Opcode: {Opcode:X7} is invalid for {Type}");
                    }
                case InstructionType.B:
                    switch (Funct3)
                    {
                        case 0b000:
                            return OpName.Beq;
                        case 0b001:
                            return OpName.Bne;
                        case 0b100:
                            return OpName.Blt;
                        case 0b101:
                            return OpName.Bge;
                        case 0b110:
                            return OpName.Bltu;
                        case 0b111:
                            return OpName.Bgeu;
                        default:
                            throw new Exception($"Funct3: {Funct3:X3} is invalid for {Type}");
                    }
                case InstructionType.I:
                    switch (Opcode)
                    {
                        case 0b1100111:
                            return OpName.Jalr;
                        case 0b0000011:
                            // L*
                            switch (Funct3)
                            {
                                case 0b000:
                                    return OpName.Lb;
                                case 0b001:
                                    return OpName.Lh;
                                case 0b010:
                                    return OpName.Lw;
                                case 0b100:
                                    return OpName.Lbu;
                                case 0b101:
                                    return OpName.Lhu;
                                default:
                                    throw new Exception($"Opcode: {Opcode:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                            }
                        case 0b0010011:
                            // ADDI, SLTI, ...
                            switch (Funct3)
                            {
                                case 0b000:
                                    return OpName.Addi;
                                case 0b010:
                                    return OpName.Slti;
                                case 0b011:
                                    return OpName.Sltiu;
                                case 0b100:
                                    return OpName.Xori;
                                case 0b110:
                                    return OpName.Ori;
                                case 0b111:
                                    return OpName.Andi;
                                default:
                                    throw new Exception($"Opcode: {Opcode:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                            }
                        default:
                            throw new Exception($"Opcode: {Opcode:X7} is invalid for {Type}");
                    }
                case InstructionType.S:
                    switch (Funct3)
                    {
                        case 0b000:
                            return OpName.Sb;
                        case 0b001:
                            return OpName.Sh;
                        case 0b010:
                            return OpName.Sw;
                        default:
                            throw new Exception($"Funct3: {Funct3:X3} is invalid for {Type}");
                    }
                case InstructionType.R:
                    switch (Opcode)
                    {
                        case 0b0010011:
                            switch (Funct3)
                            {
                                case 0b001:
                                    return OpName.Slli;
                                case 0b101:
                                    switch (Funct7)
                                    {
                                        case 0b0000000:
                                            return OpName.Srli;
                                        case 0b0100000:
                                            return OpName.Srai;
                                        default:
                                            throw new Exception($"Opcode: {Opcode:X7}, Funct7: {Funct7:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                                    }
                                default:
                                    throw new Exception($"Opcode: {Opcode:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                            }
                        case 0b0110011:
                            switch (Funct3)
                            {
                                case 0b000:
                                    switch (Funct7)
                                    {
                                        case 0b0000000:
                                            return OpName.Add;
                                        case 0b0100000:
                                            return OpName.Sub;
                                        default:
                                            throw new Exception($"Opcode: {Opcode:X7}, Funct7: {Funct7:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                                    }
                                case 0b001:
                                    return OpName.Sll;
                                case 0b010:
                                    return OpName.Slt;
                                case 0b011:
                                    return OpName.Sltu;
                                case 0b100:
                                    return OpName.Xor;
                                case 0b101:
                                    switch (Funct7)
                                    {
                                        case 0b0000000:
                                            return OpName.Srl;
                                        case 0b0100000:
                                            return OpName.Sra;
                                        default:
                                            throw new Exception($"Opcode: {Opcode:X7}, Funct7: {Funct7:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                                    }
                                case 0b110:
                                    return OpName.Or;
                                case 0b111:
                                    return OpName.And;
                                default:
                                    throw new Exception($"Opcode: {Opcode:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                            }
                        default:
                            throw new Exception($"Opcode: {Opcode:X7} is invalid for {Type}");
                    }
                case InstructionType.F:
                    switch (Funct3)
                    {
                        case 0b000:
                            return OpName.Fence;
                        case 0b001:
                            return OpName.FenceI;
                        default:
                            throw new Exception($"Funct3: {Funct3:X3} is invalid for {Type}");
                    }
                case InstructionType.C:
                    switch (Funct3)
                    {
                        case 0b000:
                            switch (Rs2)
                            {
                                case 0:
                                    return OpName.Ecall;
                                case 1:
                                    return OpName.Ebreak;
                                default:
                                    throw new Exception($"Opcode: {Opcode:X7}, Rs2: {Rs2}, Funct3: {Funct3:X3} is invalid for {Type}");
                            }
                        case 0b001:
                            return OpName.Csrrw;
                        case 0b010:
                            return OpName.Csrrs;
                        case 0b011:
                            return OpName.Csrrc;
                        case 0b101:
                            return OpName.Csrrwi;
                        case 0b110:
                            return OpName.Csrrsi;
                        case 0b111:
                            return OpName.Csrrci;
                        default:
                            throw new Exception($"Opcode: {Opcode:X7}, Funct3: {Funct3:X3} is invalid for {Type}");
                    }
                default:
                    throw new Exception($"Instruction type {Type} not supported");
            }
        }
        public string GetCodeString()
        {
            switch (GetInstructionType())
            {
                case InstructionType.R:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rd)}, {RegName(Rs1)}, {RegName(Rs2)}";
                case InstructionType.I:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rd)}, {SignExtension(Imm, 12)}({RegName(Rs1)})";
                case InstructionType.S:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rs1)}, {SignExtension(Imm, 12)}({RegName(Rs2)})";
                case InstructionType.B:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rs1)}, {RegName(Rs2)}, {SignExtension(Imm, 13)}";
                case InstructionType.U:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rd)}, {SignExtension(Imm >> 12, 20)}";
                case InstructionType.J:
                    return $"{GetOpName().ToString().Substring(2)} {RegName(Rd)}, PC+0x{SignExtension(Imm, 21):X}";
                case InstructionType.F:
                    return $"{GetOpName().ToString().Substring(2)}(TBD)";
                case InstructionType.C:
                    return $"{GetOpName().ToString().Substring(2)}(TBD)";
                default:
                    return $"{GetOpName().ToString().Substring(2)}(TBD)";
            }
        }


        private uint SignExtension(uint imm, int digit)
        {
            uint sign = (imm >> digit) & 0b1;
            if (sign == 1)
            {
                uint mask = (uint)0xFFFFFFFF - ((uint)(1 << digit) - 1);
                imm = mask | imm;
            }
            return imm;
        }

        private string RegName(byte reg)
        {
            // TODO: Implement register name mapping
            return $"x{reg}";
        }
    }
}