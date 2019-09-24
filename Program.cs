using System;

using GameBoy;
using GameBoy.Debugger;

namespace AndrewsWorld
{
    class Program
    {
        static long ticks = 0;
        static bool print = false;

        public static Action<string> Log = msg =>
        {
            if (print)
            {
                Console.Write(msg);
            }
        };

        public static Action<string> LogLine = msg =>
        {
            if (print)
            {
                Console.WriteLine(msg);
            }
        };

        static void Main(string[] args)
        {
            var gameboy = new Z80A();
            gameboy.PrintMemory(0..100);
            gameboy.PrintRegisters();

            while (true)
            {
                if (gameboy.Registers.HL == 0x8000)
                {
                    print = true;
                }

                if (print)
                {
                    gameboy.PrintMemory(0..100);
                    gameboy.PrintRegisters();
                }

                gameboy.Step();

                if (Console.CursorTop > 36)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Clear();
                }

                //Console.ReadKey(true);
            }
        }
    }
}
