namespace FuScript {
	public static class Compiler {
		static int pos, length;

		public static readonly byte[] insts = new byte[65536];
		public static int icount;

		public static readonly string[] strings = new string[256];
		public static readonly double[] numbers = new double[256];
		public static byte scount, ncount;

		static bool Peek(byte t) {
			return Lexer.tokens[pos] == t;
		}

		static bool Match(byte t) {
			if (Lexer.tokens[pos] == t) { pos += 1; return true; }
			return false;
		}

		static int Eat(byte t) {
			if (Lexer.tokens[pos] == t) return pos++;
			throw new System.Exception("Compiler: Unexpected token " + Lexer.tokens[pos] + ", expecting " + t);
		}

		static double GetNumber() {
			return Lexer.numbers[Lexer.tokens[pos++]];
		}

		static void Emit(byte opcode) {
			insts[icount++] = opcode;

			Recant();
		}

		static void Emit(byte opcode, byte oprand) {
			insts[icount++] = opcode;
			insts[icount++] = oprand;
		}

		static byte AddConst(double value) {
			numbers[ncount] = value;
			return ncount++;
		}

		public static void Compile() {
			pos = 0; length = Lexer.tcount;
			icount = 0; scount = 0; ncount = 0;

			Addition();
		}

		/**
		 * addition -> multiplication (("-" | "+") multiplication)*
		 */
		static void Addition() {
			Multiplication();
			while (true) {
				if (Match(Token.Minus)) {
					Multiplication();
					Emit(Inst.BinarySubtract);
				} else if (Match(Token.Plus)) {
					Multiplication();
					Emit(Inst.BinaryAdd);
				} else break;
			}
		}

		/**
		 * multiplication -> unary (("/" | "*") unary)*
		 */
		static void Multiplication() {
			Unary();
			while (true) {
				if (Match(Token.Slash)) {
					Unary();
					Emit(Inst.BinaryDivide);
				} else if (Match(Token.Star)) {
					Unary();
					Emit(Inst.BinaryMultiply);
				} else break;
			}
		}

		/** 
		 * unary -> ("!" | "-") unary
		 *        | call
		 */
		static void Unary() {
			if (Match(Token.Bang)) {
				Primary();
				Emit(Inst.UnaryNot);
			} else if (Match(Token.Minus)) {
				Primary();
				Emit(Inst.UnaryNegative);
			} else Primary();
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary() {
			if (Match(Token.Number)) {
				Emit(Inst.PushConst, AddConst(GetNumber()));
			} else {
				Eat(Token.LParen);
				Addition();
				Eat(Token.RParen);
			}
		}

		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string Recant() {
			sb.Clear();

			int i = 0;
			while (i < icount) {
				switch (insts[i]) {
				case Inst.BinarySubtract: sb.AppendFormat("{0,3}: BINARY_SUBTRACT \n", insts[i++]); break;
				case Inst.BinaryAdd:      sb.AppendFormat("{0,3}: BINARY_ADD      \n", insts[i++]); break;
				case Inst.BinaryDivide:   sb.AppendFormat("{0,3}: BINARY_DIVIDE   \n", insts[i++]); break;
				case Inst.BinaryMultiply: sb.AppendFormat("{0,3}: BINARY_MULTIPLY \n", insts[i++]); break;

				case Inst.UnaryNot:       sb.AppendFormat("{0,3}: UNARY_NOT       \n", insts[i++]); break;
				case Inst.UnaryNegative:  sb.AppendFormat("{0,3}: UNARY_NEGATIVE  \n", insts[i++]); break;
					
				case Inst.PushConst:      sb.AppendFormat("{0,3}: PUSH_CONST      {1,3} ({2})\n", insts[i++], insts[i], numbers[insts[i++]]); break;

				default:
					throw new System.Exception("Unrecognized instruction " + insts[i]);
				}
			}

			return sb.ToString();
		}
	}
}

