using ArgList = System.Collections.Generic.List<ushort>;

namespace FuScript.Fifth {
	public class UnexpectedTokenException : System.Exception {
		public UnexpectedTokenException(ushort t, params ushort[] es) : base("Unexpected token '" + Token.Recant(t) + "', expecting one of " + Token.Recant(es)) { }
	}

	public static class Parser {
		static ushort pos, length;

		public static readonly ushort[] nodes = new ushort[1024];
		public static readonly ushort[] marks = new ushort[1024];
		public static ushort nodeCount, mark, lastMark;

		static void Log(string str) {
			System.Console.Write(str);
		}

		static ushort Current() {
			return pos < length ? Lexer.tokens[pos] : Token.Eof;
		}

		static ushort Find(ushort n) {
			return nodes[n];
		}

		static bool Peek(ushort t) {
			return Current() == t;
		}

		static bool Match(ushort t) {
			if (Current() == t) {
				if (lastMark != mark) { lastMark = mark; Log(string.Format("\n{0}: ", mark)); }
				Log(Token.Recant(Current()) + " "); 
				pos += 1; return true; }
			return false;
		}

		static void Eat(ushort t) {
			if (Current() == t) {
				if (lastMark != mark) { lastMark = mark; Log(string.Format("\n{0}: ", mark)); }
				Log(Token.Recant(Current()) + " "); 
				pos += 1; return; }
			throw new UnexpectedTokenException(Current(), t);
		}

		static ushort EatLiteral() {
			return Lexer.tokens[pos++];
		}

		static ushort Emit(ushort node) {
			marks[nodeCount] = mark;
			nodes[nodeCount++] = node;
			return (ushort)(nodeCount - 1);
		}

		static ushort Emit(ushort node, ushort arg) {
			marks[nodeCount] = mark;
			nodes[nodeCount++] = node;
			nodes[nodeCount++] = arg;
			return (ushort)(nodeCount - 2);
		}

		static ushort Emit(ushort node, ushort arg1, ushort arg2) {
			marks[nodeCount] = mark;
			nodes[nodeCount++] = node;
			nodes[nodeCount++] = arg1;
			nodes[nodeCount++] = arg2;
			return (ushort)(nodeCount - 3);
		}

		static ushort Emit(ushort node, ushort arg1, ushort arg2, ushort arg3) {
			marks[nodeCount] = mark;
			nodes[nodeCount++] = node;
			nodes[nodeCount++] = arg1;
			nodes[nodeCount++] = arg2;
			nodes[nodeCount++] = arg3;
			return (ushort)(nodeCount - 4);
		}

		static ushort Emit(ushort node, ushort[] args) {
			marks[nodeCount] = mark;
			nodes[nodeCount++] = node;
			nodes[nodeCount++] = (ushort)args.Length;
			for (int i = 0; i < args.Length; ++i) nodes[nodeCount++] = args[i];
			return (ushort)(nodeCount - 2 - args.Length);
		}

		public static void Reset() {
			pos = 0; nodeCount = 0; mark = 0; lastMark = 0;
		}

		public static ushort Parse() {
			length = Lexer.tokenCount;

			return Program();
		}

		/**
		 * program -> ( statement )* EOF
		 */
		public static ushort Program() {
			var args = new ArgList();
			while (!Match(Token.Eof)) args.Add(Statement());
			return Emit(Node.Program, args.ToArray());
		}

		/**
		 * statement -> block | varDecl | funcDecl
		 *            | ifStmt
		 *            | whileStmt | forStmt | doStmt
		 *            | "break" ";" | "continue" ";" | "return" expression? ";"
		 *            | exprStmt
		 */
		static ushort Statement() {
			mark += 1;

			if (Peek(Token.LCurly))    return Block();
			if (Peek(Token.KVar))      return VarDecl();
			if (Peek(Token.KFunction)) return FuncDecl();

			if (Peek(Token.KIf)) return IfStmt();

			if (Peek(Token.KWhile)) return WhileStmt();
			if (Peek(Token.KDo))    return DoStmt();
			if (Peek(Token.KFor))   return ForStmt();

			if (Match(Token.KBreak))    { Eat(Token.Semi); return Emit(Node.Break); }
			if (Match(Token.KContinue)) { Eat(Token.Semi); return Emit(Node.Continue); }

			if (Match(Token.KReturn)) {
				if (Match(Token.Semi)) return Emit(Node.Return, Emit(Node.Null));
				ushort expr = Expression();
				Eat(Token.Semi);
				return Emit(Node.Return, expr);
			}

			return ExprStmt();
		}

		/**
		 * block -> "{" statement* "}"
		 */
		static ushort Block() {
			var argList = new ArgList();
			Eat(Token.LCurly);
			while (!Match(Token.RCurly)) argList.Add(Statement());
			return Emit(Node.Block, argList.ToArray());
		}

