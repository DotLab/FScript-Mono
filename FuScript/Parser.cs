using System;
using System.Collections.Generic;

namespace FuScript {
	public sealed class Node {
		public const byte BinaryOp = 0, UnaryOp = 1, Literal = 2, Statement = 3, Program = 4;
		public const byte VarDecl = 5, Variable = 6;

		public readonly byte type, token;
		public readonly Node child1, child2, child3;
		public readonly Node[] children;

		public readonly bool boolLiteral;
		public readonly float numberLiteral;
		public readonly string stringLiteral;

		public Node(byte type, byte token) {
			this.type = type;
			this.token = token;
		}

		public Node(byte type, byte token, Node[] children) {
			this.type = type;
			this.token = token;
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
				case Token.Bang:        return "!" + child2;
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
				case Token.KPrint:      return "print " + child1;
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Program:
				int length = children.Length;
				var sb = new System.Text.StringBuilder();
				for (int i = 0; i < length; i++) sb.Append(children[i] + "; ");
				return sb.ToString();
			case Node.VarDecl:
				return child1 == null ? "var " + stringLiteral : "var " + stringLiteral + " = " + child1;
			case Node.Variable:
				return stringLiteral;
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}

	public static class Parser {
		static Token[] _tokens;
		static int _pos, _length;

		static List<Node> _list = new List<Node>();

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
		 * program -> (declaration ";")* EOF
		 */
		public static Node Program() {
			_list.Clear();
			while (!Match(Token.Eof)) {
				_list.Add(Declaration());
				Eat(Token.Semi);
			}
			return MkProgram(Token.Eof, _list.ToArray());
		}

		/**
		 * declaration -> valDecl
		 *              | statement
		 */
		static Node Declaration() {
			if (Peek(Token.KVar)) return VarDecl();
			return Statement();
		}

		/**
		 * varDecl -> "var" IDENTIFIER ("=" expression)?
		 */
		static Node VarDecl() {
			var token = Eat(Token.KVar);
			string id = Eat(Token.Id).stringLiteral;
			if (Match(Token.Equal)) {
				return MkDeclaration(token.type, id, Expression());
			}
			return MkDeclaration(token.type, id);
		}

		/**
		 * statement -> exprStmt
		 *            | printStmt
		 */
		public static Node Statement() {
			if (Peek(Token.KPrint)) return PrintStmt();
			return ExprStmt();
		}

		/**
		 * exprStmt -> expression
		 */
		static Node ExprStmt() {
			return Expression();
		}

		/**
		 * printStmt -> "print" expression
		 */
		static Node PrintStmt() {
			var token = Eat(Token.KPrint);
			return MkStatement(token.type, Expression());
		}

		/**
		 * expression -> equlity
		 */
		static Node Expression() {
			return Equality();
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
		 * addition -> bitwise (("-" | "+") bitwise)*
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
		 *        | primary
		 */
		static Node Unary() {
			if (Peek(Token.Bang, Token.Minus)) 
				return MkUnaryOp(_tokens[_pos++].type, Unary());
			return Primary();
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

		static Node MkStatement(byte token, Node expr) {
			return new Node(Node.Statement, token, expr);
		}

		static Node MkProgram(byte token, Node[] children) {
			return new Node(Node.Program, token, children);
		}

		static Node MkDeclaration(byte token, string id) {
			return new Node(Node.VarDecl, token, id);
		}

		static Node MkDeclaration(byte token, string id, Node expr) {
			return new Node(Node.VarDecl, token, id, expr);
		}
	}
}

