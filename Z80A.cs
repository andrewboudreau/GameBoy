using AndrewsWorld;
using System;

namespace GameBoy
{
    /*
        d8  means immediate 8 bit data
        d16 means immediate 16 bit data
        a8  means 8 bit unsigned data, which are added to $FF00 in certain instructions (replacement for missing IN and OUT instructions)
        a16 means 16 bit address
        r8  means 8 bit signed data, which are added to program counter

        Terminology
        -	Flag is not affected by this operation.
        *	Flag is affected according to result of operation.
        b	A bit number in any 8-bit register or memory location.
        C	Carry flag.
        cc	Flag condition code: C,NC,NZ,Z
        d	Any 8-bit destination register or memory location.
        dd	Any 16-bit destination register or memory location.
        e	8-bit signed 2's complement displacement.
        f	8 special call locations in page zero.
        H	Half-carry flag.
        N	Subtraction flag.
        NC	Not carry flag
        NZ	Not zero flag.
        n	Any 8-bit binary number.
        nn	Any 16-bit binary number.
        r	Any 8-bit register. (A,B,C,D,E,H, or L)
        s	Any 8-bit source register or memory location.
        sb	A bit in a specific 8-bit register or memory location.
        ss	Any 16-bit source register or memory location.
        Z	Zero Flag.
     */

    public partial class Z80A
    {
        public Registers Registers = new Registers();
        public Clock Clock = new Clock();
        public Mmu Mmu = new Mmu();

