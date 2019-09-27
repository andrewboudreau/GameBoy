using System;

namespace GameBoy
{
    public partial class Z80A
    {
        private void PerfixCB()
        {
            CB[Mmu.ReadByte(Registers.PC + 1)]();
            Registers.PC += 2;
            Registers.M += 4 / 4;
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

        public byte RL(byte value, string name)
        {
            DebugOutputLine($"RL {name} #{value:X2} 0b{Convert.ToString(value, 2)} FC={Registers.FC}");
            var carry = Registers.FC ? 1 : 0;
            var result = (byte)((value << 1) + carry);

            Registers.FC = value > 0x80;
            Registers.FH = false;
            Registers.FN = false;
            Registers.FZ = result == 0;

            Registers.M += 8 / 4;

            return result;
        }
    }
}
