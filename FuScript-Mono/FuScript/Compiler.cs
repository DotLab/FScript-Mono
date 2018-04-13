namespace FuScript {
	public class UnexpectedTokenException : System.Exception {
		public UnexpectedTokenException(byte t, params byte[] es) : base("Unexpected token " + Token.Recant(t) + ", expecting one of " + Token.Recant(es)) { }
	}

	public class TypeStackUnderflowException : System.Exception {
		public TypeStackUnderflowException() : base("TypeStack underflow") { }
		public TypeStackUnderflowException(byte t) : base("TypeStack underflow, expecting " + Compiler.RecantType(t)) { }
	}

	public class UnexpectedTypeException : System.Exception {
		public UnexpectedTypeException(byte t, params byte[] es) : base("Unexpected type " + Compiler.RecantType(t) + ", expecting one of " + Compiler.RecantType(es)) { }
	}

	public static class Compiler {
		static readonly System.Collections.Generic.Dictionary<ushort, TypePrediction> varTypeDict = new System.Collections.Generic.Dictionary<ushort, TypePrediction>();
		static readonly string[] typeDict = new string[256];
		static byte typeDictCounter;

		static Compiler () {
			typeDict[NIL] = "nil";
			typeDict[BOO] = "boo";
			typeDict[INT] = "int";
			typeDict[FLO] = "flo";
			typeDict[STR] = "str";

			typeDictCounter = 5;
		}

		sealed class TypePrediction {
			public byte type;
			public bool isPoly;

			public System.Collections.Generic.List<byte> types;

			public TypePrediction(params byte[] types) {
				if (types == null || types.Length < 1) {
					isPoly = true;
					this.types = new System.Collections.Generic.List<byte>();
					for (byte i = 0; i < typeDictCounter; i++) this.types.Add(i);
				} else if (types.Length == 1) {
					type = types[0];
					isPoly = false;
				} else {
					isPoly = true;
					this.types = new System.Collections.Generic.List<byte>(types);
				}
			}

			public void Instantiate(byte t) {
				if (!isPoly) throw new System.Exception("Wat?");
				if (!types.Contains(t)) throw new System.Exception("Cannot instatiate poly type");

				type = t;
				isPoly = false;
				types = null;
			}

			public void Specify(byte[] t) {
				if (!isPoly || t == null || t.Length < 1) throw new System.Exception("Wat?");

				for (int i = 0; i < t.Length; i++) {
					if (types.Contains(t[i])) types.Remove(t[i]);
				}

				if (types.Count == 0) {
					throw new System.Exception("Cannot specify poly type");
				} else if (types.Count == 1) {
					type = types[0];
					isPoly = false;
					types = null;
				}
			}

			public override string ToString() {
				if (isPoly) return RecantType(types.ToArray());
				return RecantType(type);
			}
		}

		static string RecantVarTypeDict() {
			var sb = new System.Text.StringBuilder();
			sb.Clear();
			sb.Append("    { ");
			foreach (var pair in varTypeDict) {
				sb.AppendFormat("{0}: {1}, ", pair.Key, pair.Value);
			}
			sb.Append("}\n");
			return sb.ToString();
		}

		#region TypeStack

		const byte NIL = 0, BOO = 1, INT = 2, FLO = 3, STR = 4;

		static readonly TypePrediction[] typeStack = new TypePrediction[256];
		static int typeStackPointer = -1;

		public static string RecantType(byte t) {
			switch (t) {
				case NIL: return "NIL";
				case BOO: return "BOO";
				case INT: return "INT";
				case FLO: return "FLO";
				case STR: return "STR";
				default: throw new System.Exception("Unrecognized type #" + t);
			}
		}

		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string RecantType(byte[] ts) {
			if (ts == null || ts.Length < 1) return "[ ]";
			if (ts.Length == 1) return "[ " + RecantType(ts[0]) + " ]";

			sb.Clear();
			sb.Append("[ ");
			int length = ts.Length;
			for (int i = 0; i < length; ++i) {
				sb.Append(RecantType(ts[i]));
				if (i + 1 < length) sb.Append(", ");
				else sb.Append(" ");
			}
			sb.Append("]");
			return sb.ToString();
		}

		static byte CurrentMonoType() {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException();
			if (typeStack[typeStackPointer].isPoly) throw new System.Exception("Cannot instantiate poly type");
			return typeStack[typeStackPointer].type;
		}

		static void PushMonoType(byte t) {
			typeStack[++typeStackPointer] = new TypePrediction(t);
			Log(RecantTypeStack());
		}

		static void PushType(TypePrediction p) {
			typeStack[++typeStackPointer] = p;
			Log(RecantTypeStack());
			Log(RecantVarTypeDict());
		}

		static bool MatchMonoType(byte t) {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException(t);
			if (typeStack[typeStackPointer].isPoly) { typeStack[typeStackPointer].Instantiate(t); Log(RecantVarTypeDict()); }
			if (typeStack[typeStackPointer].type == t) { typeStackPointer -= 1; Log(RecantTypeStack()); return true; }
			return false;
		}

		static void PopMonoType(byte t) {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException(t);
			if (typeStack[typeStackPointer].isPoly) { typeStack[typeStackPointer].Instantiate(t); Log(RecantVarTypeDict()); }
			if (typeStack[typeStackPointer].type == t) { typeStackPointer -= 1; Log(RecantTypeStack()); return; }
			throw new UnexpectedTypeException(typeStack[typeStackPointer].type, t);
		}

		static byte EatMonoType() {
			if (typeStackPointer < 0) throw new TypeStackUnderflowException();
			if (typeStack[typeStackPointer].isPoly) throw new System.Exception("Cannot instantiate poly type");
			byte res = typeStack[typeStackPointer].type; typeStackPointer -= 1; Log(RecantTypeStack()); return res;
		}

		public static string RecantTypeStack() {
			var sb = new System.Text.StringBuilder();
			sb.Clear();
			sb.Append("    [ ");
			for (int i = 0; i <= typeStackPointer; i++) {
				sb.Append(typeStack[i]);
				if (i + 1 <= typeStackPointer) sb.Append(", ");
				else sb.Append(" ");
			}
			sb.Append("]\n");
			return sb.ToString();
		}

		#endregion

		static ushort pos, length;
		static ushort stmtCount;

		#region Helper

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
			if (Current() == t) { Log(string.Format("{0} {1}\n", pos, Token.Recant(Current()))); pos += 1; return true; }
			return false;
		}

		static void Eat(byte t) {
			if (Current() == t) { Log(string.Format("{0} {1}\n", pos, Token.Recant(Current()))); pos++; return; }
			throw new UnexpectedTokenException(Current(), t);
		}

		static ushort EatLiteral() {
			return (ushort)(Lexer.tokens[pos++] | Lexer.tokens[pos++] << 8);
		}

		public static void Reset() {
			pos = 0; stmtCount = 0; typeStackPointer = -1;
		}

		public static void Compile() {
			length = Lexer.tokenCount;
			Statement();
		}

		#endregion

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
			//else if (Peek(Token.KIf)) IfStmt();
			//else if (Peek(Token.KWhile)) WhileStmt();
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
			Eat(Token.KVar); Eat(Token.Id); ushort id = EatLiteral();
			if (varTypeDict.ContainsKey(id)) throw new System.Exception("Variable is already defined");

			if (Match(Token.Equal)) {
				Expression();
				varTypeDict.Add(id, new TypePrediction(EatMonoType()));
			} else {
				varTypeDict.Add(id, new TypePrediction());
			}

			Eat(Token.Semi);
		}

		/**
		 * exprStmt -> expression ";"
		 */
		static void ExprStmt() {
			Expression();
			Eat(Token.Semi);
			EatMonoType();
		}

		#region Expression

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
			ushort start = pos;
			LogicOr();
			typeStackPointer -= 1;

			if (Match(Token.Equal)) {
				Expression();
				byte type = EatMonoType();

				ushort end = pos;
				pos = start;
				LogicOr();
				PopMonoType(type);
				PushMonoType(type);

				pos = end;
			} else {
				pos = start;
				LogicOr();
			}
		}

		/**
		 * logic_or -> logic_and ("||" logic_and)*
		 */
		static void LogicOr() {
			LogicAnd();
			while (Match(Token.OrOr)) {
				PopMonoType(BOO); LogicAnd(); PopMonoType(BOO); PushMonoType(BOO);
			}
		}

		/**
		 * logic_and -> equality ("&&" equality)*
		 */
		static void LogicAnd() {
			Comparision();
			while (Match(Token.AndAnd)) {
				PopMonoType(BOO); Comparision(); PopMonoType(BOO); PushMonoType(BOO);
			}
		}

		/**
		 * comparison -> bitwise ((">" | ">=" | "<" | "<=") bitwise)*
		 */
		static void Comparision() {
			Addition();

			while (true) {
				if (Match(Token.LAngle)) {
					TrySpecify(INT, FLO);
					if      (MatchMonoType(INT)) { Addition(); PopMonoType(INT); PushMonoType(BOO); }
					else throw new UnexpectedTypeException(CurrentMonoType(), FLO);
				} else if (Match(Token.RAngle)) {
					if      (MatchMonoType(INT)) { Addition(); PopMonoType(INT); PushMonoType(BOO); }
					else throw new UnexpectedTypeException(CurrentMonoType(), FLO);
				} else break;
			}
		}

		/**
		 * addition -> multiplication (("-" | "+") multiplication)*
		 */
		static void Addition() {
			Primary();

			while (true) {
				if (Match(Token.Minus)) {
					if (MatchMonoType(INT)) { Primary(); PopMonoType(INT); PushMonoType(INT); }
					else throw new UnexpectedTypeException(CurrentMonoType(), INT);
				} else if (Match(Token.Plus)) {
					if (MatchMonoType(INT)) { Primary(); PopMonoType(INT); PushMonoType(INT); }
					else throw new UnexpectedTypeException(CurrentMonoType(), INT);
				} else break;
			}
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static void Primary() {
			if      (Match(Token.Int))     { var id = EatLiteral(); PushMonoType(INT); } 
			else if (Match(Token.KFalse)) { PushMonoType(BOO); } 
			else if (Match(Token.KTrue))  { PushMonoType(BOO); } 
			else if (Match(Token.LParen)) { Expression(); Eat(Token.RParen); } 
			else if (Match(Token.Id))    { 
				ushort id = EatLiteral(); 
				if (!varTypeDict.ContainsKey(id)) throw new System.Exception("Variable is never defined");
				PushType(varTypeDict[id]); 
			} 
			else throw new UnexpectedTokenException(Current(), Token.Int, Token.KFalse, Token.KTrue);
		}

		#endregion
	}
}
