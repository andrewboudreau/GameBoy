using System;

using GameBoy;
using GameBoy.Debugger;

namespace AndrewsWorld
{
	class Program
	{
		static void Main(string[] args)
		{
			var gameboy = new Z80A();
			while (true)
			{
				gameboy.PrintMemory(0..100);
				gameboy.PrintRegisters();

				gameboy.Step();
				//Console.ReadKey(true);
			}
		}
	}
}
