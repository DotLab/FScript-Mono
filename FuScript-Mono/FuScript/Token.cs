namespace FuScript {
	public class UnrecognizedTokenException : System.Exception {
		public UnrecognizedTokenException(byte t) : base("Unrecognized token #" + t) { }
	}

	public static class Token {
		public const byte LParen = 0, RParen = 1, LCurly = 2, RCurly = 3, LSquare = 4, RSquare = 5;
		public const byte Comma = 6, Dot = 7, Semi = 8, Colon = 9, Quest = 10, Tilde = 11, Sharp = 12;

		public const byte Minus = 20, MinusEqual = 21, MinusMinus = 32;
		public const byte Plus = 22,  PlusEqual = 23,  PlusPlus = 33;
		public const byte Slash = 24, SlashEqual = 25;
		public const byte Star = 26,  StarEqual = 27;
		public const byte Caret = 28, CaretEqual = 29;
		public const byte Mod = 30,   ModEqual = 31;

		public const byte Bang = 40,  BangEqual = 41;
		public const byte Equal = 42, EqualEqual = 43;

		public const byte LAngle = 44, LAngleEqual = 45, LAngleAngle = 46, LAngleAngleEqual = 56;
		public const byte RAngle = 47, RAngleEqual = 48, RAngleAngle = 49, RAngleAngleEqual = 57;
		public const byte And = 50,    AndEqual = 54,    AndAnd = 51;
		public const byte Or = 52,     OrEqual = 55,     OrOr = 53;

		public const byte Id = 60, String = 61, Int = 62, Float = 63;

		public const byte KTrue = 70, KFalse = 71, KNull = 72;
		public const byte KIf = 73, KElse = 74;
		public const byte KWhile = 75, KDo = 76, KFor = 77;
		public const byte KReturn = 78, KBreak = 79, KContinue = 80;
		public const byte KVar = 81, KFunction = 82;
		public const byte KPrint = 83;


		public const byte Eof = 255;

		public static string Recant(byte t) {
			switch (t) {
				case LParen:           return "(";
				case RParen:           return ")";
				case LCurly:           return "{";
				case RCurly:           return "}";
				case LSquare:          return "[";
				case RSquare:          return "]";
				case Comma:            return ",";
				case Dot:              return ".";
				case Semi:             return ";";
				case Colon:            return ":";
				case Quest:            return "?";
				case Tilde:            return "~";
				case Sharp:            return "#";

				case Minus:            return "-";
				case MinusEqual:       return "-=";
				case MinusMinus:       return "--";
				case Plus:             return "+";
				case PlusEqual:        return "+=";
				case PlusPlus:         return "++";
				case Slash:            return "/";
				case SlashEqual:       return "/=";
				case Star:             return "*";
				case StarEqual:        return "*=";
				case Caret:            return "^";
				case CaretEqual:       return "^=";
				case Mod:              return "%";
				case ModEqual:         return "%=";

				case Bang:             return "!";
				case BangEqual:        return "!=";
				case Equal:            return "=";
				case EqualEqual:       return "==";

				case LAngle:           return "<";
				case LAngleEqual:      return "<=";
				case LAngleAngle:      return "<<";
				case LAngleAngleEqual: return "<<=";
				case RAngle:           return ">";
				case RAngleEqual:      return ">=";
				case RAngleAngle:      return ">>";
				case RAngleAngleEqual: return ">>=";
				case And:              return "&";
				case AndEqual:         return "&=";
				case AndAnd:           return "&&";
				case Or:               return "|";
				case OrEqual:          return "|=";
				case OrOr:             return "||";

				case Id:               return "ID";
				case String:           return "STRING";
				case Int:              return "INT";
				case Float:            return "FLOAT";

				case KTrue:            return "true";
				case KFalse:           return "false";
				case KNull:            return "null";
				case KIf:              return "if";
				case KElse:            return "else";
				case KWhile:           return "while";
				case KDo:              return "do";
				case KFor:             return "for";
				case KReturn:          return "return";
				case KBreak:           return "break";
				case KContinue:        return "continue";
				case KVar:             return "var";
				case KFunction:        return "function";
				case KPrint:           return "print";

				case Eof:              return "EOF";

				default:
					throw new UnrecognizedTokenException(t);
			}
		}

		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string Recant(byte[] ts) {
			if (ts == null || ts.Length < 1) return "[ ]";
			if (ts.Length == 1) return "[ " + Recant(ts[0]) + " ]";

			sb.Clear();
			sb.Append("[ ");
			int length = ts.Length;
			for (int i = 0; i < length; ++i) {
				sb.Append(Recant(ts[i]));
				if (i + 1 < length) sb.Append(", ");
				else sb.Append(" ");
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
}
