using System;
using System.Collections.Generic;

namespace RV32I
{
    public class Cpu
    {
        // Register ABIName Description                         Saver
        // -----------------------------------------------------------
        // x0       zero    Hard-wired zero                     —
        // x1       ra      Return address                      Caller
        // x2       sp      Stack pointer                       Callee
        // x3       gp      Global pointer                      —
        // x4       tp      Thread pointer                      —
        // x5–7     t0–2    Temporaries                         Caller
        // x8       s0/fp   Saved register/frame pointer        Callee
        // x9       s1      Saved register                      Callee
        // x10–11   a0–1    Function arguments/return values    Caller
        // x12–17   a2–7    Function arguments                  Caller
        // x18–27   s2–11   Saved registers                     Callee
        // x28–31   t3–6    Temporaries                         Caller

        private Dictionary<string, int> Regs = new Dictionary<string, int>()
        {
            {"zero", 0},
            {"ra", 1},
            {"sp", 2},
            {"gp", 3},
            {"tp", 4},
            {"t0", 5},
            {"t1", 6},
            {"t2", 7},
            {"s0", 8},
            {"fp", 8},
            {"s1", 9},
            {"a0", 10},
            {"a1", 11},
            {"a2", 12},
            {"a3", 13},
            {"a4", 14},
            {"a5", 15},
            {"a6", 16},
            {"a7", 17},
            {"s2", 18},
            {"s3", 19},
            {"s4", 20},
            {"s5", 21},
            {"s6", 22},
            {"s7", 23},
            {"s8", 24},
            {"s9", 25},
            {"s10", 26},
            {"s11", 27},
            {"t3", 28},
            {"t4", 29},
            {"t5", 30},
            {"t6", 31},
            {"x0", 0},
            {"x1", 1},
            {"x2", 2},
            {"x3", 3},
            {"x4", 4},
            {"x5", 5},
            {"x6", 6},
            {"x7", 7},
            {"x8", 8},
            {"x9", 9},
            {"x10", 10},
            {"x11", 11},
            {"x12", 12},
            {"x13", 13},
            {"x14", 14},
            {"x15", 15},
            {"x16", 16},
            {"x17", 17},
            {"x18", 18},
            {"x19", 19},
            {"x20", 20},
            {"x21", 21},
            {"x22", 22},
            {"x23", 23},
            {"x24", 24},
            {"x25", 25},
            {"x26", 26},
            {"x27", 27},
            {"x28", 28},
            {"x29", 29},
            {"x30", 30},
            {"x31", 31}
        };

        private Dictionary<int, string> RegsR = new Dictionary<int, string>()
        {
            {0, "zero"},
            {1, "ra"},
            {2, "sp"},
            {3, "gp"},
            {4, "tp"},
            {5, "t0"},
            {6, "t1"},
            {7, "t2"},
            {8, "s0"}, // s0/fp
            {9, "s1"},
            {10, "a0"},
            {11, "a1"}, // a1/ret
            {12, "a2"}, // a2/ret
            {13, "a3"},
            {14, "a4"},
            {15, "a5"},
            {16, "a6"},
            {17, "a7"},
            {18, "s2"},
            {19, "s3"},
            {20, "s4"},
            {21, "s5"},
            {22, "s6"},
            {23, "s7"},
            {24, "s8"},
            {25, "s9"},
            {26, "s10"},
            {27, "s11"},
            {28, "t3"},
            {29, "t4"},
            {30, "t5"},
            {31, "t6"}
        };

        public uint[] X { get; set; }
        public uint PC { get; set; }
        public Emulator? Emu { get; set; }// registers
                                          // program counter
        public string RegName(byte i)
        {
            return RegsR[i];
        }

        public Cpu()
        {
            X = new uint[32];
            PC = 0;
            Emu = null;
        }

        public void Reset()
        {
            X = new uint[32];
            PC = 0;
        }

        public void Step()
        {
            uint u32instr = Fetch();
            Log.Trace($"PC: 0x{PC:X8}, u32instr: {u32instr:X8}");
            // fetch
            Instruction instr = new Instruction(u32instr);
            Log.Trace($"instr: {instr}");

            bool incrementPC = Execute(instr);// decode

            if (incrementPC)
            {
                PC += 4;// execute
            }
        }
        // increment PC if it's not jump
        public void DumpRegisters()
        {
            Log.Info("* Registers");
            for (int i = 0; i < X.Length; i++)
            {
                Log.Info($"x{i} = {X[i]}, 0x{X[i]:X8}");
            }
            Log.Info($"pc = 0x{PC:X8}");
        }

