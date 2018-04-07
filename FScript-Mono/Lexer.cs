using System;
using System.Text;

namespace FScriptMono {
	public struct Token {
		public const int Num = 0;
		public const int Plus = 1, Minus = 2, Mul = 3, Div = 4;

		public const int Mis = -1, Eof = -2;
		public const int LParen = 10, RParen = 11, LCurly = 12, RCurly = 13;
		public const int Equal = 20, Semi = 21;
		public const int Id = 30;

		public const int Var = 100;

		public int type;
		public float num;
		public string id;
	}

	public static class Lexer {
		public static string text;
		public static int position;

		static readonly StringBuilder sb = new StringBuilder();

		static Token Id() {
			sb.Clear();
			while (position < text.Length && char.IsLetterOrDigit(text[position])) sb.Append(text[position++]);
			string id = sb.ToString();
			switch (id) {
				case "var":
					return new Token{ type = Token.Var };
				default:
					return new Token{ type = Token.Id, id = id };
			}
		}

		static Token Number() {
			sb.Clear();
			while (position < text.Length && char.IsDigit(text[position])) sb.Append(text[position++]);
			if (position < text.Length && text[position] == '.') sb.Append(text[position++]);
			while (position < text.Length && char.IsDigit(text[position])) sb.Append(text[position++]);
			return new Token{ type = Token.Num, num = float.Parse(sb.ToString()) };
		}

		static Token Eat() {
			// Eat whitespaces
			while (position < text.Length && char.IsWhiteSpace(text[position])) position += 1;

			if (position >= text.Length) return new Token { type = Token.Eof };

			char c = text[position];

			if (char.IsLetter(c)) {
				return Id();
			}

			if (char.IsDigit(c)) {
				return Number();
			}

			switch (c) {
				case '+':
					position += 1;
					return new Token { type = Token.Plus };
				case '-':
					position += 1;
					return new Token { type = Token.Minus };
				case '*':
					position += 1;
					return new Token { type = Token.Mul };
				case '/':
					position += 1;
					return new Token { type = Token.Div };
				case '(':
					position += 1;
					return new Token { type = Token.LParen };
				case ')':
					position += 1;
					return new Token { type = Token.RParen };
				case '{':
					position += 1;
					return new Token { type = Token.LCurly };
				case '}':
					position += 1;
					return new Token { type = Token.RCurly };
				case '=':
					position += 1;
					return new Token { type = Token.Equal };
				case ';':
					position += 1;
					return new Token { type = Token.Semi };
			}

			throw new Exception("Cannot parse");
		}

		public static Token Expect(int type) {
			var token = Eat();
			if (token.type != type) throw new Exception("Token type mismatch");
			return token;
		}

		public static Token Expect(int type1, int type2) {
			var token = Eat();
			if (token.type != type1 && token.type != type2) throw new Exception("Token type mismatch");
			return token;
		}

		public static Token Expect(int type1, int type2, int type3, int type4) {
			var token = Eat();
			if (token.type != type1 && token.type != type2 && token.type != type3 && token.type != type4) throw new Exception("Token type mismatch");
			return token;
		}

		public static Token Expect(int type1, int type2, int type3, int type4, int type5) {
			var token = Eat();
			if (token.type != type1 && token.type != type2 && token.type != type3 && token.type != type4 && token.type != type5) throw new Exception("Token type mismatch");
			return token;
		}

		public static Token Prefer(int type) {
			int pos = position;
			var token = Eat();
			if (token.type != type) {
				position = pos;
				return new Token{ type = Token.Mis };
			}
			return token;
		}

		public static Token Prefer(int type1, int type2) {
			int pos = position;
			var token = Eat();
			if (token.type != type1 && token.type != type2) {
				position = pos;
				return new Token{ type = Token.Mis };
			}
			return token;
		}

		public static Token Peek() {
			int pos = position;
			var token = Eat();
			position = pos;
			return token;
		}
	}
}

