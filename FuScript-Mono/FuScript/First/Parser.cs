using System.Collections.Generic;

namespace FuScript.First {
	public sealed class Node {
		public const int Num = 0;
		public const int Add = 1, Sub = 2, Mul = 3, Div = 4;
		public const int Pos = 11, Neg = 12;
		public const int Prog = 20, Blck = 21, NoOp = 22;
		public const int Asgn = 30, Var = 31, Decl = 32;

		public int type;
		public float num;
		public string id;
		public Node left, right;
		public Node[] children;
	}

	public static class Parser {
		static Node Var(string id) {
			return new Node{ type = Node.Var, id = id };
		}

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
			var token = Lexer.Expect(Token.Num, Token.LParen, Token.Plus, Token.Minus, Token.Id);
			if (token.type == Token.Num) return Num(token.num);
			if (token.type == Token.LParen) {
				var result = Expr();
				Lexer.Expect(Token.RParen);
				return result;
			}
			if (token.type == Token.Id) return Var(token.id);

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

		static Node Expr() {
			var node = Term();
			Token token;
			while ((token = Lexer.Prefer(Token.Plus, Token.Minus)).type != Token.Mis) {
				node = BinOp(token.type, node, Term());
			}
			return node;
		}

		static Node Asgn() {
			var left = Var(Lexer.Expect(Token.Id).id);
			Lexer.Expect(Token.Equal);
			var right = Expr();
			return new Node{ type = Node.Asgn, left = left, right = right };
		}

		static Node Decl() {
			Lexer.Expect(Token.Var);
			var left = Var(Lexer.Expect(Token.Id).id);
			Lexer.Expect(Token.Equal);
			var right = Expr();
			return new Node{ type = Node.Decl, left = left, right = right };
		}

		public static Node Stmt() {
			var token = Lexer.Peek();
			if (token.type == Token.LCurly) return Block();
			if (token.type == Token.Id) return Asgn();
			if (token.type == Token.Var) return Decl();
			return new Node{ type = Node.NoOp };

		}

		static Node Block() {
			Lexer.Expect(Token.LCurly);
			var list = new List<Node>();
			list.Add(Stmt());
			while ((Lexer.Prefer(Token.Semi)).type != Token.Mis) {
				list.Add(Stmt());
			}
			Lexer.Expect(Token.RCurly);
			return new Node{ type = Node.Blck, children = list.ToArray() };
		}

		public static Node Prog() {
			return new Node{ type = Node.Prog, left = Block() };
		}
	}
}