        private uint Fetch()
        {
            if (PC > Emulator.MaxMemory)
            {
                throw new Exception("PC overflow");
            }
            return Emu.ReadU32(PC);
        }

        private bool Execute(Instruction i)
        {
            var op = i.GetOpName();
            bool incrementPC = true;
            uint shamt;

            switch (op)
            {
                case OpName.Lui:
                    Log.Trace($"lui: X[{i.Rd}] <- {i.Imm:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = i.Imm;
                    }
                    break;
                case OpName.Auipc:
                    Log.Trace($"auipc: X[{i.Rd}] <- PC:{PC:x} + imm:{i.Imm:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = PC + i.Imm;
                    }
                    break;
                case OpName.Jal:
                    uint t = PC + 4;
                    PC += i.Imm;
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = t;
                    }
                    Log.Trace($"jal: PC={PC:x}, X[{i.Rd}]={t:x}");
                    incrementPC = false;
                    break;
                case OpName.Jalr:
                    t = PC + 4;
                    PC = (X[i.Rs1] + i.Imm) & 0xffffffe;
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = t;
                    }
                    Log.Trace($"jalr: PC={PC:x}, X[{i.Rd}]={t:x}");
                    incrementPC = false;
                    break;
                case OpName.Beq:
                    Log.Trace($"beq: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (X[i.Rs1] == X[i.Rs2])
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Bne:
                    Log.Trace($"bne: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (X[i.Rs1] != X[i.Rs2])
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Blt:
                    int a = (int)X[i.Rs1];
                    int b = (int)X[i.Rs2];
                    Log.Trace($"blt: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (a < b)
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Bge:
                    a = (int)X[i.Rs1];
                    b = (int)X[i.Rs2];
                    Log.Trace($"bge: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (a >= b)
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Bltu:
                    Log.Trace($"bltu: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (X[i.Rs1] < X[i.Rs2])
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Bgeu:
                    Log.Trace($"bgeu: Rs1:{i.Rs1:x}, Rs2:{i.Rs2:x}");
                    if (X[i.Rs1] >= X[i.Rs2])
                    {
                        PC += i.Imm;
                        incrementPC = false;
                    }
                    break;
                case OpName.Lb:
                    uint addr = X[i.Rs1] + i.Imm;
                    Log.Trace($"lb: read {addr:x} -> X[{i.Rd}]");
                    uint data = Emu.ReadU8(addr);
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = Util.SignExtension(data, 7);
                    }
                    break;
                case OpName.Lh:
                    addr = X[i.Rs1] + i.Imm;
                    Log.Trace($"lh: read {addr:x} -> X[{i.Rd}]");
                    data = Emu.ReadU16(addr);
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = Util.SignExtension(data, 15);
                    }
                    break;
                case OpName.Lw:
                    addr = X[i.Rs1] + i.Imm;
                    Log.Trace($"lw: read {addr:x} -> X[{i.Rd}]");
                    data = Emu.ReadU32(addr);
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = data;
                    }
                    break;
                case OpName.Lbu:
                    addr = X[i.Rs1] + i.Imm;
                    Log.Trace($"lbu: read {addr:x} -> X[{i.Rd}]");
                    data = Emu.ReadU8(addr);
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = data;
                    }
                    break;
                case OpName.Lhu:
                    addr = X[i.Rs1] + i.Imm;
                    Log.Trace($"lhu: read {addr:x} -> X[{i.Rd}]");
                    data = Emu.ReadU16(addr);
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = data;
                    }
                    break;
                case OpName.Sb:
                    addr = X[i.Rs1] + i.Imm;
                    byte byteData = (byte)(X[i.Rs2] & 0xFF);
                    Log.Trace($"sb: write {byteData:x} at {addr:x}");
                    Emu.WriteU8(addr, byteData);
                    break;
                case OpName.Sh:
                    addr = X[i.Rs1] + i.Imm;
                    ushort shortData = (ushort)(X[i.Rs2] & 0xFFFF);
                    Log.Trace($"sh: write {shortData:x} at {addr:x}");
                    Emu.WriteU16(addr, shortData);
                    break;
                case OpName.Sw:
                    addr = X[i.Rs1] + i.Imm;
                    data = X[i.Rs2];
                    Log.Trace($"sw: write {data:x} at {addr:x}");
                    Emu.WriteU32(addr, data);
                    break;
                case OpName.Addi:
                    Log.Trace($"addi: rs1:{i.Rs1:x} + imm:{i.Imm:x} -> rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] + i.Imm;
                    }
                    break;
                case OpName.Slti:
                    Log.Trace($"slti: rs1:{i.Rs1:x}, imm:{i.Imm:x}, rd:{i.Rd:x}");
                    if ((int)X[i.Rs1] < (int)i.Imm)
                    {
                        if (i.Rd > 0)
                        {
                            X[i.Rd] = 1;
                        }
                    }
                    else
                    {
                        X[i.Rd] = 0;
                    }
                    break;
                case OpName.Sltiu:
                    Log.Trace($"sltiu: rs1:{i.Rs1:x}, imm:{i.Imm:x}, rd:{i.Rd:x}");
                    if (X[i.Rs1] < i.Imm)
                    {
                        if (i.Rd > 0)
                        {
                            X[i.Rd] = 1;
                        }
                    }
                    else
                    {
                        X[i.Rd] = 0;
                    }
                    break;
                case OpName.Xori:
                    Log.Trace($"xori: rs1:{i.Rs1:x}, imm:{i.Imm:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] ^ i.Imm;
                    }
                    break;
                case OpName.Ori:
                    Log.Trace($"ori: rs1:{i.Rs1:x}, imm:{i.Imm:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] | i.Imm;
                    }
                    break;
                case OpName.Andi:
                    Log.Trace($"andi: rs1:{i.Rs1:x}, imm:{i.Imm:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] & i.Imm;
                    }
                    break;
                case OpName.Slli:
                    Log.Trace($"slli: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] << (int)i.Rs2;
                    }
                    break;
                case OpName.Srli:
                    Log.Trace($"srli: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] >> (int)i.Rs2;
                    }
                    break;
                case OpName.Srai:
                    Log.Trace($"srai: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    data = X[i.Rs1] >> (int)i.Rs2;
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = Util.SignExtension(data, 31 - (int)i.Rs2);
                    }
                    break;
                case OpName.Add:
                    Log.Trace($"add: rs1:{i.Rs1:x} + rs2:{i.Rs2:x} -> rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] + X[i.Rs2];
                    }
                    break;
                case OpName.Sub:
                    Log.Trace($"sub: rs1:{i.Rs1:x} + rs2:{i.Rs2:x} -> rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] - X[i.Rs2];
                    }
                    break;
                case OpName.Sll:
                    Log.Trace($"sll: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] << (int)X[i.Rs2];
                    }
                    break;
                case OpName.Slt:
                    Log.Trace($"slt: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if ((int)X[i.Rs1] < (int)X[i.Rs2])
                    {
                        if (i.Rd > 0)
                        {
                            X[i.Rd] = 1;
                        }
                    }
                    else
                    {
                        X[i.Rd] = 0;
                    }
                    break;
                case OpName.Sltu:
                    Log.Trace($"sltu: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (X[i.Rs1] < X[i.Rs2])
                    {
                        if (i.Rd > 0)
                        {
                            X[i.Rd] = 1;
                        }
                    }
                    else
                    {
                        X[i.Rd] = 0;
                    }
                    break;
                case OpName.Xor:
                    Log.Trace($"xor: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] ^ X[i.Rs2];
                    }
                    break;
                case OpName.Srl:
                    Log.Trace($"srl: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        shamt = 0b11111 & X[i.Rs2];
                        X[i.Rd] = X[i.Rs1] >> (int)shamt;
                    }
                    break;
                case OpName.Sra:
                    Log.Trace($"sra: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    shamt = 0b11111 & X[i.Rs2];
                    data = X[i.Rs1] >> (int)shamt;
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = Util.SignExtension(data, 31 - (int)shamt);
                    }
                    break;
                case OpName.Or:
                    Log.Trace($"or: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] | X[i.Rs2];
                    }
                    break;
                case OpName.And:
                    Log.Trace($"and: rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}");
                    if (i.Rd > 0)
                    {
                        X[i.Rd] = X[i.Rs1] & X[i.Rs2];
                    }
                    break;
                case OpName.Fence:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.FenceI:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Ecall:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Ebreak:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrw:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrs:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrc:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrwi:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrsi:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                case OpName.Csrrci:
                    Log.Warn($"Op {op} is not implemented yet. rs1:{i.Rs1:x}, rs2:{i.Rs2:x}, rd:{i.Rd:x}, imm:{i.Imm:x}");
                    break;
                default:
                    throw new Exception($"Op: {op} invalid");
            }
            return incrementPC;
        }
    }
}