using System;
using System.Text;
using System.IO;

namespace FuScript {
	class MainClass {
		public static void Main(string[] args) {
			while (true) {
				Console.Write("FuScript> ");
				string text = Console.ReadLine();
				try {
					var tokens = Lexer.Scan(text);
					Console.WriteLine("> " + Lexer.Print(tokens));
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
	}
}
