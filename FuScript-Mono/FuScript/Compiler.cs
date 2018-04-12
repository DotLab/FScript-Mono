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
		const byte Null = 0, BOO = 1, INT = 2, FLO = 3, STR = 4;

		public static string RecantType(byte t) {
			switch (t) {
				case Null: return "Null";
				case BOO: return "Boolean";
				case INT: return "Int";
				case FLO: return "Float";
				case STR: return "String";
				default: throw new CompilerException("Unrecognized type #" + t);
			}
		}

		static ushort pos, length;

		public static readonly byte[] insts = new byte[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort instCount, stmtCount;

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
			pos = 0; instCount = 0; stmtCount = 0; typeStackPointer = -1;
			isScanning = false;
		}

		public static void Compile() {
			length = Lexer.tokenCount;

			Statement();
		}

		/**
		 * block -> "{" declaration* "}"
		 */
		static void Block(bool needNewEnv = true) {
			Eat(Token.LCurly);
			//if (needNewEnv) Emit(Opcode.CloneEnv);
			//while (!Match(Token.RCurly)) Declaration();
			//if (needNewEnv) Emit(Opcode.RestoreEnv);
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
			//else if (Peek(Token.If)) IfStmt();
			//else if (Peek(Token.While)) WhileStmt();
			//else if (Peek(Token.Return)) ReturnStmt();
			//else if (Peek(Token.Print)) PrintStmt();
			else ExprStmt();

			stmtCount += 1;
		}

		/**
		 * exprStmt -> expression ";"
		 */
		static void ExprStmt() {
			Expression();
			if      (MatchType(INT)) { PopType(INT); Emit(Opcode.POP_INT); }
			else if (MatchType(FLO)) { PopType(FLO); Emit(Opcode.POP_FLO); }
			else if (MatchType(BOO)) { PopType(BOO); Emit(Opcode.POP_BOO); }
			else if (MatchType(STR)) { PopType(STR); Emit(Opcode.POP_STR); }
			Eat(Token.Semi);
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
				PopType(BOO); LogicAnd(); PushType(BOO); Emit(Opcode.BIN_OR);
			}
		}

		/**
		 * logic_and -> equality ("&&" equality)*
		 */
		static void LogicAnd() {
			Equality();
			while (Match(Token.AndAnd)) {
				PopType(BOO); Equality(); PushType(BOO); Emit(Opcode.BIN_AND);
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
		 * comparison -> addition ((">" | ">=" | "<" | "<=") addition)*
		 */
		static void Comparision() {
			Addition();

			while (true) {
				if (Match(Token.LAngle)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_LT_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Addition(); PopType(FLO); Emit(Opcode.BIN_LT_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.LAngleEqual)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_LEQ_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Addition(); PopType(FLO); Emit(Opcode.BIN_LEQ_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.RAngle)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_GT_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Addition(); PopType(FLO); Emit(Opcode.BIN_GT_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
				} else if (Match(Token.RAngleEqual)) {
					if      (MatchType(INT)) { Addition(); PopType(INT); Emit(Opcode.BIN_GEQ_INT); PushType(BOO); }
					else if (MatchType(FLO)) { Addition(); PopType(FLO); Emit(Opcode.BIN_GEQ_FLO); PushType(BOO); }
					else throw new UnexpectedTypeException(CurrentType(), FLO);
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
				Primary(true);
				Emit(Opcode.UNARY_NOT); PopType(BOO); PushType(BOO);
			} else if (Match(Token.Minus)) {
				Primary(true);
				if      (MatchType(INT)) { Emit(Opcode.UNARY_NEG_INT); PushType(INT); }
				else if (MatchType(FLO)) { Emit(Opcode.UNARY_NEG_FLO); PushType(FLO); }
				else throw new UnexpectedTypeException(CurrentType(), INT, FLO);
			} else Primary(false);
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary(bool forced) {
			if      (Match(Token.Int))    { Emit(Opcode.PUSH_CONST_INT, EatLiteral()); PushType(INT); }
			else if (Match(Token.Float))  { Emit(Opcode.PUSH_CONST_FLO, EatLiteral()); PushType(FLO); }
			else if (Match(Token.String)) { Emit(Opcode.PUSH_CONST_STR, EatLiteral()); PushType(STR); }
			else if (Match(Token.KFalse)) { Emit(Opcode.PUSH_FALSE, EatLiteral()); PushType(BOO); }
			else if (Match(Token.KTrue))  { Emit(Opcode.PUSH_TRUE, EatLiteral()); PushType(BOO); }
			else if (Match(Token.KNull))  { Emit(Opcode.PUSH_NULL, EatLiteral()); PushType(Null); }
			else if (Match(Token.LParen)) { Expression(); Eat(Token.RParen); }
			else if (forced) throw new UnexpectedTokenException(Current(), Token.Int, Token.Float, Token.String, Token.KFalse, Token.KTrue, Token.KNull, Token.LParen);
		}

		public static string Recant() {
			return "";
		}
	}
}