		/**
		 * varDecl -> "var" ID ( "=" expression )? ( "," ID ( "=" expression )? )* ";"
		 */
		static ushort VarDecl() {
			Eat(Token.KVar);
			var argList = new ArgList();
			do {
				Eat(Token.Id);
				argList.Add(EatLiteral());
				if (Match(Token.Equal)) argList.Add(Expression());
				else                    argList.Add(Emit(Node.Null));
			} while (Match(Token.Comma));
			Eat(Token.Semi);
			return Emit(Node.VarDecl, argList.ToArray());
		}

		/**
		 * funcDecl -> "function" ID "(" ( ID ( "," ID )* )? ")" block
		 */
		static ushort FuncDecl() {
			Eat(Token.KFunction);
			Eat(Token.Id);
			ushort id = EatLiteral();
			ushort func = FuncExpr();
			return Emit(Node.VarDecl, new [] { id, func });
		}

		/**
		 * ifStmt -> "if" "(" expression ")" statement ("else" statement)?
		 */
		static ushort IfStmt() {
			Eat(Token.KIf);
			Eat(Token.LParen);
			ushort cond = Expression();
			Eat(Token.RParen);
			ushort then = Statement();
			if (Match(Token.KElse)) return Emit(Node.IfStmt, cond, then, Statement());
			return Emit(Node.IfStmt, cond, then, Emit(Node.NoOp));
		}

		/**
		 * whileStmt -> "while" "(" expression ")" statement
		 */
		static ushort WhileStmt() {
			Eat(Token.KWhile);
			Eat(Token.LParen);
			ushort cond = Expression();
			Eat(Token.RParen);
			return Emit(Node.WhileStmt, cond, Statement());
		}

		/**
		 * doStmt -> "do" statement "while" "(" expression ")" ";"
		 */
		static ushort DoStmt() {
			Eat(Token.KDo);
			ushort stmt = Statement();
			Eat(Token.KWhile);
			Eat(Token.LParen);
			ushort cond = Expression();
			Eat(Token.RParen);
			Eat(Token.Semi);
			return Emit(Node.Block, new [] { stmt, Emit(Node.WhileStmt, cond, stmt) });
		}

		/**
		 * forStmt -> "for" "(" ( ";" | varDecl | ExprStmt ) expression? ";" expression? ")" statement 
		 */
		static ushort ForStmt() {
			Eat(Token.KFor);
			Eat(Token.LParen);
			ushort init = 0;
			if     (Match(Token.Semi)) init = Emit(Node.NoOp);
			else if (Peek(Token.KVar)) init = VarDecl();
			else init = ExprStmt();

			ushort cond;
			if (Peek(Token.Semi)) cond = Emit(Node.True);
			else cond = Expression();
			Eat(Token.Semi);

			ushort inc;
			if (Peek(Token.RParen)) inc = Emit(Node.NoOp);
			else inc = Expression();
			Eat(Token.RParen);

			ushort stmt = Statement();

			var stmtList = new ArgList();
			if (nodes[stmt] == Node.Block) {
				for (int i = 0; i < nodes[stmt + 1]; ++i) stmtList.Add(nodes[stmt + 1 + i]);
			} else {
				stmtList.Add(stmt);
			}
			stmtList.Add(inc);
			stmt = Emit(Node.Block, stmtList.ToArray());

			return Emit(Node.Block, new [] { init, Emit(Node.WhileStmt, cond, stmt) });
		}

		/**
		 * exprStmt -> expression ";"
		 */
		static ushort ExprStmt() {
			ushort stmt = Expression();
			Eat(Token.Semi);
			return Emit(Node.ExprStmt, stmt);
		}

		/**
		 * expression -> objExpr
		 *             | arrExpr
		 *             | funcExpr
		 *             | arithmetic
		 */
		static ushort Expression() {
			if (Match(Token.LCurly))    return ObjExpr();
			if (Match(Token.LSquare))   return ArrExpr();
			if (Match(Token.KFunction)) return FuncExpr();
			return Arithmetic();
		}

		/**
		 * objExpr -> "{" ( ID ":" expression ( "," ID ":" expression )* )? "}"
		 */
		static ushort ObjExpr() {
			//Eat(Token.LCurly);
			var argList = new ArgList();
			if (!Match(Token.RCurly)) {
				do {
					Eat(Token.Id);
					argList.Add(EatLiteral());
					Eat(Token.Colon);
					argList.Add(Expression());
				} while (Match(Token.Comma));
				Eat(Token.RCurly);
			}
			return Emit(Node.Object, argList.ToArray());
		}

