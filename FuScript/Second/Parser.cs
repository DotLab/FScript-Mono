using System;
using System.Collections.Generic;

namespace FuScript.Second {
	public sealed class Node {
		public const byte BinaryOp = 0, UnaryOp = 1, Literal = 2, Statement = 3, Program = 4;
		public const byte VarDecl = 5, Variable = 6, Assignment = 7, Block = 8, Call = 9, FuncDecl = 10, Return = 11;

		public readonly byte type, token;
		public readonly Node child1, child2, child3, child4;
		public readonly Node[] children;

		public readonly bool boolLiteral;
		public readonly float numberLiteral;
		public readonly string stringLiteral;

		public Node(byte type, byte token) {
			this.type = type;
			this.token = token;
		}

		public Node(byte type, Node children) {
			this.type = type;
			this.child1 = children;
		}

		public Node(byte type, Node[] children) {
			this.type = type;
			this.children = children;
		}

		public Node(byte type, Node child1, Node[] children) {
			this.type = type;
			this.child1 = child1;
			this.children = children;
		}

		public Node(byte type, byte token, Node node1) {
			this.type = type;
			this.token = token;
			this.child1 = node1;
		}

		public Node(byte type, byte token, Node node1, Node node2) {
			this.type = type;
			this.token = token;
			this.child1 = node1;
			this.child2 = node2;
		}

		public Node(byte type, byte token, Node node1, Node node2, Node node3) {
			this.type = type;
			this.token = token;
			this.child1 = node1;
			this.child2 = node2;
			this.child3 = node3;
		}

		public Node(byte type, byte token, Node node1, Node node2, Node node3, Node node4) {
			this.type = type;
			this.token = token;
			this.child1 = node1;
			this.child2 = node2;
			this.child3 = node3;
			this.child4 = node4;
		}

		public Node(byte type, byte token, bool boolLiteral) {
			this.type = type;
			this.token = token;
			this.boolLiteral = boolLiteral;
		}

		public Node(byte type, byte token, float numberLiteral) {
			this.type = type;
			this.token = token;
			this.numberLiteral = numberLiteral;
		}

		public Node(byte type, byte token, string stringLiteral) {
			this.type = type;
			this.token = token;
			this.stringLiteral = stringLiteral;
		}

		public Node(byte type, byte token, string stringLiteral, Node child1) {
			this.type = type;
			this.token = token;
			this.stringLiteral = stringLiteral;
			this.child1 = child1;
		}

		public Node(byte type, string stringLiteral) {
			this.type = type;
			this.stringLiteral = stringLiteral;
		}

		public Node(byte type, string stringLiteral, Node node1) {
			this.type = type;
			this.stringLiteral = stringLiteral;
			this.child1 = node1;
		}

		public Node(byte type, string stringLiteral, Node node1, Node[] children) {
			this.type = type;
			this.stringLiteral = stringLiteral;
			this.child1 = node1;
			this.children = children;
		}

