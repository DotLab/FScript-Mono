using System;
using System.Text;
using System.IO;

namespace FuScript {
	class MainClass {
		static readonly StringBuilder _sb = new StringBuilder();
		static string _line;

		public static void Main(string[] args) {
			var env = Interpreter.CreateEnv();

			while (true) {
				_sb.Clear();
				do {
					Console.Write("FuScript> ");
					_sb.AppendLine(_line = Console.ReadLine());
				} while (_line != "");

				try {
					var tokens = Lexer.Scan(_sb.ToString());
					Console.WriteLine(Lexer.ToString(tokens));

					Parser.Set(tokens);
					var root = Parser.Program();
					Console.WriteLine(root);

					Console.WriteLine(Interpreter.Eval(root, env));
				} catch (Exception e) {
					Console.WriteLine(e);
				}

				Console.WriteLine();
			}
		}
	}
}
