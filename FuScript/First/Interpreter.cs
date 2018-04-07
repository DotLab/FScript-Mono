using System;
using System.Text;
using Env = System.Collections.Generic.Dictionary<string, FuScript.First.Value>;

namespace FuScript.First {
	public class Value {
		public float numberValue;
	}

	/*
	 public const int Prog = 20, Blck = 21, NoOp = 22;
	 public const int Asgn = 30, Var = 31, Expr = 32;
	*/
	public static class Interpreter {
		public static Env NewEnv() {
			return new Env();
		}

		public static Env CloneEnv(Env env) {
			return new Env(env);
		}

		public static float Eval(Node node, Env env) {
			switch (node.type) {
			case Node.Num:
				return node.num;
			case Node.Var:
				if (!env.ContainsKey(node.id)) throw new Exception("Never defined " + node.id);
				return env[node.id].numberValue;

			case Node.Add:
				return Eval(node.left, env) + Eval(node.right, env);
			case Node.Sub:
				return Eval(node.left, env) - Eval(node.right, env);
			case Node.Mul:
				return Eval(node.left, env) * Eval(node.right, env);
			case Node.Div:
				return Eval(node.left, env) / Eval(node.right, env);

			case Node.Pos:
				return +Eval(node.left, env);
			case Node.Neg:
				return -Eval(node.left, env);

			case Node.Prog:
				return Eval(node.left, env);

			case Node.Blck:
				env = CloneEnv(env);
				for (int i = 0; i < node.children.Length; i++) {
					Eval(node.children[i], env);
				}
				return 0;

			case Node.NoOp:
				return 0;

			case Node.Asgn:
				if (!env.ContainsKey(node.left.id)) throw new Exception("Never defined " + node.id);
				return env[node.left.id].numberValue = Eval(node.right, env);
			case Node.Decl:
				env[node.left.id] = new Value{ numberValue = Eval(node.right, env) };
				return env[node.left.id].numberValue;

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
			case Node.Decl:
				return string.Format("var {0} = {1}", Print(node.left), Print(node.right));

			default:
				throw new Exception("Code pass not possible " + node.type);
			}
		}
	}
}

