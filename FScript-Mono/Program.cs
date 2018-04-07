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
				try {
					var node = Lexer.Peek().type == Token.LCurly ? Parser.Prog() : Parser.Expr();
					
					Console.WriteLine(Interpreter.Print(node));
					//				Console.WriteLine(Interpreter.Eval(node));
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
	}
}
