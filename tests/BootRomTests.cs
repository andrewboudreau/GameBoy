using GameBoy.Debugger;
using GameBoy.Test.Extensions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace GameBoy.Test
{

    /// <summary>
    /// Based on `docs\Gameboy_BootRom_Explained.html`.
    /// </summary>
    [TestFixture]
    public class BootRomTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Step1_InitializeStackPointTo_0xFFFE()
        {
            var cpu = new Z80A();
            cpu.Step();
            Assert.That(cpu.Registers.SP == 0xFFFE);
        }

        [Test]
        public void Step2_ClearVRAM_0x8000To0x9FFF()
        {
            // The purpose of these instructions is to clear (set to zero) all of the 
            // Video RAM (memory area from 0x8000 to 0x9FFF

            // Arrange
            var cpu = new Z80A();
            cpu.Mmu.Read(0x55FF..0xAFFF).Fill(0xFF);
            Assert.That(cpu.Mmu.ReadByte(0x7FFE), Is.EqualTo(0xFF));

            // Act
            cpu.StepUntil(r => r.HL == 0x7FFF);

            // Assert
            foreach (var i in cpu.Mmu.Read(0x8000..0x9FFF))
            {
                Assert.That(i == 0x0);
            }

            // Bytes just before
            Assert.That(cpu.Mmu.ReadByte(0x7FFE) == 0xFF);
            Assert.That(cpu.Mmu.ReadByte(0x7FFF) == 0xFF);

            // Bytes just after
            Assert.That(cpu.Mmu.ReadByte(0xA000) == 0xFF);
            Assert.That(cpu.Mmu.ReadByte(0xA001) == 0xFF);
        }

        [Test]
        public void Step2_ClearVRAM_InstructionCount()
        {
            // The purpose of these instructions is to clear (set to zero) all of the 
            // Video RAM (memory area from 0x8000 to 0x9FFF

            // Arrange
            var cpu = new Z80A();

            // Act
            cpu.StepUntil(r => r.PC == 0x7);
            var steps = cpu.Clock.Steps;
            cpu.StepUntil(r =>
            {
                System.Console.WriteLine($"{cpu.Clock.Steps}");
                return r.HL == 0x7FFF;
            });

            steps = cpu.Clock.Steps - steps;

            var expected = (0x9FFF - 0x8000) * 3 + 1;
            // The three operations
            // 0x0007 – LD (HL-), A
            // 0x0008 – BIT 7, H
            // 0x000A – JRNZ.+ 0xfb

            Assert.AreEqual(expected, steps);
        }

        [Test]
        public void Step3_InitializeAudio()
        {
            /*
                The following fragment makes some writes to the audio device; suffice for now that 
                it turns on the device and writes to some of the devices’ registers:

                0x000C – LD HL, $0xFF26 # load 0xFF26 to HL
                0x000F – LD C, $0x11 # load 0x11 to C
                0x0011 – LD A, $0x80 # load 0x80 to A
                0x0013 – LD (HL-), A # load A to address pointed to by HL and Dec HL
                0x0014 – LD ($0xFF00+C), A # load A to address 0xFF00+C (0xFF11)
                0x0015 – INC C # increment C register
                0x0016 – LD A, $0xF3 # load 0xF3 to A
                0x0018 – LD ($0xFF00+C), A # load A to address 0xFF00+C (0xFF12)
                0x0019 – LD (HL-), A # load A to address pointed to by HL and Dec HL
                0x001A – LD A, $0x77 # load 0x77 to A
                0x001C – LD (HL), A # load A to address pointed to by HL
            */

            var cpu = new Z80A();
            cpu.StepUntil(r => r.PC == 0x14)
                .StartOutput()
                .StepUntil(r => r.PC == 0x1D);

            Assert.That(cpu.Mmu.ReadByte(0xFF26) == 0x80);
            Assert.That(cpu.Mmu.ReadByte(0xFF11) == 0x80);

            Assert.That(cpu.Mmu.ReadByte(0xFF12) == 0xF3);
            Assert.That(cpu.Mmu.ReadByte(0xFF25) == 0xF3);

            Assert.That(cpu.Mmu.ReadByte(0xFF24) == 0x77);
        }

        [Test]
        public void Step4_NintendoLogo()
        {
            var cpu = new Z80A();
            cpu.StepUntil(r => r.PC >= 0x1D)
                .StartOutput()
                .StepUntil(r => r.PC >= 0x28);

            // initialize the palette
            Assert.That(cpu.Mmu.ReadByte(0xFF47), Is.EqualTo(0xFC));

            // Pointer to Nintendo Logo
            Assert.That(cpu.Registers.DE, Is.EqualTo(0x0104));

            // Pointer to Video RAM
            Assert.That(cpu.Registers.HL, Is.EqualTo(0x8010));

            // Other register values
            Assert.That(cpu.Registers.AF, Is.EqualTo(0xCE00));
            Assert.That(cpu.Registers.BC, Is.EqualTo(0x0012));
            Assert.That(cpu.Registers.SP, Is.EqualTo(0xFFFE));
        }

        [Test]
        public void Step5_DecompressIntoVRam_FirstLoop0x95()
        {
            var cpu = new Z80A();
            cpu.StepUntil(r => r.PC >= 0x1D)
                .StartOutput()
                .StepUntil(r => r.PC == 0xA1);

            Assert.That(cpu.Mmu.ReadByte(0xFF47), Is.EqualTo(0xFC));

            Assert.That(cpu.Registers.BC, Is.EqualTo(0x039D));
            Assert.That(cpu.Registers.DE, Is.EqualTo(0x0106)); // Nintnedo Logo Data Pointer
            Assert.That(cpu.Registers.HL, Is.EqualTo(0x8010));

            // Flags
            Assert.That(cpu.Registers.FZ, Is.False);
            Assert.That(cpu.Registers.FN, Is.True);
            Assert.That(cpu.Registers.FH, Is.False);
            Assert.That(cpu.Registers.FC, Is.True);

            Assert.That(cpu.Registers.AF, Is.EqualTo(0x3B50));
            Assert.That(cpu.Registers.SP, Is.EqualTo(0xFFFC));
        }
    }
}
