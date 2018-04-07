using System;
using System.Text;
using System.IO;

namespace FScriptMono {
	class MainClass {
		public static void Main(string[] args) {
			var env = Interpreter.NewEnv();

			while (true) {
				Console.Write("fscript> ");
				Lexer.text = Console.ReadLine();
				Lexer.position = 0;
				try {
					var node = Lexer.Peek().type == Token.LCurly ? Parser.Prog() : Parser.Stmt();
					
					Console.WriteLine(Interpreter.Print(node));
					Console.WriteLine(Interpreter.Eval(node, env));
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
	}
}
