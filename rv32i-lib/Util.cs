using System;

namespace RV32I
{
    public static class Util
    {
        public static uint SignExtension(uint imm, int digit)
        {
            uint sign = (imm >> digit) & 0b1;
            if (sign == 1)
            {
                uint mask = uint.MaxValue - (uint)(1 << digit) + 1;
                imm = mask | imm;
            }
            return imm;
        }

        public static int InterpretSignedUint32(uint imm)
        {
            uint sign = (imm >> 31) & 0b1;
            if (sign == 1)
            {
                uint x = uint.MaxValue ^ imm;
                x++;
                return (int)x * -1;
            }
            return (int)imm;
        }

        public static byte ByteString2U8(char b)
        {
            if (b >= '0' && b <= '9')
            {
                return (byte)(b - '0');
            }
            else if (b >= 'a' && b <= 'f')
            {
                return (byte)(b - 'a' + 10);
            }
            else if (b >= 'A' && b <= 'F')
            {
                return (byte)(b - 'A' + 10);
            }
            else
            {
                throw new Exception($"byte 0x{b:X2} not supported");
            }
        }

        public static int Abs(int v)
        {
            int y = v >> (sizeof(int) * 8 - 1);
            return (v ^ y) - y;
        }

        public static void ChkErr(Exception err)
        {
            if (err != null)
            {
                throw err;
            }
        }
    }
}