using System;

namespace FuScript {
	public struct Value {
		public const byte Null = 0, True = 1, False = 2, Number = 3, String = 4, Function = 5;

		public byte type;
		public double num;
		public string str;

		public Value(bool b) {
			type = b ? True : False;
			num = 0;
			str = null;
		}

		public Value(double n) {
			type = Number;
			num = n;
			str = null;
		}

		public Value(string s) {
			type = String;
			num = 0;
			str = s;
		}

		public bool GetBoolean() {
			if (type == Null || type == False || (type == Number && num == 0)) return false;
			return true;
		}

		public double GetFloat() {
			if (type == Number) return num;
			if (type == Null || type == False) return 0;
			if (type == True) return 1;

			throw new Exception("Cannot convert String to Number");
		}

		public int GetIngeter() {
			if (type == Number) return (int)num;
			if (type == Null || type == False) return 0;
			if (type == True) return 1;

			throw new Exception("Cannot convert String to Number");
		}

		public string GetString() {
			if (type == String) return str;
			if (type == Null)   return "";
			if (type == Number) return num.ToString();
			return type == True ? "true" : "false";
		}

		public override string ToString() {
			switch (type) {
			case Value.Null:        return "null";
			case Value.True:        return "true";
			case Value.False:       return "false";
			case Value.Number:      return num.ToString();
			case Value.String:      return "'" + str + "'";
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

