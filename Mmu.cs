using System;

namespace GameBoy
{
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
			Memory[address + 1] = (byte)((value & 0x0F) >> 8);
			Memory[address] = (byte)(value & 0x0F);
		}

		public Span<byte> Read(Range range)
		{
			return Memory[range].AsSpan();
		}
	}
}
