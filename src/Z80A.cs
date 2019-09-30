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
        public Action<string> DebugOutput = _ => { };
        public Action<string> DebugOutputLine = _ => { };

        public Registers Registers = new Registers();
        public Clock Clock = new Clock();
        public Mmu Mmu = new Mmu();

        public Z80A()
        {
            BootRom.Bytes.CopyTo(Mmu.Memory, 0);
            BootRom.Logo.CopyTo(Mmu.Memory, 0x104);

            for (var i = 0; i < 16 * 16; i++)
            {
                Op[i] = NotImplemented;
                CB[i] = NotImplemented;
            }

            Op[0xCB] = () => PerfixCB();

            Op[0x00] = NOP;
            Op[0x02] = LDBCmA;

            Op[0x05] = () =>
            {
                // DEC B
                // 1 4
                // Z 1 H -

                Registers.PC += 1;
                Registers.M += 2;
            };

            Op[0x3C] = () => Registers.A = Increment(Registers.A, "A");
            Op[0x04] = () => Registers.B = Increment(Registers.B, "B");
            Op[0x0C] = () => Registers.C = Increment(Registers.C, "C");
            Op[0x14] = () => Registers.D = Increment(Registers.D, "D");
            Op[0x1C] = () => Registers.E = Increment(Registers.E, "E");
            Op[0x24] = () => Registers.H = Increment(Registers.H, "H");
            Op[0x2C] = () => Registers.L = Increment(Registers.L, "L");
            Op[0x34] = () =>
            {
                Mmu.WriteByte(Registers.HL, Increment(Mmu.ReadByte(Registers.HL), "(HL)"));
                Registers.M += 8 / 4;
            };

            Op[0x3D] = () => Registers.A = Decrement(Registers.A, "A");
            Op[0x05] = () => Registers.B = Decrement(Registers.B, "B");
            Op[0x0D] = () => Registers.C = Decrement(Registers.C, "C");
            Op[0x15] = () => Registers.D = Decrement(Registers.D, "D");
            Op[0x1D] = () => Registers.E = Decrement(Registers.E, "E");
            Op[0x25] = () => Registers.H = Decrement(Registers.H, "H");
            Op[0x2D] = () => Registers.L = Decrement(Registers.L, "L");
            Op[0x35] = () =>
            {
                Mmu.WriteByte(Registers.HL, Decrement(Mmu.ReadByte(Registers.HL), "(HL)"));
                Registers.M += 8 / 4;
            };

            Op[0x01] = () => Registers.BC = LDd16("BC");
            Op[0x11] = () => Registers.DE = LDd16("DE");
            Op[0x21] = () => Registers.HL = LDd16("HL");
            Op[0x31] = () => Registers.SP = LDd16("SP");

            Op[0x1A] = () =>
            {
                // LD A, (DE)
                DebugOutputLine($"LD A, (DE) #{Registers.DE:X2} {Registers.DE}");
                Registers.A = Mmu.ReadByte(Registers.DE);
                Registers.PC += 1;
                Registers.M += 2;
            };

            Op[0x06] = () => Registers.B = LDd8("B");
            Op[0x0E] = () => Registers.C = LDd8("C");
            Op[0x16] = () => Registers.D = LDd8("D");
            Op[0x1E] = () => Registers.E = LDd8("E");
            Op[0x36] = () => NotImplemented();
            Op[0x3E] = () => Registers.A = LDd8("A");


            //LD($FF00 + C),A
            Op[0xE2] = () =>
            {
                DebugOutputLine($"LD ($FF00 + C)[{0xFF00 + Registers.C:X4}], A #{Registers.A:X2} {Registers.A}");
                Mmu.WriteByte((ushort)(0xFF00 + Registers.C), Registers.A);
                Registers.PC += 1;
                Registers.M += 2;
            };


            // JR cc,n
            Op[0x20] = () => { JRccnn(!Registers.FZ, "NZ", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x28] = () => { JRccnn(Registers.FZ, "Z", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x30] = () => { JRccnn(!Registers.FC, "NC", Mmu.ReadByte(Registers.PC + 1)); };
            Op[0x38] = () => { JRccnn(Registers.FC, "C", Mmu.ReadByte(Registers.PC + 1)); };

            // LD (HL-),A
            Op[0x32] = () =>
            {
                DebugOutputLine($"LD (HL-), A #{Registers.A:X2}");
                Mmu.WriteByte(Registers.HL, Registers.A);
                Registers.HL -= 1;
                Registers.PC += 1;
                Registers.M += 2;
            };

            // LD n,A
            Op[0x7F] = () => Registers.A = LDnA("A");
            Op[0x47] = () => Registers.B = LDnA("B");
            Op[0x4F] = () => Registers.C = LDnA("C");
            Op[0x57] = () => Registers.D = LDnA("D");
            Op[0x5F] = () => Registers.E = LDnA("E");
            Op[0x67] = () => Registers.H = LDnA("H");
            Op[0x6F] = () => Registers.L = LDnA("L");

            //LD (HL),A
            Op[0x77] = () =>
            {
                DebugOutputLine($"LD (HL), A #{Registers.A:X2}, d{Registers.A}");
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

            // POP BC, 1 12
            Op[0xC1] = () =>
            {
                DebugOutputLine($"POP BC #{Mmu.ReadWord(Registers.SP):X4} SP={Registers.SP:X4}");

                Registers.BC = Mmu.ReadWord(Registers.SP);
                Registers.SP += 2;

                Registers.PC += 1;
                Registers.M += 12 / 4;
            };

            // PUSH BC , 1 16
            Op[0xC5] = () =>
            {
                DebugOutputLine($"PUSH BC #{Registers.BC:X4} SP={Registers.SP - 2:X4}");

                Registers.SP -= 2;
                Mmu.WriteWord(Registers.SP, Registers.BC);

                Registers.PC += 1;
                Registers.M += 16 / 4;
            };

            //CALL a16
            Op[0xCD] = () =>
            {
                DebugOutputLine($"CALL #{Mmu.ReadWord(Registers.PC + 1):X4} push({Registers.PC + 3:X4})");

                Registers.SP -= 2;
                Mmu.WriteWord(Registers.SP, Registers.PC + 3);
                Registers.PC = Mmu.ReadWord(Registers.PC + 1);
                Registers.M += 24 / 4;
            };

            //LD($FF00 + C),A
            Op[0xE0] = () =>
            {
                var n = Mmu.ReadByte(Registers.PC + 1);
                DebugOutputLine($"LD ($FF00 + n)[{0xFF00 + n:X4}], A #{Registers.A:X2} {Registers.A}");
                Mmu.WriteByte(0xFF00 + n, Registers.A);

                Registers.PC += 2;
                Registers.M += 12 / 4;
            };
            CB[0x7c] = () => BIT_br(7, Registers.H, "H");

            // Rotate Left
            CB[0x11] = () => Registers.C = RL(Registers.C, "C");
            CB[0x17] = () => Registers.A = RL(Registers.A, "A");

            //RL A
            Op[0x17] = () =>
            {
                //RLA
                DebugOutputLine($"RL A #{Registers.A:X2} [{(Registers.FC ? "1" : "0")}] {Convert.ToString(Registers.A, 2)} ");

                var carry = Registers.FC ? 1 : 0;
                Registers.FC = (Registers.A & 0x80) > 0;
                Registers.FH = false;
                Registers.FN = false;
                Registers.FZ = false;

                Registers.M += 4 / 4;
                Registers.PC += 1;

                Registers.A = (byte)((Registers.A << 1) + carry);
            };
        }

        public readonly Action[] Op = new Action[256];
        public readonly Action[] CB = new Action[256];

        public void Step()
        {
            var op = Mmu.ReadByte(Registers.PC);
            DebugOutput($"{Registers.PC:X4}\t");
            DebugOutput($"{Mmu.ReadByte(Registers.PC):X2} [{Mmu.ReadByte(Registers.PC + 1):X2} {Mmu.ReadByte(Registers.PC + 2):X2}] \t");

            Op[op]();
            Clock.Step(Registers.M);
            Registers.M = 0;
        }

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

        void NOP()
        {
            DebugOutputLine("NOP");
            Registers.PC++;
            Registers.M += 1;
        }
    }

    public struct Clock
    {
        public ulong Steps { get; set; }

        public uint M { get; set; }

        public void Step(uint m)
        {
            Steps++;
            M += m;
        }
    }
}
