using System;
using System.Text;
using System.IO;

namespace FScriptMono {
	class MainClass {
		struct Token {
			public const int Mis = 0, Num = 1, Add = 2, Sub = 3, Eof = 4, Mul = 5, Div = 6;

			public int type;
			public float numberValue;

			public override string ToString() {
				return string.Format("[Token: type={0}, value={1}]", type, numberValue);
			}
		}

		static string text;
		static int position;

		static void EatWhitespaces() {
			while (position < text.Length && char.IsWhiteSpace(text[position])) position += 1;
		}

		static Token Eat() {
			EatWhitespaces();

			if (position >= text.Length) return new Token { type = Token.Eof };

			char c = text[position];
			if (char.IsDigit(c)) {
				return new Token{ type = Token.Num, numberValue = Number() };
			}

			if (c == '+') {
				position += 1;
				return new Token{ type = Token.Add };
			}

			if (c == '-') {
				position += 1;
				return new Token{ type = Token.Sub };
			}

			if (c == '*') {
				position += 1;
				return new Token{ type = Token.Mul };
			}

			if (c == '/') {
				position += 1;
				return new Token{ type = Token.Div };
			}

			throw new Exception("Cannot parse");
		}

		static Token Expect(int type) {
			var token = Eat();
			if (token.type != type) throw new Exception("Token type mismatch");
			return token;
		}

		static Token Expect(int type1, int type2) {
			var token = Eat();
			if (token.type != type1 && token.type != type2) throw new Exception("Token type mismatch");
			return token;
		}

		static Token Prefer(int type1, int type2) {
			int pos = position;
			var token = Eat();
			if (token.type != type1 && token.type != type2) {
				position = pos;
				return new Token{ type = Token.Mis };
			}
			return token;
		}

		static readonly StringBuilder sb = new StringBuilder();
		static float Number() {
			sb.Clear();
			while (position < text.Length && char.IsDigit(text[position])) sb.Append(text[position++]);

			return float.Parse(sb.ToString());
		}

		static float Factor() {
			
		}

		static float Term() {
			var token = Expect(Token.Num);
			return token.numberValue;
		}

		static float Expr() {
			float value = Term();
			Token token;
			while ((token = Prefer(Token.Add, Token.Sub)).type != Token.Mis) {
				if (token.type == Token.Add) value += Term();
				else if (token.type == Token.Sub) value -= Term();
			}
			return value;
		}

		public static void Main(string[] args) {
			while (true) {
				Console.Write("fscript> ");
				text = Console.ReadLine();
				position = 0;
				var val = Expr();
				Console.WriteLine(val);
			}
		}
	}
}
