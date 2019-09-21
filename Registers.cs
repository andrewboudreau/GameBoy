using System.Collections.Generic;

namespace GameBoy
{
	public struct Registers
	{
		/*
			Registers
			 16bit Hi   Lo   Name/Function
			 AF    A    -    Accumulator & Flags
			 BC    B    C    BC
			 DE    D    E    DE
			 HL    H    L    HL
			 SP    -    -    Stack Pointer
			 PC    -    -    Program Counter/Pointer

			As shown above, most registers can be accessed either as one 16bit register, or as two separate 8bit registers.
		*/
		public byte A { get; set; }
		public byte B { get; set; }
		public byte C { get; set; }
		public byte D { get; set; }
		public byte E { get; set; }
		public byte H { get; set; }
		public byte L { get; set; }

		public ushort AF
		{
			get
			{
				return (ushort)((A << 8) + F);
			}
			set
			{
				A = (byte)((value & 0XFF00) >> 8);
				F = (byte)(value & 0xFF);
			}
		}
		public ushort BC
		{
			get
			{
				return (ushort)((B << 8) + C);
			}
			set
			{
				B = (byte)((value & 0XFF00) >> 8);
				C = (byte)(value & 0xFF);
			}
		}
		public ushort DE
		{
			get
			{
				return (ushort)((D << 8) + E);
			}
			set
			{
				D = (byte)((value & 0XFF00) >> 8);
				E = (byte)(value & 0xFF);
			}
		}
		public ushort HL
		{
			get
			{
				return (ushort)((H << 8) + L);
			}
			set
			{
				H = (byte)((value & 0X00FF) >> 8);
				L = (byte)(value & 0x0F);
			}
		}

		public ushort SP { get; set; }

		public ushort PC { get; set; }

		/*
            The Flag Register (lower 8bit of AF register)
                Bit  Name  Set Clr  Expl.
                7    zf    Z   NZ   Zero Flag
                6    n     -   -    Add/Sub-Flag (BCD)
                5    h     -   -    Half Carry Flag (BCD)
                4    cy    C   NC   Carry Flag
                3-0  -     -   -    Not used (always zero)
            Conatins the result from the recent instruction which has affected flags.

            The Zero Flag (Z)
            This bit becomes set (1) if the result of an operation has been zero (0). Used for conditional jumps.

            The Carry Flag (C, or Cy)
            Becomes set when the result of an addition became bigger than FFh (8bit) or FFFFh (16bit). Or when the result of a subtraction or comparision became less than zero (much as for Z80 and 80x86 CPUs, but unlike as for 65XX and ARM CPUs). Also the flag becomes set when a rotate/shift operation has shifted-out a "1"-bit. Used for conditional jumps, and for instructions such like ADC, SBC, RL, RLA, etc.

            The BCD Flags (N, H)
            These flags are (rarely) used for the DAA instruction only, N Indicates whether the previous instruction has been an addition or subtraction, and H indicates carry for lower 4bits of the result, also for DAA, the C flag must indicate carry for upper 8bits. After adding/subtracting two BCD numbers, DAA is intended to convert the result into BCD format; BCD numbers are ranged from 00h to 99h rather than 00h to FFh. Because C and H flags must contain carry-outs for each digit, DAA cannot be used for 16bit operations (which have 4 digits), or for INC/DEC operations (which do not affect C-flag).
        */
		public bool FZ { get; set; }
		public bool FN { get; set; }
		public bool FH { get; set; }
		public bool FC { get; set; }

		public byte F
		{
			get
			{
				return (byte)(
					 (FZ ? (1 << 7) : 0) +
					 (FN ? (1 << 6) : 0) +
					 (FH ? (1 << 5) : 0) +
					 (FC ? (1 << 4) : 0)
				);
			}
			set
			{
				FZ = (value & (1 << 7)) > 1;
				FN = (value & (1 << 6)) > 1;
				FH = (value & (1 << 5)) > 1;
				FC = (value & (1 << 4)) > 1;
			}
		}

		// Clock For Instructions
		public uint M { get; set; }

		public IEnumerator<(string Name, byte Value)> GetEnumerator()
		{
			yield return ("A", A);
			yield return ("B", B);
			yield return ("C", C);
			yield return ("D", D);
			yield return ("E", E);
			yield return ("H", H);
			yield return ("L", L);
			yield return ("F", F);
		}
	}
}
