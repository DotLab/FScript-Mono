using System;
using System.Text;
using System.IO;

namespace FScriptMono {
	class MainClass {
		public static void Main(string[] args) {
			while (true) {
				Console.Write("fscript> ");
				Lexer.text = Console.ReadLine();
				Lexer.position = 0;
				var node = Parser.Expr();
		
				Console.WriteLine(Interpreter.Print(node));
				Console.WriteLine(Interpreter.Eval(node));
			}
		}
	}
}