        public Z80A()
        {
            BootRom.Bytes.CopyTo(Mmu.Memory, 0);

            for (var i = 0; i < 16 * 16; i++)
            {
                Op[i] = NotImplemented;
                CB[i] = NotImplemented;
            }

            Op[0x00] = NOP;
            Op[0x02] = LDBCmA;
            Op[0xCB] = () => PerfixCB();
            Op[0x01] = () => Registers.BC = LDd16("BC");
            Op[0x11] = () => Registers.DE = LDd16("DE");
            Op[0x21] = () => Registers.HL = LDd16("HL");
            Op[0x31] = () => Registers.SP = LDd16("SP");

            Op[0x0E] = () => Registers.C = LDd8("C");
            Op[0x1A] = () =>
            {
                // LD A, (DE)
                Program.LogLine($"LD A, (DE) #{Registers.DE:X2} {Registers.DE}");
                Registers.A = Mmu.ReadByte(Registers.DE);
                Registers.PC += 1;
                Registers.M += 2;
            };

            Op[0x3E] = () => Registers.A = LDd8("A");

            //LD (C),A
            Op[0xE2] = () =>
            {
                Program.LogLine($"LD (C), A#{Registers.A}");
                Mmu.WriteByte(Registers.C, Registers.A);
                Registers.PC += 2;
                Registers.M += 2;
            };


            // JR cc,n
            Op[0x20] = () => { JRccnn(!Registers.FZ, "NZ", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x28] = () => { JRccnn(Registers.FZ, "Z", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x30] = () => { JRccnn(Registers.FC, "NC", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x38] = () => { JRccnn(!Registers.FC, "C", Mmu.ReadByte(Registers.PC + 1)); };

            // LD (HL-),A
            Op[0x32] = () =>
            {
                Program.LogLine($"LD (HL-), A #{Registers.A:X2}");
                Mmu.WriteByte(Registers.HL, Registers.A);
                Registers.HL -= 1;
                Registers.PC += 1;
                Registers.M += 2;
            };

            Op[0x4F] = () =>
            {
                Program.LogLine($"LD C, A #{Registers.A:X2} {Registers.A:}");
                NotImplemented();
                //LD C, A
                //1  4

            };

            //LD (HL),A
            Op[0x77] = () =>
            {
                Program.LogLine($"LD (HL), A #{Registers.A:X2}, d{Registers.A}");
                Mmu.WriteByte(Registers.HL, Registers.A);
                Registers.PC += 1;
                Registers.M += 2;
            };

            Op[0x95] = () =>
            {
                NotImplemented();
                //SUB L
                //1  4
                //Z 1 H C

                Registers.PC += 1;
                Registers.M += 1;
            };

            Op[0xA8] = () => XOR_A(Registers.B, "B");
            Op[0xA9] = () => XOR_A(Registers.C, "C");
            Op[0xAA] = () => XOR_A(Registers.D, "D");
            Op[0xAB] = () => XOR_A(Registers.E, "E");
            Op[0xAC] = () => XOR_A(Registers.H, "H");
            Op[0xAD] = () => XOR_A(Registers.L, "L");
            Op[0xAE] = () => XOR_A(Mmu.ReadByte(Registers.HL), "(HL)");
            Op[0xAF] = () => XOR_A(Registers.A, "A");

            Op[0xCD] = () =>
            {
                //CALL a16
                Program.LogLine($"CALL #{Mmu.ReadWord(Registers.PC + 1):X4} push({Registers.PC + 3:X4})");

                Registers.SP -= 2;
                Mmu.WriteWord(Registers.SP, Registers.PC + 3);
                Registers.PC = Mmu.ReadWord(Registers.PC + 1);
                Registers.M += 24 / 4;
            };

            //LDH (a8),A
            Op[0xE0] = () =>
            {
                Program.LogLine("LDH (a8), A");
                Registers.PC += 2;
                Registers.M += 3;
            };

            CB[0x7c] = () => BIT_br(7, Registers.H, "H");
        }

        private void PerfixCB()
        {
            CB[Mmu.ReadByte(Registers.PC + 1)]();
            Registers.PC += 2;
            Registers.M += 2;
        }

        public readonly Action[] Op = new Action[256];
        public readonly Action[] CB = new Action[256];

        void NotImplemented()
        {
            var ops = Mmu.Read(new Range(Registers.PC, Registers.PC + 3));

            var debug = string.Empty;
            for (var i = 0; i < ops.Length; i++)
            {
                debug += $"{ops[i]:X2} ";
            }

            throw new NotImplementedException($"{debug} PC={Registers.PC:X4}");

        }

        internal void Step()
        {
            var op = Mmu.ReadByte(Registers.PC);
            Program.Log($"{Registers.PC:X4}\t");
            Program.Log($"{Mmu.ReadByte(Registers.PC):X2} [{Mmu.ReadByte(Registers.PC + 1):X2} {Mmu.ReadByte(Registers.PC + 2):X2}] \t");
            Op[op]();
        }

        void NOP()
        {
            Program.LogLine("NOP");
            Registers.PC++;
            Registers.M += 1;
        }

        byte LDd8(string name)
        {
            //LD C,d8
            //2  8
            var data = Mmu.ReadByte(Registers.PC + 1);
            Program.LogLine($"LD {name}, d8(${data:X1})");

            Registers.PC += 2;
            Registers.M += 2;
            return data;
        }


        ushort LDd16(string name)
        {
            var data = Mmu.ReadWord(Registers.PC + 1);
            Program.LogLine($"LD {name}, d16(${data:X2})");

            Registers.PC += 3;
            Registers.M += 3;
            return data;
        }

        void XOR_A(byte input, string name)
        {
            Program.LogLine($"XOR {name}, ${input:X2}");

            Registers.A ^= input;
            Registers.PC += 1;
            Registers.M += 1;
            Registers.FZ = Registers.A == 0;
        }

        void LDBCmA()
        {
            Registers.B = Mmu.ReadByte((ushort)(Registers.PC + 1));
            Registers.C = Mmu.ReadByte((ushort)(Registers.PC + 2));
            Registers.M += 3;
        }

        public void JRccnn(bool value, string name, byte nn)
        {
            Program.LogLine($"JR {name}({value.ToString()}), {(sbyte)nn}");

            if (value)
            {
                if (Registers.PC + (sbyte)nn < 0)
                {
                    throw new InvalidOperationException($"Instruction JR, jump relative cannot move Program Counter below 0. PC={Registers.PC}, Offset={(sbyte)nn}");
                }

                Registers.PC = (ushort)(Registers.PC + (sbyte)nn);
                Registers.M += 1;
            }
            else
            {
                var foo = false;
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

            Program.LogLine($"BIT {bit}, {name}");
            var result = value & (1 << bit);
            Registers.FZ = result == 0;
            Registers.FN = false;
            Registers.FH = true;
        }

        public void BIT_b_addr(byte bit, byte value)
        {
            throw new NotImplementedException("figure out how to render addr correctly.");
            BIT_br(bit, value, "addr");
            Registers.M += 4;
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

    public struct Clock
    {
        uint M { get; set; }
    }
}
