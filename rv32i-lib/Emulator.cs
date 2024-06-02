using System;

namespace RV32I
{
    public class Emulator
    {
        public const uint MaxMemory = 0x10000;

        public Cpu Cpu { get; set; }
        public byte[] Memory { get; set; }

        public Emulator()
        {
            Cpu = new Cpu();
            Memory = new byte[MaxMemory];
            Cpu.Emu = this;
        }

        public void Reset()
        {
            Cpu.Reset();
            Memory = new byte[MaxMemory];
        }

        public void Load(string filePath)
        {
            Loader loader = new Loader();
            loader.LoadAt(filePath, Memory, MaxMemory);
        }

        public void LoadString(string data)
        {
            Loader loader = new Loader();
            loader.LoadStringAt(data, Memory, MaxMemory);
        }

        public void Step()
        {
            Cpu.Step();
        }

        public void Run()
        {
            while (true)
            {
                Cpu.Step();
            }
        }

        public void StepUntil(uint PC)
        {
            while (true)
            {
                if (Cpu.PC == PC)
                {
                    break;
                }
                Cpu.Step();
            }
        }

        public void Dump()
        {
            Cpu.DumpRegisters();
        }

        public void WriteU8(uint addr, byte data)
        {
            Memory[addr] = data;
        }

        public void WriteU16(uint addr, ushort data)
        {
            Memory[addr] = (byte)(data & 0x00FF);
            addr++;
            Memory[addr] = (byte)((data & 0xFF00) >> 8);
            addr++;
        }

        public void WriteU32(uint addr, uint data)
        {
            Memory[addr] = (byte)(data & 0x000000FF);
            addr++;
            Memory[addr] = (byte)((data & 0x0000FF00) >> 8);
            addr++;
            Memory[addr] = (byte)((data & 0x00FF0000) >> 16);
            addr++;
            Memory[addr] = (byte)((data & 0xFF000000) >> 24);
        }

        public byte ReadU8(uint addr)
        {
            return Memory[addr];
        }

        public ushort ReadU16(uint addr)
        {
            ushort data = (ushort)(Memory[addr] | (Memory[addr + 1] << 8));
            return data;
        }

        public uint ReadU32(uint addr)
        {
            uint data = (uint)(Memory[addr] | (Memory[addr + 1] << 8) | (Memory[addr + 2] << 16) | (Memory[addr + 3] << 24));
            return data;
        }
    }
}