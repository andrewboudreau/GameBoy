using System;

namespace GameBoy.Debugger
{
    public static class Z80AExtensions
    {
        public static Z80A Step(this Z80A cpu, int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                cpu.Step();
            }

            return cpu;
        }

        public static Z80A StepUntil(this Z80A cpu, Func<Registers, Mmu, bool> stop, ulong stopAfter = ulong.MaxValue)
        {
            ulong loop = 0;
            while (!stop(cpu.Registers, cpu.Mmu) && (++loop < stopAfter))
            {
                cpu.Step();
            }

            return cpu;
        }

        public static Z80A StepUntil(this Z80A cpu, Func<Registers, bool> stop, ulong stopAfter = ulong.MaxValue)
        {
            ulong loop = 0;
            while (!stop(cpu.Registers) && (++loop < stopAfter))
            {
                cpu.Step();
            }

            return cpu;
        }
    }
}
