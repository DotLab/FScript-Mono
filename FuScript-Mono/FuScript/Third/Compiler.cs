namespace FuScript.Third {
	public static class Compiler {
		static ushort pos, length;

		public static readonly byte[] insts = new byte[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort icount, dcount;

		public static readonly System.Collections.Generic.Dictionary<string, ushort> stringDict = new System.Collections.Generic.Dictionary<string, ushort>();
		public static readonly System.Collections.Generic.Dictionary<double, ushort> numberDict = new System.Collections.Generic.Dictionary<double, ushort>();
		public static readonly string[] strings = new string[256];
		public static readonly double[] numbers = new double[256];
		public static ushort scount, ncount;

		public static bool scaning;

		static void Print(string str) {
			System.Console.WriteLine(str);
		}

		static bool Peek(byte t) {
			return pos < length && Lexer.tokens[pos] == t;
		}

		static bool Match(byte t) {
			if (pos < length && Lexer.tokens[pos] == t) { pos += 1; return true; }
			return false;
		}

		static int Eat(byte t) {
			if (pos < length && Lexer.tokens[pos] == t) return pos++;
			throw new System.Exception("Compiler: Unexpected token " + Lexer.tokens[pos] + " at " + pos + ", expecting " + t);
		}

		static double EatNumber() {
			return Lexer.numbers[Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8];
		}

		static string EatString() {
			return Lexer.strings[Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8];
		}

		static void Emit(byte opcode) {
			if (scaning) return;

			marks[icount] = dcount;
			insts[icount++] = opcode;
		}

		static void Emit(byte opcode, ushort operand) {
			if (scaning) return;

			marks[icount] = dcount;
			insts[icount++] = opcode;
			insts[icount++] = (byte)(operand & 0xff);
			insts[icount++] = (byte)(operand >> 8);
		}

		static void Fill(ushort ic, ushort operand) {
			if (scaning) return;

			ic += 1;  // Opcode
			insts[ic++] = (byte)(operand & 0xff);
			insts[ic++] = (byte)(operand >> 8);
		}

		static ushort PoolNumber(double value) {
			if (!numberDict.ContainsKey(value)) {
				numbers[ncount] = value;
				numberDict[value] = ncount;
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
			length = Lexer.tcount;
			// pos = 0;
			// icount = 0; scount = 0; ncount = 0;
			// numberDict.Clear(); stringDict.Clear();

			while (pos < length) Declaration();
			// Eat(Token.Eof);
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

			dcount += 1;
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

			Function(false);

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
			else if (Peek(Token.If)) IfStmt();
			else if (Peek(Token.While)) WhileStmt();
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
		 * ifStmt -> "if" "(" expression ")" statement ("else" statement)?
		 */
		static void IfStmt() {
			Eat(Token.If);
			Eat(Token.LParen);
			Expression();
			ushort ifIc = icount;
			Emit(Opcode.BranchIfFalsy, 0);
			Eat(Token.RParen);

			Statement();

			if (Match(Token.Else)) {
				ushort elseJmpIc = icount;
				Emit(Opcode.Jump, 0);
				ushort elseIc = icount;
				Statement();
				Fill(elseJmpIc, icount);
				Fill(ifIc, elseIc);
			} else Fill(ifIc, icount);
		}

		/**
		 * whileStmt -> "while" "(" expression ")" statement
		 */
		static void WhileStmt() {
			Eat(Token.While);

			Eat(Token.LParen);
			ushort condIc = icount;
			Expression();
			ushort branchIc = icount;
			Emit(Opcode.BranchIfFalsy, 0);
			Eat(Token.RParen);

			Statement();
			Emit(Opcode.Jump, condIc);
			Fill(branchIc, icount);
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
		 * assignment -> function
		 *             | object
		 *             | identifier "=" expression
		 *             | addition
		 */
		static void Assignment() {
			if (Peek(Token.Function)) {
				Function();
			} else if (Peek(Token.LCurly)) {
				Object();
			} else {
				ushort start = pos;
				bool st = scaning;
				scaning = true;
				LogicOr();
				scaning = st;

				if (Match(Token.Equal)) {  // Assign
					Expression();
					ushort end = pos;
					pos = start;
					LogicOr();
					pos = end;
				} else {  // Not assign;
					pos = start;
					LogicOr();
				}
			}
		}


		/**
		 * function -> "function" "(" (IDENTIFIER ("," IDENTIFIER)*)? ")" block
		 */
		static void Function(bool needHeader = true) {
			if (needHeader) Eat(Token.Function);

			// Function start
			ushort jmpIc = icount;
			Emit(Opcode.Jump, 0);

			// Clone env
			ushort funcIc = icount;

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
			Emit(Opcode.Return);

			// Function end
			Fill(jmpIc, icount);

			Emit(Opcode.PushSmallInt, funcIc);
			Emit(Opcode.MakeFunction, argc);
		}

		/**
		 * object -> "{" (IDENTIFIER ":" expression ("," IDENTIFIER ":" expression)*)? "}"
		 */
		static void Object() {
			Eat(Token.LCurly);

			Emit(Opcode.MakeObject);

			if (!Match(Token.RCurly)) {
				do {
					Eat(Token.Id);
					var strId = PoolString(EatString());
					Eat(Token.Colon);
					Expression();
					Emit(Opcode.MakeObjectMember, strId);
				} while (Match(Token.Comma));
				Eat(Token.RCurly);
			}
		}

		/**
		 * logic_or -> logic_and ("||" logic_and)*
		 */
		static void LogicOr() {
			LogicAnd();
			while (Match(Token.OrOr)) {
				LogicAnd();
				Emit(Opcode.BinaryLogicOr);
			}
		}

		/**
		 * logic_and -> equality ("&&" equality)*
		 */
		static void LogicAnd() {
			Equality();
			while (Match(Token.AndAnd)) {
				LogicAnd();
				Emit(Opcode.BinaryLogicAnd);
			}
		}

		/**
		 * equality -> comparison (("!=" | "==") comparison)*
		 */
		static void Equality() {
			Comparision();
			while (true) {
				if (Match(Token.BangEqual)) {
					Comparision();
					Emit(Opcode.BinaryEqual);
					Emit(Opcode.UnaryNot);
				} else if (Match(Token.EqualEqual)) {
					Comparision();
					Emit(Opcode.BinaryEqual);
				} else break;
			}
		}

		/**
		 * comparison -> addition ((">" | ">=" | "<" | "<=") addition)*
		 */
		static void Comparision() {
			Addition();
			while (true) {
				if (Match(Token.LAngle)) {
					Addition();
					Emit(Opcode.BinaryLess);
				} else if (Match(Token.LAngleEqual)) {
					Addition();
					Emit(Opcode.BinaryLessEqual);
				} else if (Match(Token.RAngle)) {
					Addition();
					Emit(Opcode.BinaryGreater);
				} else if (Match(Token.RAngleEqual)) {
					Addition();
					Emit(Opcode.BinaryGreaterEqual);
				} else break;
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
		 * call -> primary ("(" (expression ("," expression)*)? ")" | "." IDENTIFIER)*
		 */
		static void Call() {
			Primary();

			ushort argc;
			while (true) {
				if (Match(Token.LParen)) {
					argc = 0;
					if (!Match(Token.RParen)) {
						do {
							argc += 1;
							Expression();
						} while (Match(Token.Comma));
						Eat(Token.RParen);
					}
					Emit(Opcode.CallFunction, argc);
				} else if (Match(Token.Dot)) {
					Eat(Token.Id);
					var strId = PoolString(EatString());
					if (Peek(Token.Equal)) {
						Emit(Opcode.ObjectMemberSet, strId);
					} else Emit(Opcode.ObjectMemberGet, strId);
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
			} else if (Match(Token.String)) Emit(Opcode.PushString, PoolString(EatString()));
			else if (Match(Token.False)) Emit(Opcode.PushConstFalse);
			else if (Match(Token.True)) Emit(Opcode.PushConstTrue);
			else if (Match(Token.Null)) Emit(Opcode.PushConstNull);
			else if (Match(Token.Id)) {
				ushort strId = PoolString(EatString());
				if (Peek(Token.Equal)) {  // assignment
					Emit(Opcode.PeekVar, strId);
				} else {
					Emit(Opcode.PushVar, strId);
				}
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
					case Opcode.BinarySubtract:           sb.Append("BINARY_SUBTRACT      \n"); break;
					case Opcode.BinaryAdd:                sb.Append("BINARY_ADD           \n"); break;
					case Opcode.BinaryDivide:             sb.Append("BINARY_DIVIDE        \n"); break;
					case Opcode.BinaryMultiply:           sb.Append("BINARY_MULTIPLY      \n"); break;

					case Opcode.BinaryLogicOr:            sb.Append("BINARY_LOGIC_OR      \n"); break;
					case Opcode.BinaryLogicAnd:           sb.Append("BINARY_LOGIC_AND     \n"); break;
					case Opcode.BinaryEqual:              sb.Append("BINARY_EQUAL         \n"); break;
					// case Opcode.BinaryNotEqual:           sb.Append("BINARY_NOT_EQUAL     \n"); break;
					case Opcode.BinaryLess:               sb.Append("BINARY_LESS          \n"); break;
					case Opcode.BinaryLessEqual:          sb.Append("BINARY_LESS_EQUAL    \n"); break;
					case Opcode.BinaryGreater:            sb.Append("BINARY_GREATER       \n"); break;
					case Opcode.BinaryGreaterEqual:       sb.Append("BINARY_GREATER_EQUAL \n"); break;

					case Opcode.UnaryNot:                 sb.Append("UNARY_NOT            \n"); break;
					case Opcode.UnaryNegative:            sb.Append("UNARY_NEGATIVE       \n"); break;

					case Opcode.CloneEnv:                 sb.Append("CLONE_ENV            \n"); break;
					case Opcode.RestoreEnv:               sb.Append("RESTORE_ENV          \n"); break;

					case Opcode.Print:                    sb.Append("PRINT                \n"); break;
					case Opcode.Return:                   sb.Append("RETURN               \n"); break;

					case Opcode.PushConstTrue:            sb.Append("PUSH_CONST_TRUE      \n"); break;
					case Opcode.PushConstFalse:           sb.Append("PUSH_CONST_FALSE     \n"); break;
					case Opcode.PushConstNull:            sb.Append("PUSH_CONST_NULL      \n"); break;
					case Opcode.PopDiscard:               sb.Append("POP_DISCARD          \n"); break;

					case Opcode.MakeObject:               sb.Append("MAKE_OBJECT          \n"); break;

					case Opcode.PushSmallInt:       sb.AppendFormat("PUSH_SMALL_INT       {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.Jump:               sb.AppendFormat("JUMP                 {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.BranchIfFalsy:      sb.AppendFormat("BRANCH_IF_FALSY      {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.CallFunction:       sb.AppendFormat("CALL_FUNCTION        {0,5}\n", insts[i++] | insts[i++] << 8); break;
					case Opcode.MakeFunction:       sb.AppendFormat("MAKE_FUNCTION        {0,5}\n", insts[i++] | insts[i++] << 8); break;

					case Opcode.PushNumber:         sb.AppendFormat("PUSH_NUMBER          {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, numbers[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PushString:         sb.AppendFormat("PUSH_STRING          {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;

					case Opcode.PushVar:            sb.AppendFormat("PUSH_VAR             {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PopVar:             sb.AppendFormat("POP_VAR              {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PeekVar:            sb.AppendFormat("PEEK_VAR             {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.PopNewVar:          sb.AppendFormat("POP_NEW_VAR          {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;

					case Opcode.ObjectMemberGet:    sb.AppendFormat("OBJECT_MEMBER_GET    {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.ObjectMemberSet:    sb.AppendFormat("OBJECT_MEMBER_SET    {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;
					case Opcode.MakeObjectMember:   sb.AppendFormat("MAKE_OBJECT_MEMBER   {0,5} ({1})\n", insts[i] | insts[i + 1] << 8, strings[insts[i++] | insts[i++] << 8]); break;

					default:
						sb.AppendFormat("UNKNOWN         \n"); return sb.ToString();
					}
				} catch (System.Exception e) {
					sb.Append("\ninsts: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", insts[i]);
					sb.Append("\nmarks: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", marks[i]);
					sb.Append("\ncontr: ");
					for (i = 0; i < icount; i++) sb.AppendFormat("{0,4}", i);
					sb.Append("\nstrings: ");
					for (i = 0; i < scount; i++) sb.Append(strings[i] + " ");
					sb.Append("\nnumbers: ");
					for (i = 0; i < ncount; i++) sb.Append(numbers[i] + " ");
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

