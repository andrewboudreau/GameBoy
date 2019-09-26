using System;

namespace GameBoy
{
    public partial class Z80A
    {
        private void PerfixCB()
        {
            CB[Mmu.ReadByte(Registers.PC + 1)]();
            Registers.PC += 2;
            Registers.M += 2;
        }

        byte LDd8(string name)
        {
            //LD C,d8
            //2  8
            var data = Mmu.ReadByte(Registers.PC + 1);
            DebugOutputLine($"LD {name}, d8(${data:X1})");

            Registers.PC += 2;
            Registers.M += 2;
            return data;
        }

        ushort LDd16(string name)
        {
            var data = Mmu.ReadWord(Registers.PC + 1);
            DebugOutputLine($"LD {name}, d16(${data:X2})");

            Registers.PC += 3;
            Registers.M += 3;
            return data;
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

            Registers.FH = (value & 0x0F) == 0x0F;
            Registers.FZ = Registers.C == 0;
            Registers.FN = false;

            Registers.PC += 1;
            Registers.M += 4 / 4;

            return (byte)(value + 1);
        }

        public void JRccnn(bool test, string name, byte nn)
        {
            DebugOutputLine($"JR {name}({test.ToString()}), {(sbyte)nn}");

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

        public void BIT_br(byte bit, byte value, string name)
        {
            if (bit < 0 || bit > 7)
            {
                throw new ArgumentOutOfRangeException("Bit to test must be between 0-7");
            }

            DebugOutputLine($"BIT {bit}, {name}");
            var result = value & (1 << bit);
            Registers.FZ = result == 0;
            Registers.FN = false;
            Registers.FH = true;
        }

        public void BIT_b_addr(byte bit, byte value)
        {
            throw new NotImplementedException("figure out how to render addr correctly.");
            ////BIT_br(bit, value, "addr");
            ////Registers.M += 4;
        }

        public byte SET_br(byte bit, byte value)
        {
            if (bit < 0 || bit > 7)
            {
                throw new ArgumentOutOfRangeException("Bit to test must be between 0-7");
            }

            Registers.M += 4;
            return (byte)(value | (1 << bit));
        }

        public void SET_b_addr(byte bit, byte value)
        {
            SET_br(bit, value);
            Registers.M += 4;
        }
    }
}
