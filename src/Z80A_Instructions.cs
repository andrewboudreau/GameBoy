using System;

namespace GameBoy
{
    public partial class Z80A
    {
        byte LDd8(string name)
        {
            //LD C,d8
            //2  8
            var value = Mmu.ReadByte(Registers.PC + 1);
            DebugOutputLine($"LD {name}, ${value:X1}");

            Registers.PC += 2;
            Registers.M += 2;
            return value;
        }

        ushort LDd16(string name)
        {
            var value = Mmu.ReadWord(Registers.PC + 1);
            DebugOutputLine($"LD {name}, ${value:X2} {value}");

            Registers.PC += 3;
            Registers.M += 3;
            return value;
        }

        byte LDnA(string name)
        {
            DebugOutputLine($"LD {name}, A #{Registers.A:X2} {Registers.A}");

            Registers.PC += 1;
            Registers.M += 4 / 4;
            return Registers.A;
        }

        void XOR_A(byte input, string name)
        {
            DebugOutputLine($"XOR {name}, ${input:X2}");

            Registers.A ^= input;
            Registers.PC += 1;
            Registers.M += 1;
            Registers.FZ = Registers.A == 0;
        }

        void LDBCmA()
        {
            throw new NotImplementedException("Doesn't increment PC");
            ////Registers.B = Mmu.ReadByte((ushort)(Registers.PC + 1));
            ////Registers.C = Mmu.ReadByte((ushort)(Registers.PC + 2));
            ////Registers.M += 3;
        }

        public byte Increment(byte value, string name)
        {
            // INC C 
            // 1  4, 
            // Z - Set if result is zero.
            // N - Reset.
            // H - Set if carry from bit 3.
            // C - Not affected.

            DebugOutputLine($"INC {name} #{value:X2}+1 {value}");
            var new_value = (byte)(value + 1);

            Registers.FZ = new_value == 0;
            Registers.FN = false;
            /*
              Op           Before         | After
                bbbb_1111     1110_1111   |   1111_0000
              & 0000_1111   & 0000_1111   | & 0000_1111
              ===========   ===========   | ===========
                0000_1111     0000_1111   |   0000_0000
            */

            Registers.FH = (value & 0x0F) == 0x0F;
            /// Registers.FC = Registers.FC;

            Registers.PC += 1;
            Registers.M += 4 / 4;

            return new_value;
        }

        public byte Decrement(byte value, string name)
        {
            // DEC C 
            // 1  4, 
            // Z - Set if reselt is zero.
            // N - Set.
            // H - Set if no borrow from bit 4
            // C - Not affected.

            DebugOutputLine($"DEC {name} #{value:X2}-1 {value}");
            var new_value = (byte)(value - 1);

            Registers.FZ = new_value == 0;
            Registers.FN = true;

            /*
               Op           Before         | After
                 bbbb_1111     1110_0000   |   1101_1111
               & 0000_1111   & 0000_1111   | & 0000_1111
               ===========   ===========   | ===========
                 0000_0000     0000_0000   |   0000_1111
             */
            Registers.FH = (value & 0x0F) == 0;
            /// Registers.FC = Registers.FC;

            Registers.PC += 1;
            Registers.M += 4 / 4;

            return new_value;
        }

        public void JRccnn(bool test, string name, byte nn)
        {
            DebugOutputLine($"JR {name}={test.ToString()}, {(sbyte)nn}");

            if (test)
            {
                if (Registers.PC + (sbyte)nn < 0)
                {
                    throw new InvalidOperationException($"Instruction JR, jump relative cannot move Program Counter below 0. PC={Registers.PC}, Offset={(sbyte)nn}");
                }

                Registers.PC = (ushort)(Registers.PC + (sbyte)nn);
                Registers.M += 1;
            }

            Registers.PC += 2;
            Registers.M += 2;
        }
    }
}
