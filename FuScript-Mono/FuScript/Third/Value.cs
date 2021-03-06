using Obj = System.Collections.Generic.Dictionary<string, FuScript.Third.Value>;

namespace FuScript.Third {
	public delegate Value InteropFunction (Value[] args);

	public struct Value {
		public const byte Falsy = 3;
		public const byte True = 0, Null = 1, False = 2, Number = 4, String = 8, Function = 16, Interop = 32, Object = 64;

		public byte type;
		public double num;

		public ushort sys1, sys2;
		public object obj;

		public void Set() {
			type = Null;
		}

		public void Set(bool b) {
			type = b ? True : False;
			num = b ? 1 : 0;
		}

		public void Set(ushort i) {
			type = Number;
			num = sys1 = sys2 = i;
		}

		public void Set(double n) {
			type = Number;
			num = n;
		}

		public Value(double n) {
			type = Number;
			num = n;

			sys1 = sys2 = 0;
			obj = null;
		}

		public void Set(string s) {
			type = String;
			obj = s;
		}

		public void Set(ushort n, ushort ci, object env) {
			type = Function;
			sys1 = n;
			sys2 = ci;
			obj = env;
		}

		public void Set(ushort n, InteropFunction func) {
			type = Interop;
			sys1 = n;
			obj = func;
		}

		public Value(ushort n, InteropFunction func) {
			type = Interop;
			num = 0;
			sys1 = sys2 = n;
			obj = func;
		}

		public void Set(Obj o) {
			type = Object;
			obj = o;
		}

		public bool IsFalsy() {
			return (type & Value.Falsy) != 0 || (type == Value.Number && num == 0);
		}

		public string AsString() {
			switch (type) {
			case Value.Null:        return "null";
			case Value.True:        return "true";
			case Value.False:       return "false";
			case Value.Number:      return num.ToString();
			case Value.String:      return obj.ToString();
			case Value.Function:    return "function";
			case Value.Interop:     return "interop";
			case Value.Object:      return "object";
			default:
				throw new System.Exception("Code path not possible");
			}
		}

		public override string ToString() {
			switch (type) {
			case Value.Null:        return "null";
			case Value.True:        return "true";
			case Value.False:       return "false";
			case Value.Number:      return num.ToString();
			case Value.String:      return "'" + obj + "'";
			case Value.Function:    return "f(" + sys1 + ")@" + sys2;
			case Value.Interop:     return "f(" + sys1 + ")";
			case Value.Object:      return "{" + ((Obj)obj).Count + "}";
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

		public override string ToString() {
			return value.ToString();
		}
	}
}

