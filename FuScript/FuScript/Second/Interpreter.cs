using System;
using System.Collections.Generic;
using Env = System.Collections.Generic.Dictionary<string, FuScript.Second.ValueRef>;

namespace FuScript.Second {
	public sealed class Return : Exception {
		public Value value;

		public Return(Value val) {
			value = val;
		}
	}

	public delegate Value Func(Value[] values);

	public struct Value {
		public const byte Null = 0, True = 1, False = 2, Number = 3, String = 4, Function = 5;

		public byte type, arity;
		public double numberValue;
		public string stringValue;
		public Func function;

		public Value(bool boolean) {
			type = boolean ? True : False;
			numberValue = 0;
			stringValue = null;

			arity = 0;
			function = null;
		}

		public Value(double number) {
			type = Number;
			numberValue = number;
			stringValue = null;

			arity = 0;
			function = null;
		}

		public Value(string str) {
			type = String;
			numberValue = 0;
			stringValue = str;

			arity = 0;
			function = null;
		}

		public Value(byte n, Func func) {
			type = Function;
			arity = n;
			function = func;

			numberValue = 0;
			stringValue = null;
		}

		public Value Assign(Value other) {
			type = other.type;
			numberValue = other.numberValue;
			stringValue = other.stringValue;
			return this;
		}
		
		public bool GetBoolean() {
			if (type == Null || type == False || (type == Number && numberValue == 0)) return false;
			return true;
		}

		public double GetFloat() {
			if (type == Number) return numberValue;
			if (type == Null || type == False) return 0;
			if (type == True) return 1;

			throw new Exception("Cannot convert String to Number");
		}

		public int GetIngeter() {
			if (type == Number) return (int)numberValue;
			if (type == Null || type == False) return 0;
			if (type == True) return 1;

			throw new Exception("Cannot convert String to Number");
		}

		public string GetString() {
			if (type == String) return stringValue;
			if (type == Null)   return "";
			if (type == Number) return numberValue.ToString();
			return type == True ? "true" : "false";
		}

