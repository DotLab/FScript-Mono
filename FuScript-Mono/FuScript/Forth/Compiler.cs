namespace FuScript.Forth {
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

	public class TypeStackUnderflowException : CompilerException {
		public TypeStackUnderflowException(byte t) : base("TypeStack underflow, expecting " + Compiler.RecantType(t)) { }
	}

	public class VariableUndefinedException : CompilerException {
		public VariableUndefinedException(string id) : base("Variable '" + id + "' is undefiend") { }
	}

	public class VariableAlreadyDefinedException : CompilerException {
		public VariableAlreadyDefinedException(string id, byte t) : base("Variable '" + id + "' is already defined as " + Compiler.RecantType(t)) { }
	}

	public class UnrecognizedOpcodeException : CompilerException {
		public UnrecognizedOpcodeException(byte t) : base("Unrecognized opcode #" + t) { }
	}

	public static class Compiler {
		const byte NIL = 0, BOO = 1, INT = 2, FLO = 3, STR = 4;

		public static string RecantType(byte t) {
			switch (t) {
				case NIL: return "NIL";
				case BOO: return "BOO";
				case INT: return "INT";
				case FLO: return "FLO";
				case STR: return "STR";
				default: throw new CompilerException("Unrecognized type #" + t);
			}
		}

		static System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string RecantTypeStack() {
			sb.Clear();
			sb.Append("    [ ");
			for (int i = 0; i <= typeStackPointer; i++) {
				sb.Append(RecantType(typeStack[i]));
				if (i + 1 <= typeStackPointer) sb.Append(", ");
				else sb.Append(" ");
			}
			sb.Append("]\n");
			return sb.ToString();
		}

		static ushort pos, length;

		public static readonly byte[] insts = new byte[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort instCount, stmtCount;

		static readonly byte[] typeStack = new byte[256];
		static int typeStackPointer = -1;

		static readonly System.Collections.Generic.Dictionary<ushort, byte> varDict = new System.Collections.Generic.Dictionary<ushort, byte>();
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

		static void Emit(byte opcode) {
			if (isScanning) return;

			marks[instCount] = stmtCount;
			insts[instCount++] = opcode;
		}

		static void Emit(byte opcode, ushort operand) {
			if (isScanning) return;

			marks[instCount] = stmtCount;
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
			return typeStackPointer < 0 ? NIL : typeStack[typeStackPointer];
		}

		static void PushType(byte t) {
			typeStack[++typeStackPointer] = t;
			Log(RecantTypeStack());
		}

		static bool MatchType(byte t) {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException(t);
			if (typeStack[typeStackPointer] == t) { typeStackPointer -= 1; Log(RecantTypeStack()); return true; }
			return false;
		}

		static void PopType(byte t) {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException(t);
			if (typeStack[typeStackPointer] == t) { typeStackPointer -= 1; Log(RecantTypeStack()); return; }
			throw new UnexpectedTypeException(typeStack[typeStackPointer], t);
		}

		public static void Reset() {
			pos = 0; instCount = 0; stmtCount = 0; typeStackPointer = -1;
			isScanning = false;
		}

		public static void Compile() {
			length = Lexer.tokenCount;

			Statement();
		}

		/**
		 * statement -> block
		 *            | varDecl
		 *            | funcDecl
		 *            | ifStmt
		 *            | whileStmt
		 *            | forStmt
		 *            | returnStmt
		 *            | printStmt
		 *            | exprStmt
		 */
		public static void Statement() {
			if (Peek(Token.LCurly)) Block();
			else if (Peek(Token.KVar)) VarDecl();
			//else if (Peek(Token.KFunction)) FuncDecl();
			else if (Peek(Token.KIf)) IfStmt();
			else if (Peek(Token.KWhile)) WhileStmt();
			//else if (Peek(Token.KReturn)) ReturnStmt();
			//else if (Peek(Token.KPrint)) PrintStmt();
			else ExprStmt();

			stmtCount += 1;
		}

		/**
		 * block -> "{" declaration* "}"
		 */
		static void Block() {
			Eat(Token.LCurly);
			while (!Match(Token.RCurly)) Statement();
		}

		/**
		 * varDecl -> "var" IDENTIFIER ("=" expression)? ";"
		 */
		static void VarDecl() {
			Eat(Token.KVar); Eat(Token.Id);
			ushort id = EatLiteral(); Eat(Token.Equal);
			Expression(); Eat(Token.Semi);

			if (varDict.ContainsKey(id)) throw new VariableAlreadyDefinedException(Lexer.strings[id], varDict[id]);

			if      (MatchType(INT)) { varDict[id] = INT; Emit(Opcode.POP_NEW_VAR_INT, id); }
			else if (MatchType(FLO)) { varDict[id] = FLO; Emit(Opcode.POP_NEW_VAR_FLO, id); }
			else if (MatchType(BOO)) { varDict[id] = BOO; Emit(Opcode.POP_NEW_VAR_BOO, id); }
			else if (MatchType(STR)) { varDict[id] = STR; Emit(Opcode.POP_NEW_VAR_STR, id); }
			else if (MatchType(NIL)) { varDict[id] = NIL; Emit(Opcode.POP_NEW_VAR_NIL, id); }
			else throw new UnexpectedTypeException(CurrentType(), INT, FLO, BOO, STR, NIL);
		}

		/**
		 * ifStmt -> "if" "(" expression ")" statement ("else" statement)?
		 */
		static void IfStmt() {
			Eat(Token.KIf); Eat(Token.LParen);
			Expression(); PopType(BOO);
			// ushort ifIc = icount;
			// Emit(Opcode.BranchIfFalsy, 0);
			Eat(Token.RParen);

			Statement();

			if (Match(Token.KElse)) {
				// ushort elseJmpIc = icount;
				// Emit(Opcode.Jump, 0);
				// ushort elseIc = icount;
				Statement();
				// Fill(elseJmpIc, icount);
				// Fill(ifIc, elseIc);
			}
			// } else Fill(ifIc, icount);
		}

		/**
		 * whileStmt -> "while" "(" expression ")" statement
		 */
		static void WhileStmt() {
			Eat(Token.KWhile); Eat(Token.LParen);
			// ushort condIc = icount;
			Expression(); PopType(BOO);
			// ushort branchIc = icount;
			// Emit(Opcode.BranchIfFalsy, 0);
			Eat(Token.RParen);

			Statement();
			// Emit(Opcode.Jump, condIc);
			// Fill(branchIc, icount);
		}

		/**
		 * exprStmt -> expression ";"
		 */
		static void ExprStmt() {
			Expression();
			Eat(Token.Semi);
			if      (MatchType(INT)) { Emit(Opcode.POP_INT); }
			else if (MatchType(FLO)) { Emit(Opcode.POP_FLO); }
			else if (MatchType(BOO)) { Emit(Opcode.POP_BOO); }
			else if (MatchType(STR)) { Emit(Opcode.POP_STR); }
		}

		/**
		 * expression -> assignment
		 */
		static void Expression() {
			LogicOr();
		}

		/**
		 * logic_or -> logic_and ("||" logic_and)*
		 */
		static void LogicOr() {
			LogicAnd();
			while (Match(Token.OrOr)) {
				PopType(BOO); LogicAnd(); PopType(BOO); PushType(BOO); Emit(Opcode.BIN_OR);
			}
		}

		/**
		 * logic_and -> equality ("&&" equality)*
		 */
		static void LogicAnd() {
			Equality();
			while (Match(Token.AndAnd)) {
				PopType(BOO); Equality(); PopType(BOO); PushType(BOO); Emit(Opcode.BIN_AND);
			}
		}

		/**
		 * equality -> comparison (("!=" | "==") comparison)*
		 */
		static void Equality() {
			Comparision();

			while (true) {
				if (Match(Token.BangEqual)) {
					if      (MatchType(INT)) { Comparision(); PopType(INT); Emit(Opcode.BIN_EQ_INT); Emit(Opcode.UNARY_NOT); PushType(BOO); }
					else if (MatchType(FLO)) { Comparision(); PopType(FLO); Emit(Opcode.BIN_EQ_FLO); Emit(Opcode.UNARY_NOT); PushType(BOO); }
					else if (MatchType(BOO)) { Comparision(); PopType(BOO); Emit(Opcode.BIN_EQ_BOO); Emit(Opcode.UNARY_NOT); PushType(BOO); }
					else if (MatchType(STR)) { Comparision(); PopType(STR); Emit(Opcode.BIN_EQ_STR); Emit(Opcode.UNARY_NOT); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.EqualEqual)) {
					if      (MatchType(INT)) { Comparision(); PopType(INT); Emit(Opcode.BIN_EQ_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Comparision(); PopType(FLO); Emit(Opcode.BIN_EQ_FLO); PushType(BOO); }
					else if (MatchType(BOO)) { Comparision(); PopType(BOO); Emit(Opcode.BIN_EQ_BOO); PushType(BOO); }
					else if (MatchType(STR)) { Comparision(); PopType(STR); Emit(Opcode.BIN_EQ_STR); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else break;
			}
		}

		/**
		 * comparison -> bitwise ((">" | ">=" | "<" | "<=") bitwise)*
		 */
		static void Comparision() {
			Bitwise();

			while (true) {
				if (Match(Token.LAngle)) {
					if      (MatchType(INT)) { Bitwise(); PopType(INT); Emit(Opcode.BIN_LT_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Bitwise(); PopType(FLO); Emit(Opcode.BIN_LT_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.LAngleEqual)) {
					if      (MatchType(INT)) { Bitwise(); PopType(INT); Emit(Opcode.BIN_LEQ_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Bitwise(); PopType(FLO); Emit(Opcode.BIN_LEQ_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.RAngle)) {
					if      (MatchType(INT)) { Bitwise(); PopType(INT); Emit(Opcode.BIN_GT_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Bitwise(); PopType(FLO); Emit(Opcode.BIN_GT_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.RAngleEqual)) {
					if      (MatchType(INT)) { Bitwise(); PopType(INT); Emit(Opcode.BIN_GEQ_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Bitwise(); PopType(FLO); Emit(Opcode.BIN_GEQ_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else break;
			}
		}

		/**
		 * bitwise -> multiplication (("<<" | ">>" | "&" | "|" | "^") multiplication)*
		 */
		static void Bitwise() {
			Addition();

			while (true) {
				if (Match(Token.LAngleAngle)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_BIT_SHL); PushType(INT); }
					else throw new UnexpectedTypeException(CurrentType(), INT);
				} else if (Match(Token.RAngleAngle)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_BIT_SHR); PushType(INT); }
					else throw new UnexpectedTypeException(CurrentType(), INT);
				} else if (Match(Token.And)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_BIT_AND); PushType(INT); }
					else throw new UnexpectedTypeException(CurrentType(), INT);
				} else if (Match(Token.Or)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_BIT_OR); PushType(INT); }
					else throw new UnexpectedTypeException(CurrentType(), INT);
				} else if (Match(Token.Caret)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_BIT_XOR); PushType(INT); }
					else throw new UnexpectedTypeException(CurrentType(), INT);
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
					if      (MatchType(INT)) { Multiplication(); PopType(INT); Emit(Opcode.BIN_SUB_INT); PushType(INT); }
					else if (MatchType(FLO)) { Multiplication(); PopType(FLO); Emit(Opcode.BIN_SUB_FLO); PushType(FLO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.Plus)) {
					if      (MatchType(INT)) { Multiplication(); PopType(INT); Emit(Opcode.BIN_ADD_INT); PushType(INT); }
					else if (MatchType(FLO)) { Multiplication(); PopType(FLO); Emit(Opcode.BIN_ADD_FLO); PushType(FLO); }
					else if (MatchType(STR)) { Multiplication(); PopType(STR); Emit(Opcode.BIN_ADD_STR); PushType(STR); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
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
					if      (MatchType(INT)) { Unary(); PopType(INT); Emit(Opcode.BIN_DIV_INT); PushType(INT); }
					else if (MatchType(FLO)) { Unary(); PopType(FLO); Emit(Opcode.BIN_DIV_FLO); PushType(FLO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.Star)) {
					if      (MatchType(INT)) { Unary(); PopType(INT); Emit(Opcode.BIN_MUL_INT); PushType(INT); }
					else if (MatchType(FLO)) { Unary(); PopType(FLO); Emit(Opcode.BIN_MUL_FLO); PushType(FLO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
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
				Emit(Opcode.UNARY_NOT); PopType(BOO); PushType(BOO);
			} else if (Match(Token.Minus)) {
				Primary();
				if      (MatchType(INT)) { Emit(Opcode.UNARY_NEG_INT); PushType(INT); }
				else if (MatchType(FLO)) { Emit(Opcode.UNARY_NEG_FLO); PushType(FLO); }
				else throw new UnexpectedTypeException(CurrentType(), INT, FLO);
			} else Primary();
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary() {
			if      (Match(Token.Int))    { Emit(Opcode.PUSH_CONST_INT, EatLiteral()); PushType(INT); }
			else if (Match(Token.Float))  { Emit(Opcode.PUSH_CONST_FLO, EatLiteral()); PushType(FLO); }
			else if (Match(Token.String)) { Emit(Opcode.PUSH_CONST_STR, EatLiteral()); PushType(STR); }
			else if (Match(Token.KFalse)) { Emit(Opcode.PUSH_FALSE); PushType(BOO); }
			else if (Match(Token.KTrue))  { Emit(Opcode.PUSH_TRUE); PushType(BOO); }
			else if (Match(Token.KNull))  { Emit(Opcode.PUSH_NULL); PushType(NIL); }
			else if (Match(Token.LParen)) { Expression(); Eat(Token.RParen); }
			else if (Match(Token.Id)) {
				ushort id = EatLiteral();
				if (!varDict.ContainsKey(id)) throw new VariableUndefinedException(Lexer.strings[id]);
				byte type = varDict[id];
				if      (type == INT) { Emit(Opcode.PUSH_VAR_INT, id); PushType(INT); }
				else if (type == FLO) { Emit(Opcode.PUSH_VAR_FLO, id); PushType(FLO); }
				else if (type == BOO) { Emit(Opcode.PUSH_VAR_BOO, id); PushType(BOO); }
				else if (type == STR) { Emit(Opcode.PUSH_VAR_STR, id); PushType(STR); }
				else if (type == NIL) { Emit(Opcode.PUSH_VAR_NIL, id); PushType(NIL); }
				else throw new UnexpectedTypeException(CurrentType(), INT, FLO, BOO, STR, NIL);
			} else throw new UnexpectedTokenException(Current(), Token.Int, Token.Float, Token.String, Token.KFalse, Token.KTrue, Token.KNull, Token.LParen);
		}

		public static string Recant() {
			return "dfasdf";
		}
	}
}
