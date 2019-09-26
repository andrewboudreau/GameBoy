using System;
using System.Diagnostics;

namespace GameBoy.Test.Extensions
{
    public static class Z80ATestExtensions
    {
        public static Action<string> ConsoleOutput = Console.Write;
        public static Action<string> ConsoleOutputLine = Console.WriteLine;

        public static Action<string> DebugOutput = msg => Debug.Write(msg);
        public static Action<string> DebugOutputLine = msg => Debug.WriteLine(msg);

        public static Action<string> Empty = _ => { };

        public static Z80A Output(this Z80A cpu, bool enable = false)
        {
            if (enable)
            {
                cpu.DebugOutput = DebugOutput;
                cpu.DebugOutputLine = DebugOutputLine;
            }
            else
            {
                cpu.DebugOutput = Empty;
                cpu.DebugOutputLine = Empty;
            }

            return cpu;
        }

        public static Z80A StartOutput(this Z80A cpu)
        {
            cpu.DebugOutput = DebugOutput;
            cpu.DebugOutputLine = DebugOutputLine;

            return cpu;
        }
    }
}
