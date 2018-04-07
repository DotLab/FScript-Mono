using System;
using System.Text;
using System.IO;

namespace FuScript {
	class MainClass {
		public static void Main(string[] args) {
			while (true) {
				Console.Write("FuScript> ");
				string text = Console.ReadLine();
				var tokens = Lexer.Scan(text);
			}
		}
	}
}