		public override string ToString() {
			switch (type) {
			case Value.Null:        return "null";
			case Value.True:        return "true";
			case Value.False:       return "false";
			case Value.Number:      return numberValue.ToString();
			case Value.String:      return "'" + stringValue + "'";
			case Value.Function:    return "function (" + arity + ") {...}";
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}

	public sealed class ValueRef {
		public Value value;

		public ValueRef(Value value) {
			this.value = value;
		}
	}

	public static class Interpreter {
		public static Env CreateEnv() {
			var env = new Env();
			env.Add("pi",       new ValueRef(new Value(Math.PI)));
			env.Add("e",        new ValueRef(new Value(Math.E)));

			env.Add("clock",    new ValueRef(new Value(0, args => new Value((double)DateTime.Now.Ticks / 1000.0))));

			env.Add("abs",      new ValueRef(new Value(1, args => new Value(Math.Abs(args[0].GetFloat())))));
			env.Add("acos",     new ValueRef(new Value(1, args => new Value(Math.Acos(args[0].GetFloat())))));
			env.Add("asin",     new ValueRef(new Value(1, args => new Value(Math.Asin(args[0].GetFloat())))));
			env.Add("atan",     new ValueRef(new Value(1, args => new Value(Math.Atan(args[0].GetFloat())))));
			env.Add("atan2",    new ValueRef(new Value(2, args => new Value(Math.Atan2(args[0].GetFloat(), args[1].GetFloat())))));
			env.Add("ceiling",  new ValueRef(new Value(1, args => new Value(Math.Ceiling(args[0].GetFloat())))));
			env.Add("cos",      new ValueRef(new Value(1, args => new Value(Math.Cos(args[0].GetFloat())))));
			env.Add("cosh",     new ValueRef(new Value(1, args => new Value(Math.Cosh(args[0].GetFloat())))));
			env.Add("exp",      new ValueRef(new Value(1, args => new Value(Math.Exp(args[0].GetFloat())))));
			env.Add("floor",    new ValueRef(new Value(1, args => new Value(Math.Floor(args[0].GetFloat())))));
			env.Add("ln",       new ValueRef(new Value(1, args => new Value(Math.Log(args[0].GetFloat())))));
			env.Add("log",      new ValueRef(new Value(2, args => new Value(Math.Log(args[0].GetFloat(), args[1].GetFloat())))));
			env.Add("log10",    new ValueRef(new Value(1, args => new Value(Math.Log10(args[0].GetFloat())))));
			env.Add("max",      new ValueRef(new Value(2, args => new Value(Math.Max(args[0].GetFloat(), args[1].GetFloat())))));
			env.Add("min",      new ValueRef(new Value(2, args => new Value(Math.Min(args[0].GetFloat(), args[1].GetFloat())))));
			env.Add("pow",      new ValueRef(new Value(2, args => new Value(Math.Pow(args[0].GetFloat(), args[1].GetFloat())))));
			env.Add("round",    new ValueRef(new Value(1, args => new Value(Math.Round(args[0].GetFloat())))));
			env.Add("sign",     new ValueRef(new Value(1, args => new Value(Math.Sign(args[0].GetFloat())))));
			env.Add("sin",      new ValueRef(new Value(1, args => new Value(Math.Sin(args[0].GetFloat())))));
			env.Add("sinh",     new ValueRef(new Value(1, args => new Value(Math.Sinh(args[0].GetFloat())))));
			env.Add("sqrt",     new ValueRef(new Value(1, args => new Value(Math.Sqrt(args[0].GetFloat())))));
			env.Add("tan",      new ValueRef(new Value(1, args => new Value(Math.Tan(args[0].GetFloat())))));
			env.Add("tanh",     new ValueRef(new Value(1, args => new Value(Math.Tanh(args[0].GetFloat())))));
			env.Add("truncate", new ValueRef(new Value(1, args => new Value(Math.Truncate(args[0].GetFloat())))));
			return env;
		}

		public static Env CopyEnv(Env env) {
			return new Env(env);
		}

		public static Value Eval(Node node, Env env) {
			switch (node.type) {
			case Node.BinaryOp:
				var left = Eval(node.child1, env);
				Value right;

				switch (node.token) {
				case Token.Minus:       right = Eval(node.child2, env); return new Value(left.GetFloat() - right.GetFloat());
				case Token.Plus:        right = Eval(node.child2, env); return left.type != Value.String && right.type != Value.String ? new Value(left.GetFloat() + right.GetFloat()) : new Value(left.GetString() + right.GetString());
				case Token.Slash:       right = Eval(node.child2, env); return new Value(left.GetFloat() / right.GetFloat());
				case Token.Star:        right = Eval(node.child2, env); return new Value(left.GetFloat() * right.GetFloat());

				case Token.BangEqual:   right = Eval(node.child2, env); return new Value(left.type != right.type ? false : left.type == Value.String ? left.GetString() != right.GetString() : left.GetFloat() != right.GetFloat()); 
				case Token.EqualEqual:  right = Eval(node.child2, env); return new Value(left.type != right.type ? false : left.type == Value.String ? left.GetString() == right.GetString() : left.GetFloat() == right.GetFloat()); 
				case Token.LAngle:      right = Eval(node.child2, env); return new Value(left.GetFloat() <  right.GetFloat());  
				case Token.LAngleEqual: right = Eval(node.child2, env); return new Value(left.GetFloat() <= right.GetFloat());  
				case Token.RAngle:      right = Eval(node.child2, env); return new Value(left.GetFloat() >  right.GetFloat());  
				case Token.RAngleEqual: right = Eval(node.child2, env); return new Value(left.GetFloat() >= right.GetFloat());  
				
				case Token.LAngleAngle: right = Eval(node.child2, env); return new Value(left.GetIngeter() << right.GetIngeter()); 
				case Token.RAngleAngle: right = Eval(node.child2, env); return new Value(left.GetIngeter() >> right.GetIngeter()); 
				case Token.And:         right = Eval(node.child2, env); return new Value(left.GetIngeter() &  right.GetIngeter()); 
				case Token.Or:          right = Eval(node.child2, env); return new Value(left.GetIngeter() |  right.GetIngeter()); 
					
				case Token.AndAnd:      return !left.GetBoolean() ? left : Eval(node.child2, env);
				case Token.OrOr:        return  left.GetBoolean() ? left : Eval(node.child2, env);;
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.UnaryOp:
				var one = Eval(node.child1, env);

				switch (node.token) {
				case Token.Minus:       return new Value(-one.GetFloat());
				case Token.Bang:        return new Value(!one.GetBoolean());
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Literal:
				switch (node.token) {
//				case Token.Id:          return node.stringLiteral;
				case Token.String:      return new Value(node.stringLiteral);
				case Token.Number:      return new Value(node.numberLiteral);
				case Token.KFalse:      return new Value(false);
				case Token.KTrue:       return new Value(true);
				case Token.KNull:       return new Value();
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Statement:
				switch (node.token) {
				case Token.KPrint:      Console.WriteLine(Eval(node.child1, env)); return new Value();
				case Token.KIf:
					if (Eval(node.child1, env).GetBoolean()) return Eval(node.child2, env);
					else if (node.child3 != null) return Eval(node.child3, env);
					else return new Value();
				case Token.KWhile:
					while (Eval(node.child1, env).GetBoolean()) Eval(node.child2, env);
					return new Value();
				case Token.KFor:
					env = CopyEnv(env);
					if (node.child1 != null) Eval(node.child1, env);
					while (node.child2 == null ? true : Eval(node.child2, env).GetBoolean()) {
						Eval(node.child4, env);
						Eval(node.child3, env);
					}
					return new Value();
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Program:
				int length = node.children.Length;
				for (int i = 0; i < length; i++) Eval(node.children[i], env);
				return new Value();
			case Node.VarDecl:
				var value = node.child1 == null ? new Value() : Eval(node.child1, env);
				env[node.stringLiteral] = new ValueRef(value);
				return value;
			case Node.Variable:
				if (!env.ContainsKey(node.stringLiteral)) throw new Exception("Variable never defined");
				return env[node.stringLiteral].value;
			case Node.Assignment:
				if (!env.ContainsKey(node.stringLiteral)) throw new Exception("Variable never defined");
				return env[node.stringLiteral].value = Eval(node.child1, env);
			case Node.Block:
				env = CopyEnv(env);
				length = node.children.Length;
				for (int i = 0; i < length; i++) Eval(node.children[i], env);
				return new Value();
			case Node.Call:
				var callee = Eval(node.child1, env);
				if (callee.type != Value.Function) throw new Exception("Callee not callable");
				if (callee.arity != node.children.Length) throw new Exception("Arguments count mismatch");
				var args = new Value[callee.arity];
				length = node.children.Length;
				for (int i = 0; i < length; i++) args[i] = Eval(node.children[i], env);
				return callee.function(args);
			case Node.FuncDecl:
				length = node.children.Length;
				var closure = CopyEnv(env);
				var func = new Value((byte)length, arg => {
					for (int i = 0; i < length; i++) closure[node.children[i].stringLiteral] = new ValueRef(arg[i]);
					try {
						Eval(node.child1, closure);
					} catch(Return ret) {
						return ret.value;
					}
					return new Value();
				});
				env[node.stringLiteral] = new ValueRef(func);
				return func;
			case Node.Return:
				if (node.child1 != null) throw new Return(Eval(node.child1, env));
				throw new Return(new Value());
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}
}

