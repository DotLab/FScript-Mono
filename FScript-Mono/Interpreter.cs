using System;

namespace FScriptMono {
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

		public static string Print(Node node) {
			switch (node.type) {
				case Node.Num:
					return node.num.ToString("N1");

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

				default:
					throw new Exception("Code pass not possible");
			}
		}
	}
}

