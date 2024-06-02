using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RV32I
{
    public class Loader
    {
        public Loader()
        {
        }

        public void LoadAt(string filePath, byte[] loadAddr, uint maxSize)
        {
            string ext = Path.GetExtension(filePath);

            if (ext == ".txt")
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    ReadInto(reader, loadAddr, maxSize);
                }
            }
            else
            {
                ReadBinary(filePath, loadAddr);
            }
        }

        public void LoadStringAt(string data, byte[] loadAddr, uint maxSize)
        {
            using (StringReader reader = new StringReader(data))
            {
                ReadInto(reader, loadAddr, maxSize);
            }
        }

        private void ReadInto(TextReader reader, byte[] loadAddr, uint maxSize)
        {
            uint[] mem = ReadText(reader);
            for (int idx = 0; idx < mem.Length; idx++)
            {
                uint u32 = mem[idx];
                loadAddr[idx * 4] = (byte)(u32 & 0x000000FF);
                loadAddr[idx * 4 + 1] = (byte)((u32 & 0x0000FF00) >> 8);
                loadAddr[idx * 4 + 2] = (byte)((u32 & 0x00FF0000) >> 16);
                loadAddr[idx * 4 + 3] = (byte)((u32 & 0xFF000000) >> 24);
            }
        }

        public void TextToBinary(string pathIn, string pathOut)
        {
            uint[] p;
            using (StreamReader reader = new StreamReader(pathIn))
            {
                p = ReadText(reader);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open(pathOut, FileMode.Create)))
            {
                foreach (uint u32 in p)
                {
                    byte[] by4 = new byte[4];
                    by4[0] = (byte)(u32 & 0x000000FF);
                    by4[1] = (byte)((u32 & 0x0000FF00) >> 8);
                    by4[2] = (byte)((u32 & 0x00FF0000) >> 16);
                    by4[3] = (byte)((u32 & 0xFF000000) >> 24);
                    writer.Write(by4);
                }
            }
        }

        private uint[] ReadText(TextReader reader)
        {
            var ba = new System.Collections.Generic.List<uint>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length < 20)
                {
                    // not a valid instruction line
                    continue;
                }

                if (line[9] == '<')
                {
                    // label
                    continue;
                }

                uint u32 = 0;
                if (line.Substring(10, 2) == "0x")
                {
                    // syntax:
                    //       0: 0x00000093 Addi ra, 0(zero)
                    u32 = Convert.ToUInt32(line.Substring(12, 8), 16);
                }
                else
                {
                    // syntax:
                    //       0: 93 00 00 00   li      ra, 0
                    if (line.Length < 21)
                    {
                        // not a valid instruction line
                        continue;
                    }

                    int s = 0;
                    for (int i = 10; i < 21; i += 3)
                    {
                        byte by = (byte)(ByteString2U8(line[i]) * 16 + ByteString2U8(line[i + 1]));
                        u32 += (uint)(by << s);
                        s += 8;
                    }
                }

                ba.Add(u32);
            }

            return ba.ToArray();
        }

        private void ReadBinary(string path, byte[] ba)
        {
            byte[] tmp = File.ReadAllBytes(path);
            Array.Copy(tmp, ba, tmp.Length);
        }

        private byte ByteString2U8(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            else if (c >= 'a' && c <= 'f')
            {
                return (byte)(c - 'a' + 10);
            }
            else if (c >= 'A' && c <= 'F')
            {
                return (byte)(c - 'A' + 10);
            }
            else
            {
                throw new ArgumentException("Invalid byte string character: " + c);
            }
        }
    }
}