		/**
		 * funcExpr -> "function" "(" ( ID ( "," ID )* )? ")" block
		 */
		static ushort FuncExpr() {
			//Eat(Token.KFunction);
			var argList = new ArgList{ 0 };
			Eat(Token.LParen);
			if (!Peek(Token.RParen)) {
				do {
					Eat(Token.Id);
					argList.Add(EatLiteral());
				} while (Match(Token.Comma));
			}
			Eat(Token.RParen);

			argList[0] = Block();
			return Emit(Node.Function, argList.ToArray());
		}

		/**
		 * arrExpr -> "[" ( expression ( "," expression )* )? "]"
		 */
		static ushort ArrExpr() {
			//Eat(Token.LSquare);
			var argList = new ArgList();
			if (!Match(Token.RSquare)) {
				do argList.Add(Expression()); while (Match(Token.Comma));
				Eat(Token.RSquare);
			}
			return Emit(Node.Array, argList.ToArray());
		}

		#region Arithmetic

		/**
		 * arithmetic -> comma
		 */
		static ushort Arithmetic() {
			return Comma();
		}

		/**
		 * comma -> assignment ( "," assignment ) *
		 */
		static ushort Comma() {
			var argList = new ArgList();
			do argList.Add(Assignment()); while (Match(Token.Comma));
			if (argList.Count == 1) return argList[0];
			return Emit(Node.Comma, argList.ToArray());
		}

		/**
		 * assignment -> call () expression
		 *             | conditional
		 */
		static ushort Assignment() {
			ushort assign = Conditional();
			if (Find(assign) == Node.Var || Find(assign) == Node.Member || Find(assign) == Node.Subscript) {
				if (Match(Token.Equal)) return Emit(Node.Assign, assign, Expression());
				
				if (Match(Token.ModEqual))   return Emit(Node.AssignMod, assign, Expression());
				if (Match(Token.StarEqual))  return Emit(Node.AssignMul, assign, Expression());
				if (Match(Token.SlashEqual)) return Emit(Node.AssignDiv, assign, Expression());

				if (Match(Token.PlusEqual))  return Emit(Node.AssignAdd, assign, Expression());
				if (Match(Token.MinusEqual)) return Emit(Node.AssignSub, assign, Expression());
				
				if (Match(Token.OrEqual))    return Emit(Node.AssignBitOr, assign, Expression());
				if (Match(Token.AndEqual))   return Emit(Node.AssignBitAnd, assign, Expression());
				if (Match(Token.CaretEqual)) return Emit(Node.AssignBitXor, assign, Expression());
				if (Match(Token.LAngleAngleEqual)) return Emit(Node.AssignShiftL, assign, Expression());
				if (Match(Token.RAngleAngleEqual)) return Emit(Node.AssignShiftR, assign, Expression());
			}
			return assign;
		}


		/**
		 * conditional -> logicalOr "?" expression ":" expression
		 */
		static ushort Conditional() {
			ushort cond = LogicalOr();
			if (Match(Token.Quest)) {
				ushort then = Expression();
				Eat(Token.Colon);
				return Emit(Node.Conditional, cond, then, Expression());
			}
			return cond;
		}

		/**
		 * logicalOr -> logicalAnd ( ( "||" ) logicalAnd )*
		 */
		static ushort LogicalOr() {
			ushort lor = LogicalAnd();
			while (true) {
				if (Match(Token.OrOr)) lor = Emit(Node.Or, lor, LogicalAnd());
				else break;
			}
			return lor;
		}

		/**
		 * logicalAnd -> equality ( ( "&&" ) equality )*
		 */
		static ushort LogicalAnd() {
			ushort land = Equality();
			while (true) {
				if (Match(Token.AndAnd)) land = Emit(Node.And, land, Equality());
				else break;
			}
			return land;
		}

		/**
		 * equality -> relational ( ( "!=" | "==" ) relational )*
		 */
		static ushort Equality() {
			ushort eq = Relational();
			while (true) {
				if      (Match(Token.EqualEqual)) eq = Emit(Node.Equal, eq, Relational());
				else if (Match(Token.BangEqual))  eq = Emit(Node.NotEqual, eq, Relational());
				else break;
			}
			return eq;
		}

		/**
		 * relational -> bitwise ( ( "<" | ">" | "<=" | ">=" ) bitwise )*
		 */
		static ushort Relational() {
			ushort rel = Bitwise();
			while (true) {
				if      (Match(Token.LAngle)) rel = Emit(Node.LessThan, rel, Bitwise());
				else if (Match(Token.RAngle)) rel = Emit(Node.GreaterThan, rel, Bitwise());
				else if (Match(Token.LAngleEqual)) rel = Emit(Node.LessEqual, rel, Bitwise());
				else if (Match(Token.RAngleEqual)) rel = Emit(Node.GreaterEqual, rel, Bitwise());
				else break;
			}
			return rel;
		}

