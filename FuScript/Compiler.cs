namespace FuScript {
	public static class Compiler {
		static int pos, length;

		public static readonly byte[] insts = new byte[1024];
		public static readonly int[] marks = new int[1024];
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

		static string GetString() {
			return Lexer.strings[Lexer.tokens[pos++]];
		}

		static void Emit(byte opcode) {
			marks[icount] = pos;
			insts[icount++] = opcode;

//			System.Console.WriteLine(Recant());
		}

		static void Emit(byte opcode, byte oprand) {
			marks[icount] = pos;
			insts[icount++] = opcode;
			insts[icount++] = oprand;

//			System.Console.WriteLine(Recant());
		}

		static byte AddConst(double value) {
			numbers[ncount] = value;
			return ncount++;
		}

		static byte AddString(string value) {
			strings[scount] = value;
			return scount++;
		}

		public static void Compile() {
			pos = 0; length = Lexer.tcount;
			icount = 0; scount = 0; ncount = 0;

			Declaration();
		}

		/**
		 * declaration -> funcDecl
		 *              | varDecl
		 *              | statement
		 */
		static void Declaration() {
//			System.Console.WriteLine(Lexer.tokens[pos]);
			if (Peek(Token.Var)) VarDecl();
			else Statement();
		}

		/**
		 * varDecl -> "var" IDENTIFIER ("=" expression)? ";"
		 */
		static void VarDecl() {
			Eat(Token.Var);
			Eat(Token.Id);
			byte strId = AddString(GetString());
			if (Match(Token.Equal)) {
				Expression();
			} else Emit(Inst.PushConstNull);
			Emit(Inst.PopNewVar, strId);
			Eat(Token.Semi);
		}

		/**
		 * statement -> block
		 *            | ifStmt
		 *            | whileStmt
		 *            | forStmt
		 *            | returnStmt
		 *            | printStmt
		 *            | assignStmt
		 */
		public static void Statement() {
			if (Peek(Token.LCurly)) Block();
			else if (Peek(Token.Print)) PrintStmt();
			else AssignStmt();
		}

		/**
		 * block -> "{" declaration* "}"
		 */
		static void Block() {
			Eat(Token.LCurly);
			Emit(Inst.CloneEnv);
			while (!Match(Token.RCurly)) Declaration();
			Emit(Inst.RestoreEnv);
		}

		/**
		 * printStmt -> "print" expression ";"
		 */
		static void PrintStmt() {
			Match(Token.Print);
			Expression();
			Match(Token.Semi);
			Emit(Inst.Print);
		}

		/**
		 * assignStmt -> identifier "=" expression ";"
		 */
		static void AssignStmt() {
			Match(Token.Id);
			byte strId = AddString(GetString());
			Match(Token.Equal);
			Expression();
			Match(Token.Semi);
			Emit(Inst.PopVar, strId);
		}

		/**
		 * expression -> addition
		 */
		static void Expression() {
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
			} else if (Match(Token.Id)) {
				Emit(Inst.PushVar, AddString(GetString()));
			} else {
				Eat(Token.LParen);
				Expression();
				Eat(Token.RParen);
			}
		}

		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string Recant() {
			sb.Clear();

			int i = 0, m = marks[0];
			while (i < icount) {
				switch (insts[i]) {
				case Inst.BinarySubtract: sb.AppendFormat("{0,3} {1,3}: BINARY_SUBTRACT \n", marks[i], insts[i++]); break;
				case Inst.BinaryAdd:      sb.AppendFormat("{0,3} {1,3}: BINARY_ADD      \n", marks[i], insts[i++]); break;
				case Inst.BinaryDivide:   sb.AppendFormat("{0,3} {1,3}: BINARY_DIVIDE   \n", marks[i], insts[i++]); break;
				case Inst.BinaryMultiply: sb.AppendFormat("{0,3} {1,3}: BINARY_MULTIPLY \n", marks[i], insts[i++]); break;

				case Inst.UnaryNot:       sb.AppendFormat("{0,3} {1,3}: UNARY_NOT       \n", marks[i], insts[i++]); break;
				case Inst.UnaryNegative:  sb.AppendFormat("{0,3} {1,3}: UNARY_NEGATIVE  \n", marks[i], insts[i++]); break;
					
				case Inst.PushConst:      sb.AppendFormat("{0,3} {1,3}: PUSH_CONST      {2,3} ({3})\n", marks[i], insts[i++], insts[i], numbers[insts[i++]]); break;

				case Inst.PushVar:        sb.AppendFormat("{0,3} {1,3}: PUSH_VAR        {2,3} ({3})\n", marks[i], insts[i++], insts[i], strings[insts[i++]]); break;
				case Inst.PopVar:         sb.AppendFormat("{0,3} {1,3}: POP_VAR         {2,3} ({3})\n", marks[i], insts[i++], insts[i], strings[insts[i++]]); break;
				case Inst.PopNewVar:      sb.AppendFormat("{0,3} {1,3}: POP_NEW_VAR     {2,3} ({3})\n", marks[i], insts[i++], insts[i], strings[insts[i++]]); break;
					
				case Inst.CloneEnv:       sb.AppendFormat("{0,3} {1,3}: CLONE_ENV       \n", marks[i], insts[i++]); break;
				case Inst.RestoreEnv:     sb.AppendFormat("{0,3} {1,3}: RESTORE_ENV     \n", marks[i], insts[i++]); break;
					
				case Inst.Print:          sb.AppendFormat("{0,3} {1,3}: PRINT           \n", marks[i], insts[i++]); break;
					
				case Inst.PushConstNull:  sb.AppendFormat("{0,3} {1,3}: PUSH_CONST_NULL \n", marks[i], insts[i++]); break;

				default:
					throw new System.Exception("Unrecognized instruction " + insts[i]);
				}

				if (i < icount && m != marks[i]) { m = marks[i]; sb.AppendLine(); }
			}

			return sb.ToString();
		}
	}
}

