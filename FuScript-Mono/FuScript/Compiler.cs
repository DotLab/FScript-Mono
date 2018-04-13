namespace FuScript {
	public class UnexpectedTokenException : System.Exception {
		public UnexpectedTokenException(byte t, params byte[] es) : base("Unexpected token " + Token.Recant(t) + ", expecting one of " + Token.Recant(es)) { }
	}

	public static class Compiler {
		static ushort pos, length;

		static void Log(string str) {
			System.Console.Write(str);
		}

		static byte Current() {
			return pos < length ? Lexer.tokens[pos] : Token.Eof;
		}

		static bool Peek(byte t) {
			return Current() == t;
		}

		static bool Match(byte t) {
			if (Current() == t) { Log(Token.Recant(Current()) + "\n"); pos += 1; return true; }
			return false;
		}

		static void Eat(byte t) {
			if (Current() == t) { Log(Token.Recant(Current()) + "\n"); pos++; return; }
			throw new UnexpectedTokenException(Current(), t);
		}

		static ushort EatLiteral() {
			return (ushort)(Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8);
		}

		public static void Reset() {
			pos = 0;
		}

		public static void Compile() {
			length = Lexer.tokenCount;
			throw new UnexpectedTokenException(Token.AndAnd, Token.And, Token.OrOr);
		}
	}
}