		public override string ToString() {
			switch (type) {
			case Node.BinaryOp:
				switch (token) {
				case Token.Minus:       return "(" + child1 + " - "  + child2 + ")";
				case Token.Plus:        return "(" + child1 + " + "  + child2 + ")";
				case Token.Slash:       return "(" + child1 + " / "  + child2 + ")";
				case Token.Star:        return "(" + child1 + " * "  + child2 + ")";
				case Token.BangEqual:   return "(" + child1 + " != " + child2 + ")";
				case Token.EqualEqual:  return "(" + child1 + " == " + child2 + ")";
				case Token.LAngle:      return "(" + child1 + " < "  + child2 + ")";
				case Token.LAngleEqual: return "(" + child1 + " <= " + child2 + ")";
				case Token.LAngleAngle: return "(" + child1 + " << " + child2 + ")";
				case Token.RAngle:      return "(" + child1 + " > "  + child2 + ")";
				case Token.RAngleEqual: return "(" + child1 + " >= " + child2 + ")";
				case Token.RAngleAngle: return "(" + child1 + " >> " + child2 + ")";
				case Token.And:         return "(" + child1 + " & "  + child2 + ")";
				case Token.AndAnd:      return "(" + child1 + " && " + child2 + ")";
				case Token.Or:          return "(" + child1 + " | "  + child2 + ")";
				case Token.OrOr:        return "(" + child1 + " || " + child2 + ")";
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.UnaryOp:
				switch (token) {
				case Token.Minus:       return "-" + child1;
				case Token.Bang:        return "!" + child1;
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Literal:
				switch (token) {
				case Token.Id:          return stringLiteral;
				case Token.String:      return "'" + stringLiteral + "'";
				case Token.Number:      return numberLiteral.ToString();
				case Token.KFalse:      return "false";
				case Token.KTrue:       return "true";
				case Token.KNull:       return "null";
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Statement:
				switch (token) {
				case Token.KPrint:      return "print " + child1 + "; ";
				case Token.KIf:         return "if (" + child1 + ") " + (child3 == null ? child2.ToString() : child2 + "else " + child3);
				case Token.KWhile:      return "while (" + child1 + ") " + child2;
				case Token.KFor:        return "for (" + (child1 == null ? "; " : child1.ToString()) + (child2 == null ? "; " : child2 + "; ") + (child3 == null ? ") " : child3 + ") ") + child4;
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Program:
				int length = children.Length;
				var sb = new System.Text.StringBuilder();
//				sb.Append("< ");
				for (int i = 0; i < length; i++) sb.Append(children[i]);
//				sb.Append(">");
				return sb.ToString();
			case Node.VarDecl:
				return child1 == null ? "var " + stringLiteral : "var " + stringLiteral + " = " + child1 + "; ";
			case Node.Variable:
				return stringLiteral;
			case Node.Assignment:
				return stringLiteral + " = " + child1;
			case Node.Block:
				length = children.Length;
				sb = new System.Text.StringBuilder();
				sb.Append("{ ");
				for (int i = 0; i < length; i++) sb.Append(children[i]);
				sb.Append("} ");
				return sb.ToString();
			case Node.Call:
				sb = new System.Text.StringBuilder();
				sb.Append(child1.ToString());
				sb.Append("(");
				length = children.Length;
				for (int i = 0; i < length; i++) {
					sb.Append(children[i]);
					if (i < length - 1) sb.Append(", ");
				}	
				sb.Append(")");
				return sb.ToString();
			case Node.FuncDecl:
				sb = new System.Text.StringBuilder();
				sb.Append("function ");
				sb.Append(stringLiteral);
				sb.Append(" (");
				length = children.Length;
				for (int i = 0; i < length; i++) {
					sb.Append(children[i].stringLiteral);
					if (i + 1 < length) sb.Append(", ");
				}
				sb.Append(") ");
				sb.Append(child1);
				return sb.ToString();
			case Node.Return:
				return "return " + child1 + "; ";
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}

	public static class Parser {
		static Token[] _tokens;
		static int _pos, _length;

		public static void Set(Token[] tokens) {
			_tokens = tokens;
			_pos = 0;
			_length = tokens.Length;
		}

		static bool Peek(byte type) {
			return _tokens[_pos].type == type;
		}

		static bool Peek(byte type1, byte type2) {
			int type = _tokens[_pos].type;
			return type == type1 || type == type2;
		}

		static bool Peek(byte type1, byte type2, byte type3, byte type4) {
			int type = _tokens[_pos].type;
			return type == type1 || type == type2 || type == type3 || type == type4;
		}

		static bool Match(byte type) {
			if (_tokens[_pos].type == type) { _pos += 1; return true; }
			return false;
		}

		static bool Match(byte type1, byte type2) {
			int type = _tokens[_pos].type;
			if (type == type1 || type == type2) { _pos += 1; return true; }
			return false;
		}

		static Token Eat(byte type) {
			if (_tokens[_pos].type == type) return _tokens[_pos++];
			throw new Exception("Unexpected token " + _tokens[_pos] + ", expecting " + new Token(type));
		}
			
		/**
		 * program -> declaration* EOF
		 */
		public static Node Program() {
			var list = new List<Node>();
			while (!Match(Token.Eof)) list.Add(Declaration());
			return new Node(Node.Program, list.ToArray());
		}

		/**
		 * declaration -> funcDecl
		 *              | varDecl
		 *              | statement
		 */
		static Node Declaration() {
			if (Peek(Token.KVar)) return VarDecl();
			if (Peek(Token.KFunc)) return FuncDecl();
			return Statement();
		}

		/**
		 * funcDecl -> "function" IDENTIFIER "(" (IDENTIFIER ("," IDENTIFIER)*)? ")" block
		 */
		static Node FuncDecl() {
			Eat(Token.KFunc);
			var id = Eat(Token.Id).stringLiteral;
			Eat(Token.LParen);
			var list = new List<Node>();
			if (!Peek(Token.RParen)) {
				do {
					list.Add(new Node(Node.Variable, Eat(Token.Id).stringLiteral));
				} while (Match(Token.Comma));
			}
			Eat(Token.RParen);
			return new Node(Node.FuncDecl, id, Block(), list.ToArray());
		}

		/**
		 * varDecl -> "var" IDENTIFIER ("=" expression)? ";"
		 */
		static Node VarDecl() {
			var token = Eat(Token.KVar);
			string id = Eat(Token.Id).stringLiteral;
			var node = Match(Token.Equal) ? new Node(Node.VarDecl, id, Expression()) : new Node(Node.VarDecl, id);
			Eat(Token.Semi);
			return node;
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
		public static Node Statement() {
			if (Peek(Token.LCurly)) return Block();
			if (Peek(Token.KIf)) return IfStmt();
			if (Peek(Token.KWhile)) return WhileStmt(); 
			if (Peek(Token.KFor)) return ForStmt(); 
			if (Peek(Token.KReturn)) return ReturnStmt();
			if (Peek(Token.KPrint)) return PrintStmt();
			return ExprStmt();
		}

		/**
		 * block -> "{" declaration* "}"
		 */
		static Node Block() {
			var list = new List<Node>();
			Eat(Token.LCurly);
			while (!Match(Token.RCurly)) list.Add(Declaration());
			return new Node(Node.Block, list.ToArray());
		}

		/**
		 * ifStmt -> "if" "(" expression ")" statement ("else" statement)?
		 */
		static Node IfStmt() {
			Eat(Token.KIf);
			Eat(Token.LParen);
			var cond = Expression();
			Eat(Token.RParen);
			var then = Statement();
			if (Match(Token.KElse)) {
				var el = Statement();
				return new Node(Node.Statement, Token.KIf, cond, then, el);
			}
			return new Node(Node.Statement, Token.KIf, cond, then);
		}

		/**
		 * whileStmt -> "while" "(" expression ")" statement
		 */
		static Node WhileStmt() {
			Eat(Token.KWhile);
			Eat(Token.LParen);
			var cond = Expression();
			Eat(Token.RParen);
			return new Node(Node.Statement, Token.KWhile, cond, Statement());
		}

		/**
		 * forStmt -> "for" "(" (varDecl | exprStmt | ";") expression? ";" expression? ")" statement
		 */
		static Node ForStmt() {
			Eat(Token.KFor);
			Eat(Token.LParen);
			Node init = null;
			if (Match(Token.Semi)) init = null;
			else if (Peek(Token.KVar)) init = VarDecl();
			else init = ExprStmt();
			Node cond = null;
			if (!Peek(Token.Semi)) cond = Expression();
			Eat(Token.Semi);
			Node incr = null;
			if (!Peek(Token.RParen)) incr = Expression();
			Eat(Token.RParen);
			return new Node(Node.Statement, Token.KFor, init, cond, incr, Statement());
		}

		/**
		 * returnStmt -> "return" expression? ";"
		 */
		static Node ReturnStmt() {
			Eat(Token.KReturn);
			Node expr = null;
			if (!Peek(Token.Semi)) expr = Expression();
			Eat(Token.Semi);
			return new Node(Node.Return, expr);
		}

		/**
		* printStmt -> "print" expression ";"
		*/
		static Node PrintStmt() {
			Eat(Token.KPrint);
			var node = new Node(Node.Statement, Token.KPrint, Expression());
			Eat(Token.Semi);
			return node;
		}

		/**
		 * exprStmt -> expression ";"
		 */
		static Node ExprStmt() {
			var expr = Expression();
			Eat(Token.Semi);
			return expr;
		}

		/**
		 * expression -> assignment
		 */
		static Node Expression() {
			return Assignment();
		}

		/**
		 * assignment -> identifier "=" assignment
		 *             | logic_or
		 */
		static Node Assignment() {
			var expr = LogicOr();

			if (Match(Token.Equal)) {
				if (expr.type != Node.Variable) throw new Exception("Invalid assignment target");
				return new Node(Node.Assignment, expr.stringLiteral, Assignment());
			}

			return expr;
		}

		/**
		 * logic_or -> logic_and ("||" logic_and)*
		 */
		static Node LogicOr() {
			var expr = LogicAnd();
			while (Peek(Token.OrOr)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, LogicAnd());
			return expr;
		}

		/**
		 * logic_and -> equality ("&&" equality)*
		 */
		static Node LogicAnd() {
			var expr = Equality();
			while (Peek(Token.AndAnd)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Equality());
			return expr;
		}

		/**
		 * equality -> comparison (("!=" | "==") comparison)*
		 */
		static Node Equality() {
			var expr = Comparision();
			while (Peek(Token.BangEqual, Token.EqualEqual)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Comparision());
			return expr;
		}

		/** 
		 * comparison -> addition ((">" | ">=" | "<" | "<") addition)*
		 */
		static Node Comparision() {
			var expr = Bitwise();
			while (Peek(Token.LAngle, Token.LAngleEqual, Token.RAngle, Token.RAngleEqual)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Bitwise());
			return expr;
		}
		
		/**
		 * bitwise -> multiplication (("<<" | ">>" | "&" | "|") multiplication)*
		 */
		static Node Bitwise() {
			var expr = Addition();
			while (Peek(Token.RAngleAngle, Token.LAngleAngle, Token.And, Token.Or)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Addition());
			return expr;
		}

		/**
		 * addition -> multiplication (("-" | "+") multiplication)*
		 */
		static Node Addition() {
			var expr = Multiplication();
			while (Peek(Token.Minus, Token.Plus)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Multiplication());
			return expr;
		}

		/**
		 * multiplication -> unary (("/" | "*") unary)*
		 */
		static Node Multiplication() {
			var expr = Unary();
			while (Peek(Token.Slash, Token.Star)) 
				expr = MkBinaryOp(expr, _tokens[_pos++].type, Unary());
			return expr;
		}

		/** 
		 * unary -> ("!" | "-") unary
		 *        | call
		 */
		static Node Unary() {
			if (Peek(Token.Bang, Token.Minus)) 
				return MkUnaryOp(_tokens[_pos++].type, Unary());
			return Call();
		}

		/**
		 * call -> primary ("(" (expression ("," expression)*)? ")")*
		 */
		static Node Call() {
			var expr = Primary();

			var list = new List<Node>();
			while (true) {
				if (Match(Token.LParen)) {
					list.Clear();
					if (!Peek(Token.RParen)) {
						do list.Add(Expression()); while (Match(Token.Comma));
					}
					Eat(Token.RParen);
					expr = new Node(Node.Call, expr, list.ToArray());
				} else break;
			}

			return expr;
		}

		/**
		 * primary -> NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER
		 *          | "(" expression ")"
		 */
		static Node Primary() {
			if (Peek(Token.Number)) return MkLiteral(Token.Number, _tokens[_pos++].numberLiteral);
			if (Peek(Token.String)) return MkLiteral(Token.String, _tokens[_pos++].stringLiteral);

			if (Peek(Token.KFalse, Token.KTrue)) return MkLiteral(_tokens[_pos++].type);
			if (Match(Token.KNull)) return MkLiteral(Token.KNull);

			if (Peek(Token.Id)) return new Node(Node.Variable, _tokens[_pos++].stringLiteral);

			Eat(Token.LParen);
			var expr = Expression();
			Eat(Token.RParen);
			return expr;
		}

		static Node MkBinaryOp(Node left, byte token, Node right) {
			return new Node(Node.BinaryOp, token, left, right);
		}

		static Node MkUnaryOp(byte token, Node one) {
			return new Node(Node.UnaryOp, token, one);
		}

		static Node MkLiteral(byte token) {
			return new Node(Node.Literal, token);
		}

		static Node MkLiteral(byte token, float numberLiteral) {
			return new Node(Node.Literal, token, numberLiteral);
		}

		static Node MkLiteral(byte token, string stringLiteral) {
			return new Node(Node.Literal, token, stringLiteral);
		}
	}
}

