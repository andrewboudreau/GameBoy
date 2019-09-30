using System;

namespace GameBoy
{
    /// <summary>
    /// GameBoy Memory Management Unit
    /// </summary>
    public class Mmu
    {
        public byte[] Memory = new byte[0XFFFFF];

        public byte ReadByte(ushort address)
        {
            return Memory[address];
        }

        public byte ReadByte(int address)
        {
            return ReadByte((ushort)address);
        }

        public void WriteByte(ushort address, byte value)
        {
            Memory[address] = value;
        }
        public void WriteByte(int address, byte value)
        {
            WriteByte((ushort)address, value);
        }

        public void WriteByte(ushort address, int value)
        {
            if (value < 0)
            {
                throw new InvalidOperationException($"Cannot write negative number '{value}' to address ${address:x4}");
            }

            WriteByte(address, (ushort)value);
        }

        public ushort ReadWord(ushort address)
        {
            var result = (ushort)((Memory[address + 1] << 8) + Memory[address]);
            return result;
        }

        public ushort ReadWord(int address)
        {
            return ReadWord((ushort)address);
        }

        public void WriteWord(ushort address, ushort value)
        {
            Memory[address + 1] = (byte)((value & 0xFF00) >> 8);
            Memory[address] = (byte)(value & 0x00FF);
        }

        public void WriteWord(ushort address, int value)
        {
            WriteWord(address, (ushort)value);
        }

        public Span<byte> Read(Range range)
        {
            return new Span<byte>(Memory)[range];
        }
    }
}
