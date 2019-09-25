using GameBoy.Debugger;
using NUnit.Framework;

namespace GameBoy.Test
{

    /// <summary>
    /// Based on `docs\Gameboy_BootRom_Explained.html`.
    /// </summary>
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
            var cpu = new Z80A();
            cpu.StepUntil(r => r.PC == 0x14);

            cpu.Step();
        }
    }
}