		/**
		 * bitwise -> additive ( ( "|" | "&" | "^" | "<<" | ">>" ) multiplicative )*
		 */
		static ushort Bitwise() {
			ushort bit = Additive();
			while (true) {
				if      (Match(Token.Or))    bit = Emit(Node.BitOr,  bit, Additive());
				else if (Match(Token.And))   bit = Emit(Node.BitAnd, bit, Additive());
				else if (Match(Token.Caret)) bit = Emit(Node.BitXor, bit, Additive());
				else if (Match(Token.LAngleAngle)) bit = Emit(Node.ShiftL,  bit, Additive());
				else if (Match(Token.RAngleAngle)) bit = Emit(Node.ShiftR, bit, Additive());
				else break;
			}
			return bit;
		}

		/**
		 * additive -> multiplicative ( ( "+" | "-" ) multiplicative )*
		 */
		static ushort Additive() {
			ushort addi = Multiplicative();
			while (true) {
				if      (Match(Token.Plus))  addi = Emit(Node.Add, addi, Multiplicative());
				else if (Match(Token.Minus)) addi = Emit(Node.Sub, addi, Multiplicative());
				else break;
			}
			return addi;
		}

		/**
		 * multiplicative -> unary ( ( "%" | "*" | "/" ) unary )*
		 */
		static ushort Multiplicative() {
			ushort mult = Unary();
			while (true) {
				if      (Match(Token.Mod))   mult = Emit(Node.Mod, mult, Unary());
				else if (Match(Token.Star))  mult = Emit(Node.Mul, mult, Unary());
				else if (Match(Token.Slash)) mult = Emit(Node.Div, mult, Unary());
				else break;
			}
			return mult;
		}

		/**
		 * unary -> ( "!" | "-" | "~" ) unary
		 *        | call
		 */
		static ushort Unary() {
			if (Match(Token.Bang))  return Emit(Node.Not, Unary());
			if (Match(Token.Minus)) return Emit(Node.Neg, Unary());
			if (Match(Token.Tilde)) return Emit(Node.BitNot, Unary());
			return Call();
		}

		/**
		 * call -> postfix ( "." ID | "(" ( expr ( "," expr )* )? ")" )*
		 */
		static ushort Call() {
			ushort call = Postfix();

			while (true) {
				if (Match(Token.Dot)) {  // Member
					Eat(Token.Id);
					call = Emit(Node.Member, call, EatLiteral());
				} else if (Match(Token.LParen)) {  // Call
					var argList = new System.Collections.Generic.List<ushort>{ call };
					if (!Match(Token.RParen)) {
						do argList.Add(Expression()); while (Match(Token.Comma));
						Eat(Token.RParen);
					}
					call = Emit(Node.Call, argList.ToArray());
				} else if (Match(Token.LSquare)) {  // Subscript
					call = Emit(Node.Subscript, call, Expression());
					Eat(Token.RSquare);
				} else break;
			}

			return call;
		}

		/**
		 * postfix -> ( "++" | "--" )* postfix
		 *          | primary ( "++" | "--" )*
		 */
		static ushort Postfix() {
			if (Match(Token.PlusPlus))   return Emit(Node.PreInc, Postfix());
			if (Match(Token.MinusMinus)) return Emit(Node.PreDec, Postfix());

			ushort postfix = Primary();
			while (true) {
				if      (Match(Token.PlusPlus))   postfix = Emit(Node.PostInc, postfix);
				else if (Match(Token.MinusMinus)) postfix = Emit(Node.PostDec, postfix);
				else break;
			}

			return postfix;
		}

		/**
		 * primary -> "null" | "true" | "false"
		 *          | ID | NUMBER | STRING
		 *          | "(" expr ")"
		 */
		static ushort Primary() {
			if (Match(Token.KNull))  return Emit(Node.Null);
			if (Match(Token.KTrue))  return Emit(Node.True);
			if (Match(Token.KFalse)) return Emit(Node.False);
			if (Match(Token.Id))     return Emit(Node.Var, EatLiteral());
			if (Match(Token.Int))    return Emit(Node.Int, EatLiteral());
			if (Match(Token.Float))  return Emit(Node.Float, EatLiteral());
			if (Match(Token.String)) return Emit(Node.String, EatLiteral());
			if (Match(Token.LParen)) { ushort primary = Expression(); Eat(Token.RParen); return primary; }
			throw new UnexpectedTokenException(Current(), Token.KNull, Token.KTrue, Token.KFalse, Token.Id, Token.Int, Token.Float, Token.String, Token.LParen);
		}

		#endregion

		public static string Recant() {
			return "\n";
		}
	}
}
