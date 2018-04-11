namespace FuScript {
	public class CompilerException : System.Exception {
		public CompilerException(string m) : base(m) { }
	}

	public class UnexpectedTokenException : CompilerException {
		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		static string Visualize(byte[] es) {
			if (es.Length == 1) return "'" + Token.Recant(es[0]) + "'";

			sb.Clear();
			sb.Append("('");
			int length = es.Length;
			for (int i = 0; i < length; ++i) {
				sb.Append(Token.Recant(es[i]));
				sb.Append("'");
				if (i + 1 < length) sb.Append(" | '");
			}
			sb.Append(")");
			return sb.ToString();
		}

		public UnexpectedTokenException(byte t, params byte[] es) : base("Unexpected token '" + Token.Recant(t) + "', expecting " + Visualize(es)) { }
	}

	public class UnexpectedTypeException : CompilerException {
		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		static string Visualize(byte[] es) {
			if (es.Length == 1) return Compiler.RecantType(es[0]);

			sb.Clear();
			sb.Append("(");
			int length = es.Length;
			for (int i = 0; i < length; ++i) {
				sb.Append(Compiler.RecantType(es[i]));
				if (i + 1 < length) sb.Append(" | ");
			}
			sb.Append(")");
			return sb.ToString();
		}

		public UnexpectedTypeException(byte t, params byte[] es) : base("Unexpected type " + Compiler.RecantType(t) + ", expecting " + Visualize(es)) { }
	}

	public class UnrecognizedOpcodeException : CompilerException {
		public UnrecognizedOpcodeException(byte t) : base("Unrecognized opcode #" + t) { }
	}

	public static class Compiler {
		const byte Null = 0, Boolean = 1, Int = 2, Float = 3, String = 4;

		public static string RecantType(byte t) {
			switch (t) {
				case Null: return "Null";
				case Boolean: return "Boolean";
				case Int: return "Int";
				case Float: return "Float";
				case String: return "String";
				default: throw new CompilerException("Unrecognized type #" + t);
			}
		}

		static ushort pos, length;

		public static readonly byte[] insts = new byte[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort instCount, declCount;

		static readonly byte[] typeStack = new byte[256];
		static int typeStackPointer = -1;

		static bool isScanning;

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
			if (Current() == t) { Log(Token.Recant(Current()) + ", "); pos += 1; return true; }
			return false;
		}

		static void Eat(byte t) {
			if (Current() == t) { Log(Token.Recant(Current()) + ", "); pos++; return; }
			throw new UnexpectedTokenException(Current(), t);
		}

		static ushort EatLiteral() {
			return (ushort)(Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8);
		}

		static void Emit(byte opcode) {
			if (isScanning) return;

			marks[instCount] = declCount;
			insts[instCount++] = opcode;
		}

		static void Emit(byte opcode, ushort operand) {
			if (isScanning) return;

			marks[instCount] = declCount;
			insts[instCount++] = opcode;
			insts[instCount++] = (byte)(operand & 0xff);
			insts[instCount++] = (byte)(operand >> 8);
		}

		static void Fill(ushort ic, ushort operand) {
			if (isScanning) return;

			ic += 1;  // Opcode
			insts[ic++] = (byte)(operand & 0xff);
			insts[ic++] = (byte)(operand >> 8);
		}

		static byte CurrentType() {
			return typeStackPointer < 0 ? Null : typeStack[typeStackPointer];
		}

		static void PushType(byte t) {
			typeStack[++typeStackPointer] = t;
		}

		static bool MatchType(byte t) {
			if (typeStack[typeStackPointer] == t) { typeStackPointer -= 1; return true; }
			return false;
		}

		static int PopType(byte t) {
			if (typeStack[typeStackPointer] == t) return typeStackPointer -= 1;
			throw new UnexpectedTypeException(typeStack[typeStackPointer], t);
		}

		public static void Reset() {
			pos = 0; instCount = 0; declCount = 0; typeStackPointer = -1;
			isScanning = false;
		}

		public static void Compile() {
			length = Lexer.tokenCount;

			Expression();
		}

		static void Expression() {
			Multiplication();
		}

		/**
		 * multiplication -> unary (("/" | "*") unary)*
		 */
		static void Multiplication() {
			Unary();

			while (true) {
				if (Match(Token.Slash)) {
					Unary();
					if (MatchType(Int)) {
						PopType(Int); Emit(Opcode.BIN_DIV_INT); PushType(Int);
					} else if (MatchType(Float)) {
						PopType(Float); Emit(Opcode.BIN_DIV_FLOAT); PushType(Float);
					} else throw new UnexpectedTypeException(CurrentType(), Int, Float);
				} else if (Match(Token.Star)) {
					Unary();
					if (MatchType(Int)) {
						PopType(Int); Emit(Opcode.BIN_MUL_INT); PushType(Int);
					} else if (MatchType(Float)) {
						PopType(Float); Emit(Opcode.BIN_MUL_FLOAT); PushType(Float);
					} else throw new UnexpectedTypeException(CurrentType(), Int, Float);
				} else break;
			}
		}

		/**
		 * unary -> ("!" | "-") unary
		 *        | call
		 */
		static void Unary() {
			if (Match(Token.Bang)) {
				Primary(true);
				Emit(Opcode.UNARY_NOT); PopType(Boolean); PushType(Boolean); 
			} else if (Match(Token.Minus)) {
				Primary(true);
				if (MatchType(Int)) {
					Emit(Opcode.UNARY_NEG_INT); PushType(Int);
				} else if (MatchType(Float)) {
					Emit(Opcode.UNARY_NEG_FLOAT); PushType(Float);
				} else throw new UnexpectedTypeException(CurrentType(), Int, Float);
			} else Primary(false);
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary(bool forced) {
			if (Match(Token.Int)) {
				Emit(Opcode.PUSH_CONST_INT, EatLiteral()); PushType(Int);
			} else if (Match(Token.Float)) {
				Emit(Opcode.PUSH_CONST_FLOAT, EatLiteral()); PushType(Float);
			} else if (Match(Token.String)) {
				Emit(Opcode.PUSH_CONST_STRING, EatLiteral()); PushType(String);
			} else if (Match(Token.KFalse)) {
				Emit(Opcode.PUSH_FALSE, EatLiteral()); PushType(Boolean);
			} else if (Match(Token.KTrue)) {
				Emit(Opcode.PUSH_TRUE, EatLiteral()); PushType(Boolean);
			} else if (Match(Token.KNull)) {
				Emit(Opcode.PUSH_NULL, EatLiteral()); PushType(Null);
			} else if (Match(Token.LParen)) {
				Expression();
				Eat(Token.RParen);
			} else if (forced) throw new UnexpectedTokenException(Current(), Token.Int, Token.Float, Token.String, Token.KFalse, Token.KTrue, Token.KNull, Token.LParen);
		}

		public static string Recant() {
			return "";
		}
	}
}
