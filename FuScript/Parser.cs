using System;

namespace FuScript {
	public sealed class Node {
		public const byte BinaryOp = 0, UnaryOp = 1, Literal = 2;

		public readonly byte type, token;
		public readonly Node node1, node2, node3;

		public readonly bool boolLiteral;
		public readonly float numberLiteral;
		public readonly string stringLiteral;

		public Node(byte type, byte token) {
			this.type = type;
			this.token = token;
		}

		public Node(byte type, byte token, Node node1) {
			this.type = type;
			this.token = token;
			this.node1 = node1;
		}

		public Node(byte type, byte token, Node node1, Node node2) {
			this.type = type;
			this.token = token;
			this.node1 = node1;
			this.node2 = node2;
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

		public override string ToString() {
			switch (type) {
			case Node.BinaryOp:
				switch (token) {
				case Token.Minus:       return "(" + node1 + " - "  + node2 + ")";
				case Token.Plus:        return "(" + node1 + " + "  + node2 + ")";
				case Token.Slash:       return "(" + node1 + " / "  + node2 + ")";
				case Token.Star:        return "(" + node1 + " * "  + node2 + ")";
				case Token.BangEqual:   return "(" + node1 + " != " + node2 + ")";
				case Token.EqualEqual:  return "(" + node1 + " == " + node2 + ")";
				case Token.LAngle:      return "(" + node1 + " < "  + node2 + ")";
				case Token.LAngleEqual: return "(" + node1 + " <= " + node2 + ")";
				case Token.LAngleAngle: return "(" + node1 + " << " + node2 + ")";
				case Token.RAngle:      return "(" + node1 + " > "  + node2 + ")";
				case Token.RAngleEqual: return "(" + node1 + " >= " + node2 + ")";
				case Token.RAngleAngle: return "(" + node1 + " >> " + node2 + ")";
				case Token.And:         return "(" + node1 + " & "  + node2 + ")";
				case Token.AndAnd:      return "(" + node1 + " && " + node2 + ")";
				case Token.Or:          return "(" + node1 + " | "  + node2 + ")";
				case Token.OrOr:        return "(" + node1 + " || " + node2 + ")";
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.UnaryOp:
				switch (token) {
				case Token.Minus:       return "-" + node1;
				case Token.Bang:        return "!" + node2;
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Literal:
				switch (token) {
				case Token.Id:          return stringLiteral;
				case Token.String:      return "'" + stringLiteral + "'";
				case Token.Number:      return numberLiteral.ToString("N1");
				case Token.KFalse:      return "false";
				case Token.KTrue:       return "true";
				case Token.KNull:       return "null";
				default:
					throw new System.Exception("Code path not possible");
				}
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

		static void Eat(byte type) {
			if (_tokens[_pos].type == type) _pos += 1;
			throw new Exception("Unexpected token " + _tokens[_pos] + ", expecting " + new Token(type));
		}

		/**
		 * expression -> equlity
		 */
		public static Node Expression() {
			return Equality();
		}

		/**
		 * equality -> comparison (("!=" | "==") comparison)*
		 */
		static Node Equality() {
			var expr = Comparision();
			while (Peek(Token.BangEqual, Token.EqualEqual)) 
				expr = BinaryOp(expr, _tokens[_pos++].type, Comparision());
			return expr;
		}

		/** 
		 * comparison -> addition ((">" | ">=" | "<" | "<") addition)*
		 */
		static Node Comparision() {
			var expr = Bitwise();
			while (Peek(Token.LAngle, Token.LAngleEqual, Token.RAngle, Token.RAngleEqual)) 
				expr = BinaryOp(expr, _tokens[_pos++].type, Bitwise());
			return expr;
		}
		
		/**
		 * bitwise -> multiplication (("<<" | ">>" | "&" | "|") multiplication)*
		 */
		static Node Bitwise() {
			var expr = Addition();
			while (Peek(Token.RAngleAngle, Token.LAngleAngle, Token.And, Token.Or)) 
				expr = BinaryOp(expr, _tokens[_pos++].type, Addition());
			return expr;
		}

		/**
		 * addition -> bitwise (("-" | "+") bitwise)*
		 */
		static Node Addition() {
			var expr = Multiplication();
			while (Peek(Token.Minus, Token.Plus)) 
				expr = BinaryOp(expr, _tokens[_pos++].type, Multiplication());
			return expr;
		}

		/**
		 * multiplication -> unary (("/" | "*") unary)*
		 */
		static Node Multiplication() {
			var expr = Unary();
			while (Peek(Token.Slash, Token.Star)) 
				expr = BinaryOp(expr, _tokens[_pos++].type, Unary());
			return expr;
		}

		/** 
		 * unary -> ("!" | "-") unary
		 *        | primary
		 */
		static Node Unary() {
			if (Peek(Token.Bang, Token.Minus)) 
				return UnaryOp(_tokens[_pos++].type, Unary());
			return Primary();
		}

		/**
		 * primary -> NUMBER | STRING | FALSE | TRUE | NULL
		 *          | "(" expression ")"
		 */
		static Node Primary() {
			if (Peek(Token.Number)) return Literal(Token.Number, _tokens[_pos++].numberLiteral);
			if (Peek(Token.String)) return Literal(Token.String, _tokens[_pos++].stringLiteral);

			if (Peek(Token.KFalse, Token.KTrue)) return Literal(_tokens[_pos++].type);
			if (Match(Token.KNull)) return Literal(Token.KNull);

			Eat(Token.LParen);
			var expr = Expression();
			Eat(Token.RParen);
			return expr;
		}

		static Node BinaryOp(Node left, byte token, Node right) {
			return new Node(Node.BinaryOp, token, left, right);
		}

		static Node UnaryOp(byte token, Node one) {
			return new Node(Node.UnaryOp, token, one);
		}

		static Node Literal(byte token) {
			return new Node(Node.Literal, token);
		}

		static Node Literal(byte token, float numberLiteral) {
			return new Node(Node.Literal, token, numberLiteral);
		}

		static Node Literal(byte token, string stringLiteral) {
			return new Node(Node.Literal, token, stringLiteral);
		}
	}
}

