namespace FuScript {
	public static class Compiler {
		static ushort pos;

		public static readonly byte[] insts = new byte[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort icount;

		public static readonly System.Collections.Generic.Dictionary<string, ushort> stringDict = new System.Collections.Generic.Dictionary<string, ushort>();
		public static readonly System.Collections.Generic.Dictionary<double, ushort> numberDict = new System.Collections.Generic.Dictionary<double, ushort>();
		public static readonly string[] strings = new string[256];
		public static readonly double[] numbers = new double[256];
		public static ushort scount, ncount;

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

		static double EatNumber() {
			return Lexer.numbers[Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8];
		}

		static string EatString() {
			return Lexer.strings[Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8];
		}

		static void Emit(byte opcode) {
			marks[icount] = pos;
			insts[icount++] = opcode;
		}

		static void Emit(byte opcode, ushort operand) {
			marks[icount] = pos;
			insts[icount++] = opcode;
			insts[icount++] = (byte)(operand & 0xff);
			insts[icount++] = (byte)(operand >> 8);
		}

		static void Fill(ushort ic, ushort operand) {
			ic += 1;  // Opcode
			insts[ic++] = (byte)(operand & 0xff);
			insts[ic++] = (byte)(operand >> 8);
		}

		static ushort PoolNumber(double value) {
			if (!numberDict.ContainsKey(value)) {
				numbers[ncount] = value;
				numberDict[value] = scount;
				return ncount++;
			}
			return numberDict[value];
		}

		static ushort PoolString(string value) {
			if (!stringDict.ContainsKey(value)) {
				strings[scount] = value;
				stringDict[value] = scount;
				return scount++;
			}
			return stringDict[value];
		}

		public static void Compile() {
//			pos = 0;
//			icount = 0; scount = 0; ncount = 0;
//			numberDict.Clear(); stringDict.Clear();

			Declaration();
		}

		/**
		 * declaration -> varDecl
		 *              | funcDecl
		 *              | statement
		 */
		static void Declaration() {
			if (Peek(Token.Var)) VarDecl();
			else if (Peek(Token.Function)) FuncDecl();
			else Statement();
		}

		/**
		 * varDecl -> "var" IDENTIFIER ("=" expression)? ";"
		 */
		static void VarDecl() {
			Eat(Token.Var);
			Eat(Token.Id);
			ushort strId = PoolString(EatString());
			if (Match(Token.Equal)) {
				Expression();
			} else Emit(Opcode.PushConstNull);
			Emit(Opcode.PopNewVar, strId);
			Eat(Token.Semi);
		}

		/**
		 * funcDecl -> "function" IDENTIFIER "(" (IDENTIFIER ("," IDENTIFIER)*)? ")" block
		 */
		static void FuncDecl() {
			Eat(Token.Function);
			Eat(Token.Id);
			Emit(Opcode.PushString, PoolString(EatString()));

			Eat(Token.LParen);
			if (!Peek(Token.RParen)) {
				do {
					Eat(Token.Id);
					Emit(Opcode.PushString, PoolString(EatString()));
				} while (Match(Token.Comma));
			}
			Eat(Token.RParen);

			ushort jmpIc = pos;
			Emit(Opcode.Jump, 0);
			Block();
			Emit(Opcode.PushConstNull);
			Emit(Opcode.Return);
			Fill(jmpIc, pos);
			Emit(Opcode.PopNewVar);
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
			Emit(Opcode.CloneEnv);
			while (!Match(Token.RCurly)) Declaration();
			Emit(Opcode.RestoreEnv);
		}

		/**
		 * printStmt -> "print" expression ";"
		 */
		static void PrintStmt() {
			Match(Token.Print);
			Expression();
			Match(Token.Semi);
			Emit(Opcode.Print);
		}

		/**
		 * assignStmt -> identifier "=" expression ";"
		 */
		static void AssignStmt() {
			Match(Token.Id);
			ushort strId = PoolString(EatString());
			Match(Token.Equal);
			Expression();
			Match(Token.Semi);
			Emit(Opcode.PopVar, strId);
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
					Emit(Opcode.BinarySubtract);
				} else if (Match(Token.Plus)) {
					Multiplication();
					Emit(Opcode.BinaryAdd);
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
					Emit(Opcode.BinaryDivide);
				} else if (Match(Token.Star)) {
					Unary();
					Emit(Opcode.BinaryMultiply);
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
				Emit(Opcode.UnaryNot);
			} else if (Match(Token.Minus)) {
				Primary();
				Emit(Opcode.UnaryNegative);
			} else Primary();
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary() {
			if (Match(Token.Number)) {
				double num = EatNumber();
				ushort shrt = (ushort)num;
				if (System.Math.Abs(shrt - num) < float.Epsilon) {
					Emit(Opcode.PushSmallInt, shrt);
				} else Emit(Opcode.PushNumber, PoolNumber(num));
			} else if (Match(Token.Id)) {
				Emit(Opcode.PushVar, PoolString(EatString()));
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
				case Opcode.BinarySubtract: sb.AppendFormat("{0,3} {1,3}: BINARY_SUBTRACT \n", marks[i], insts[i++]); break;
				case Opcode.BinaryAdd:      sb.AppendFormat("{0,3} {1,3}: BINARY_ADD      \n", marks[i], insts[i++]); break;
				case Opcode.BinaryDivide:   sb.AppendFormat("{0,3} {1,3}: BINARY_DIVIDE   \n", marks[i], insts[i++]); break;
				case Opcode.BinaryMultiply: sb.AppendFormat("{0,3} {1,3}: BINARY_MULTIPLY \n", marks[i], insts[i++]); break;

				case Opcode.UnaryNot:       sb.AppendFormat("{0,3} {1,3}: UNARY_NOT       \n", marks[i], insts[i++]); break;
				case Opcode.UnaryNegative:  sb.AppendFormat("{0,3} {1,3}: UNARY_NEGATIVE  \n", marks[i], insts[i++]); break;
					
				case Opcode.PushSmallInt:   sb.AppendFormat("{0,3} {1,3}: PUSH_SMALL_INT  {2,3}\n", marks[i], insts[i++], insts[i++] | insts[i++] << 8); break;
					
				case Opcode.PushNumber:     sb.AppendFormat("{0,3} {1,3}: PUSH_NUMBER     {2,3} ({3})\n", marks[i], insts[i++], insts[i] | insts[i + 1] << 8, numbers[insts[i++] | insts[i++] << 8]); break;
				case Opcode.PushString:     sb.AppendFormat("{0,3} {1,3}: PUSH_STRING     {2,3} ({3})\n", marks[i], insts[i++], insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;

				case Opcode.PushVar:        sb.AppendFormat("{0,3} {1,3}: PUSH_VAR        {2,3} ({3})\n", marks[i], insts[i++], insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
				case Opcode.PopVar:         sb.AppendFormat("{0,3} {1,3}: POP_VAR         {2,3} ({3})\n", marks[i], insts[i++], insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
				case Opcode.PopNewVar:      sb.AppendFormat("{0,3} {1,3}: POP_NEW_VAR     {2,3} ({3})\n", marks[i], insts[i++], insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					
				case Opcode.CloneEnv:       sb.AppendFormat("{0,3} {1,3}: CLONE_ENV       \n", marks[i], insts[i++]); break;
				case Opcode.RestoreEnv:     sb.AppendFormat("{0,3} {1,3}: RESTORE_ENV     \n", marks[i], insts[i++]); break;
					
				case Opcode.Print:          sb.AppendFormat("{0,3} {1,3}: PRINT           \n", marks[i], insts[i++]); break;
					
				case Opcode.PushConstNull:  sb.AppendFormat("{0,3} {1,3}: PUSH_CONST_NULL \n", marks[i], insts[i++]); break;

				default:
					throw new System.Exception("Unrecognized instruction " + insts[i]);
				}

				if (i < icount && m != marks[i]) { m = marks[i]; sb.AppendLine(); }
			}

			return sb.ToString();
		}
	}
}

