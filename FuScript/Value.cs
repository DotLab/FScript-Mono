using System;

namespace FuScript {
	public struct Value {
		public const byte Null = 0, True = 1, False = 2, Number = 3, String = 4, Function = 5;

		public byte type;
		public double num;

		public ushort sys1, sys2;
		public readonly object obj;

		public Value(bool b) {
			type = b ? True : False;
			num = sys1 = sys2 = 0;
			obj = null;
		}

		public Value(ushort i) {
			type = Number;
			num = sys1 = sys2 = i;
			obj = null;
		}

		public Value(double n) {
			type = Number;
			num = n; sys1 = sys2 = 0;
			obj = null;
		}

		public Value(string s) {
			type = String;
			num = sys1 = sys2 = 0;
			obj = s;
		}

		public override string ToString() {
			switch (type) {
			case Value.Null:        return "null";
			case Value.True:        return "true";
			case Value.False:       return "false";
			case Value.Number:      return num.ToString();
			case Value.String:      return "'" + obj + "'";
			case Value.Function:    return "function (" + sys2 + ") @ " + sys1;
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

