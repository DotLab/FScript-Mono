using System;

namespace FScriptMono {
	public sealed class Node {
		public const int Num = 0;
		public const int Add = 1, Sub = 2, Mul = 3, Div = 4;
		public const int Pos = 11, Neg = 12;

		public int type;
		public float num;
		public Node left, right;
	}

	public static class Parser {
		static Node Num(float num) {
			return new Node{ type = Node.Num, num = num };
		}

		static Node UnaryOp(int op, Node exp) {
			return new Node{ type = op + 10, left = exp };
		}

		static Node BinOp(int op, Node left, Node right) {
			return new Node{ type = op, left = left, right = right };
		}

		static Node Factor() {
			var token = Lexer.Expect(Token.Num, Token.LParen, Token.Plus, Token.Minus);
			if (token.type == Token.Num) return Num(token.num);
			if (token.type == Token.LParen) {
				var result = Expr();
				Lexer.Expect(Token.RParen);
				return result;
			}

			// Plus || Minus
			return UnaryOp(token.type, Factor());
		}

		static Node Term() {
			var node = Factor();
			Token token;
			while ((token = Lexer.Prefer(Token.Mul, Token.Div)).type != Token.Mis) {
				node = BinOp(token.type, node, Factor());
			}
			return node;
		}

		public static Node Expr() {
			var node = Term();
			Token token;
			while ((token = Lexer.Prefer(Token.Plus, Token.Minus)).type != Token.Mis) {
				node = BinOp(token.type, node, Term());
			}
			return node;
		}
	}
}

