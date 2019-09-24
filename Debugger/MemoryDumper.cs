using System;
using System.Text;

namespace GameBoy.Debugger
{
    public static class MemoryDumper
    {
        internal static void PrintMemory(this Z80A z80a, Range range)
        {
            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            var leftpad = 65;
            var lines = 1;
            var datarow = 0;

            Console.SetCursorPosition(leftpad, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"│ADDR 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            Console.SetCursorPosition(leftpad, lines++);
            Console.Write($"│{range.Start.Value:X4} ");
            Console.ResetColor();

            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                if (i == z80a.Registers.PC || i == z80a.Registers.PC + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                //if (i == z80a.Index)
                //{
                //	Console.ForegroundColor = ConsoleColor.DarkGreen;
                //}

                Console.Write($"{z80a.Mmu.ReadByte(i):X2} ");
                Console.ResetColor();

                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine();
                    Console.SetCursorPosition(leftpad, lines++);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"│{range.Start.Value + (++datarow * 16):X4} ");
                    Console.ResetColor();
                }
            }

            Console.SetCursorPosition(leftpad, lines++);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("└───────────────────────────┬──────────────────────────");
            //Console.WriteLine("└".PadRight(Console.BufferWidth - leftpad, '─'));
            Console.ResetColor();
            Console.SetCursorPosition(left, top);
        }

        public static void PrintRegisters(this Z80A z80a)
        {
            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            var leftpad = 93;
            var lines = 1;
            var length = 0;
            var hOffset = 8;

            Console.SetCursorPosition(leftpad, hOffset);

            var dump = z80a.RegisersAsFormattedString();
            foreach (var line in dump.Split("\r\n"))
            {
                Console.SetCursorPosition(leftpad, hOffset + lines);
                Console.WriteLine(line);

                length = Math.Max(length, line.Length);
                lines++;
            }

            Console.SetCursorPosition(left, top);
        }

        public static string RegisersAsFormattedString(this Z80A z80a)
        {
            var buffer = new StringBuilder();

            buffer.Append($"│ SP = #{z80a.Registers.SP:X4} ");
            buffer.AppendLine($"PC = #{z80a.Registers.PC:X4} ");

            var itr = z80a.Registers.GetEnumerator();
            while (itr.MoveNext())
            {
                var value = itr.Current.Name.Length > 1 ? itr.Current.Value.ToString("X4") : itr.Current.Value.ToString("X2");
                buffer.AppendLine($"│ {itr.Current.Name}: #{value}");
            }

            return buffer.ToString();
        }
    }
}
