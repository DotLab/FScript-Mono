using System;
using System.Collections.Generic;
using Env = System.Collections.Generic.Dictionary<string, FuScript.Value>;

namespace FuScript {
	public struct Value {
		public const byte Null = 0, True = 1, False = 2, Number = 3, String = 4;

		public byte type;
		public float numberValue;
		public string stringValue;

		public Value(bool boolean) {
			type = boolean ? True : False;
			numberValue = 0;
			stringValue = null;
		}

		public Value(float number) {
			type = Number;
			numberValue = number;
			stringValue = null;
		}

		public Value(string str) {
			type = String;
			numberValue = 0;
			stringValue = str;
		}
		
		public bool GetBoolean() {
			if (type == Null || type == False || (type == Number && numberValue == 0)) return false;
			return true;
		}

		public float GetFloat() {
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
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}

	public static class Interpreter {
		public static Env CreateEnv() {
			return new Env();
		}

		public static Env CopyEnv(Env env) {
			return new Env(env);
		}

		public static Value Eval(Node node, Env env) {
			switch (node.type) {
			case Node.BinaryOp:
				var left = Eval(node.child1, env);
				var right = Eval(node.child2, env);

				switch (node.token) {
				case Token.Minus:       return new Value(left.GetFloat() - right.GetFloat());
				case Token.Plus:        return left.type != Value.String && right.type != Value.String ? new Value(left.GetFloat() + right.GetFloat()) : new Value(left.GetString() + right.GetString());
				case Token.Slash:       return new Value(left.GetFloat() / right.GetFloat());
				case Token.Star:        return new Value(left.GetFloat() * right.GetFloat());

				case Token.BangEqual:   return new Value(left.type != right.type ? false : left.type == Value.String ? left.GetString() != right.GetString() : left.GetFloat() != right.GetFloat()); 
				case Token.EqualEqual:  return new Value(left.type != right.type ? false : left.type == Value.String ? left.GetString() == right.GetString() : left.GetFloat() == right.GetFloat()); 
				case Token.LAngle:      return new Value(left.GetFloat() <  right.GetFloat());  
				case Token.LAngleEqual: return new Value(left.GetFloat() <= right.GetFloat());  
				case Token.RAngle:      return new Value(left.GetFloat() >  right.GetFloat());  
				case Token.RAngleEqual: return new Value(left.GetFloat() >= right.GetFloat());  
				
				case Token.LAngleAngle: return new Value(left.GetIngeter() << right.GetIngeter()); 
				case Token.RAngleAngle: return new Value(left.GetIngeter() >> right.GetIngeter()); 
				case Token.And:         return new Value(left.GetIngeter() &  right.GetIngeter()); 
				case Token.Or:          return new Value(left.GetIngeter() |  right.GetIngeter()); 
					
				case Token.AndAnd:      return new Value(left.GetBoolean() && right.GetBoolean()); 
				case Token.OrOr:        return new Value(left.GetBoolean() || right.GetBoolean()); 
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
				default:
					throw new System.Exception("Code path not possible");
				}
			case Node.Program:
				int length = node.children.Length;
				for (int i = 0; i < length; i++) Eval(node.children[i], env);
				return new Value();
			case Node.VarDecl:
				var value = node.child1 == null ? new Value() : Eval(node.child1, env);
				return env[node.stringLiteral] = value;
			case Node.Variable:
				if (!env.ContainsKey(node.stringLiteral)) throw new Exception("Variable never defined");
				return env[node.stringLiteral];
			case Node.Assignment:
				if (!env.ContainsKey(node.stringLiteral)) throw new Exception("Variable never defined");
				return env[node.stringLiteral] = Eval(node.child1, env);
			case Node.Block:
				env = CopyEnv(env);
				length = node.children.Length;
				for (int i = 0; i < length; i++) Eval(node.children[i], env);
				return new Value();
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}
}

