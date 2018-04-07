using System;
using System.Text;

namespace FScriptMono {
	/*
	 public const int Prog = 20, Blck = 21, NoOp = 22;
	 public const int Asgn = 30, Var = 31, Expr = 32;
	*/
	public static class Interpreter {
		public static float Eval(Node node) {
			switch (node.type) {
				case Node.Num:
					return node.num;

				case Node.Add:
					return Eval(node.left) + Eval(node.right);
				case Node.Sub:
					return Eval(node.left) - Eval(node.right);
				case Node.Mul:
					return Eval(node.left) * Eval(node.right);
				case Node.Div:
					return Eval(node.left) / Eval(node.right);

				case Node.Pos:
					return +Eval(node.left);
				case Node.Neg:
					return -Eval(node.left);

				default:
					throw new Exception("Code pass not possible");
			}
		}

		/*
	 public const int Prog = 20, Blck = 21, NoOp = 22;
	 public const int Asgn = 30, Var = 31, Expr = 32;
	*/
		static readonly StringBuilder sb = new StringBuilder();
		public static string Print(Node node) {
			switch (node.type) {
				case Node.Num:
					return node.num.ToString("N1");
				case Node.Var:
					return node.id;

				case Node.Add:
					return string.Format("({0} + {1})", Print(node.left), Print(node.right));
				case Node.Sub:
					return string.Format("({0} - {1})", Print(node.left), Print(node.right));
				case Node.Mul:
					return string.Format("({0} * {1})", Print(node.left), Print(node.right));
				case Node.Div:
					return string.Format("({0} / {1})", Print(node.left), Print(node.right));

				case Node.Pos:
					return "+" + Print(node.left);
				case Node.Neg:
					return "-" + Print(node.left);

				case Node.Prog:
					return Print(node.left);

				case Node.Blck:
					sb.Clear();
					sb.Append("{ ");
					for (int i = 0; i < node.children.Length; i++) {
						sb.Append(Print(node.children[i]) + "; ");
					}
					sb.Append("}");
					return sb.ToString();
				
				case Node.NoOp:
					return "";
				
				case Node.Asgn:
					return string.Format("{0} = {1}", Print(node.left), Print(node.right));

				default:
					throw new Exception("Code pass not possible " + node.type);
			}
		}
	}
}

