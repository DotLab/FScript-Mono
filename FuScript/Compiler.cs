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
			System.Console.WriteLine(ic + " <- " + operand);
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
//			Eat(Token.Eof);
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
			ushort strId = PoolString(EatString());

			// Function start
			ushort jmpIc = icount;
			Emit(Opcode.Jump, 0);

			// Clone env
			ushort funcIc = icount;
			Emit(Opcode.CloneEnv);

			// Bind parameters
			Eat(Token.LParen);
			ushort argc = 0;
			if (!Peek(Token.RParen)) {
				do {
					argc += 1;
					Eat(Token.Id);
					Emit(Opcode.PopNewVar, PoolString(EatString()));
				} while (Match(Token.Comma));
			}
			Eat(Token.RParen);

			// Function body
			Block(false);
			// Force return
			Emit(Opcode.PushConstNull);
			Emit(Opcode.RestoreEnv);
			Emit(Opcode.Return);

			// Function end
			Fill(jmpIc, icount);

			Emit(Opcode.PushSmallInt, argc);
			Emit(Opcode.MakeFunction, funcIc);
			Emit(Opcode.PopNewVar, strId);
		}

		/**
		 * statement -> block
		 *            | ifStmt
		 *            | whileStmt
		 *            | forStmt
		 *            | returnStmt
		 *            | printStmt
		 *            | exprStmt
		 */
		public static void Statement() {
			if (Peek(Token.LCurly)) Block();
			else if (Peek(Token.Return)) ReturnStmt();
			else if (Peek(Token.Print)) PrintStmt();
			else ExprStmt();
		}

		/**
		 * block -> "{" declaration* "}"
		 */
		static void Block(bool needNewEnv = true) {
			Eat(Token.LCurly);
			if (needNewEnv) Emit(Opcode.CloneEnv);
			while (!Match(Token.RCurly)) Declaration();
			if (needNewEnv) Emit(Opcode.RestoreEnv);
		}

		/**
		 * returnStmt -> "return" expression? ";"
		 */
		static void ReturnStmt() {
			Eat(Token.Return);
			if (!Peek(Token.Semi)) Expression();
			else Emit(Opcode.PushConstNull);
			Emit(Opcode.Return);
			Eat(Token.Semi);
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
		 * exprStmt -> expression ";"
		 */
		static void ExprStmt() {
			Expression();
			Emit(Opcode.PopDiscard);
			Eat(Token.Semi);
		}

		/**
		 * expression -> assignment
		 */
		static void Expression() {
			Assignment();
		}

		/**
		 * assignment -> identifier "=" expression
		 *             | addition
		 */
		static void Assignment() {
			if (Lexer.tokens[pos] == Token.Id && Lexer.tokens[pos + 3] == Token.Equal) {
				Eat(Token.Id);
				ushort strId = PoolString(EatString());
				Eat(Token.Equal);
				Assignment();
				Emit(Opcode.PeekVar, strId);
			} else {
				Addition();
			}
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
				Call();
				Emit(Opcode.UnaryNot);
			} else if (Match(Token.Minus)) {
				Call();
				Emit(Opcode.UnaryNegative);
			} else Call();
		}

		/**
		 * call -> primary ("(" (expression ("," expression)*)? ")")*
		 */
		static void Call() {
			Primary();

			ushort argc;
			while (true) {
				if (Match(Token.LParen)) {
					System.Console.WriteLine("call ");
					argc = 0;
					if (!Match(Token.RParen)) {
						do {
							argc += 1;
							Expression();
						} while (Match(Token.Comma));
					}
					Emit(Opcode.CallFunction, argc);
				} else break;
			}
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

			byte inst;
			int i = 0, m = marks[0];
			while (i < icount) {
				try {
					sb.AppendFormat("{0,5} {1,3} {2,3}: ", i, marks[i], inst = insts[i++]);
					switch (inst) {
					case Opcode.BinarySubtract: sb.AppendFormat("BINARY_SUBTRACT \n"); break;
					case Opcode.BinaryAdd:      sb.AppendFormat("BINARY_ADD      \n"); break;
					case Opcode.BinaryDivide:   sb.AppendFormat("BINARY_DIVIDE   \n"); break;
					case Opcode.BinaryMultiply: sb.AppendFormat("BINARY_MULTIPLY \n"); break;

					case Opcode.UnaryNot:       sb.AppendFormat("UNARY_NOT       \n"); break;
					case Opcode.UnaryNegative:  sb.AppendFormat("UNARY_NEGATIVE  \n"); break;
						
					case Opcode.CloneEnv:       sb.AppendFormat("CLONE_ENV       \n"); break;
					case Opcode.RestoreEnv:     sb.AppendFormat("RESTORE_ENV     \n"); break;
						
					case Opcode.Print:          sb.AppendFormat("PRINT           \n"); break;
					case Opcode.Return:         sb.AppendFormat("RETURN          \n"); break;
						
					case Opcode.PushConstNull:  sb.AppendFormat("PUSH_CONST_NULL \n"); break;
					case Opcode.PopDiscard:     sb.AppendFormat("POP_DISCARD     \n"); break;
						
					case Opcode.PushSmallInt:   sb.AppendFormat("PUSH_SMALL_INT  {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.Jump:           sb.AppendFormat("JUMP            {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.CallFunction:   sb.AppendFormat("CALL_FUNCTION   {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.MakeFunction:   sb.AppendFormat("MAKE_FUNCTION   {0,5}\n", insts[i++] | insts[i++] << 8); break;
						
					case Opcode.PushNumber:     sb.AppendFormat("PUSH_NUMBER     {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, numbers[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PushString:     sb.AppendFormat("PUSH_STRING     {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
						
					case Opcode.PushVar:        sb.AppendFormat("PUSH_VAR        {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PopVar:         sb.AppendFormat("POP_VAR         {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PeekVar:        sb.AppendFormat("PEEK_VAR        {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PopNewVar:      sb.AppendFormat("POP_NEW_VAR     {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;

					default:
						sb.AppendFormat("UNKNOWN         \n"); return sb.ToString();
					}
				} catch (System.Exception e) {
					sb.Append("insts: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", insts[i]);
					sb.Append("\nmarks: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", marks[i]);
					sb.Append("\ncontr: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", i);
					sb.AppendLine();
					sb.AppendLine(e.ToString());
					return sb.ToString();
				}

				if (i < icount && m != marks[i]) { m = marks[i]; sb.AppendLine(); }
			}

			return sb.ToString();
		}
	}
}

