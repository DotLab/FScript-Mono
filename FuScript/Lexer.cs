namespace FuScript {
	public static class Token {
		// One
		public const byte LParen = 0, RParen = 1, LCurly = 2, RCurly = 3, LSquare = 4, RSquare = 5;
		public const byte Comma = 6, Dot = 7, Semi = 8;

		// One or two
		public const byte Minus = 20, MinusEqual = 21;
		public const byte Plus = 22,  PlusEqual = 23;
		public const byte Slash = 24, SlashEqual = 25;
		public const byte Star = 26,  StarEqual = 27;

		// One or two
		public const byte Bang = 40,   BangEqual = 41;
		public const byte Equal = 42,  EqualEqual = 43;

		public const byte LAngle = 44, LAngleEqual = 45, LAngleAngle = 46;
		public const byte RAngle = 47, RAngleEqual = 48, RAngleAngle = 49;
		public const byte And = 50,    AndEqual = 54,    AndAnd = 51;
		public const byte Or = 52,     OrEqual = 55,     OrOr = 53;

		// Literal
		public const byte Id = 60, String = 61, Number = 62;

		// Keywords
		public const byte True = 70, False = 71, Null = 72;
		public const byte If = 73, Else = 74;
		public const byte While = 75, Do = 76, For = 77;
		public const byte Return = 78, Break = 79, Continue = 80;
		public const byte Var = 81, Function = 82;
		public const byte Print = 83;

		// Eof
		public const byte Eof = 255;
	}

	public class UnexpectedCharacterException : System.Exception {
		public UnexpectedCharacterException(char c) : base("Lexer: Unexpected character '" + c + "'") {}
	}

	public class UnrecognizedTokenException : System.Exception {
		public UnrecognizedTokenException(byte t) : base("Lexer: Unrecognized token #" + t) {}
	}

	public static class Lexer {
		static readonly System.Collections.Generic.Dictionary<string, byte> keywords = new System.Collections.Generic.Dictionary<string, byte>();

		static Lexer() {
			keywords.Add("true",     Token.True);
			keywords.Add("false",    Token.False);
			keywords.Add("null",     Token.Null);
			keywords.Add("if",       Token.If);
			keywords.Add("else",     Token.Else);
			keywords.Add("while",    Token.While);
			keywords.Add("do",       Token.Do);
			keywords.Add("for",      Token.For);
			keywords.Add("return",   Token.Return);
			keywords.Add("break",    Token.Break);
			keywords.Add("continue", Token.Continue);
			keywords.Add("var",      Token.Var);
			keywords.Add("func", Token.Function);
			keywords.Add("function", Token.Function);
			keywords.Add("print",    Token.Print);
			keywords.Add("eof",      Token.Eof);
		}

		static string text;
		static int pos, length;

		public static readonly string[] strings = new string[256];
		public static readonly double[] numbers = new double[256];
		public static ushort scount, ncount;

		public static readonly byte[] tokens = new byte[1024];
		public static ushort tcount;

		static void Add(byte t) {
			tokens[tcount++] = t;
		}

		static void Add(byte t, string s) {
			tokens[tcount++] = t;
			tokens[tcount++] = (byte)(scount & 0xff);
			tokens[tcount++] = (byte)(scount >> 8);
			strings[scount++] = s;
		}

		static void Add(byte t, double n) {
			tokens[tcount++] = t;
			tokens[tcount++] = (byte)(ncount & 0xff);
			tokens[tcount++] = (byte)(ncount >> 8);
			numbers[ncount++] = n;
		}

		static bool Match(char c) {
			if (text[pos] == c) { pos += 1; return true; }
			return false;
		}

		static bool IsDigit(char c) {
			return '0' <= c && c <= '9';
		}

		static bool IsAlpha(char c) {
			return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_';
		}

		public static void Scan(string text) {
			Lexer.text = text; length = text.Length; pos = 0; 
//			scount = 0; ncount = 0; tcount = 0;

			while (pos < length) {
				char c = text[pos++];
				switch (c) {
				case '(': Add(Token.LParen); break;
				case ')': Add(Token.RParen); break;
				case '{': Add(Token.LCurly); break;
				case '}': Add(Token.RCurly); break;
				case '[': Add(Token.LSquare); break;
				case ']': Add(Token.RSquare); break;
				case ',': Add(Token.Comma); break;
				case '.': Add(Token.Dot); break;
				case ';': Add(Token.Semi); break;
					
				case '-': Add(Match('=') ? Token.MinusEqual  : Token.Minus); break;
				case '+': Add(Match('=') ? Token.PlusEqual   : Token.Plus); break;
				case '/': Add(Match('=') ? Token.SlashEqual  : Token.Slash); break;
				case '*': Add(Match('=') ? Token.StarEqual   : Token.Star); break;
					
				case '!': Add(Match('=') ? Token.BangEqual   : Token.Bang); break;
				case '=': Add(Match('=') ? Token.EqualEqual  : Token.Equal); break;

				case '<': Add(Match('=') ? Token.LAngleEqual : Match('<') ? Token.LAngleAngle : Token.LAngle); break;
				case '>': Add(Match('=') ? Token.RAngleEqual : Match('>') ? Token.RAngleAngle : Token.RAngle); break;
				case '&': Add(Match('=') ? Token.AndEqual    : Match('&') ? Token.AndAnd :      Token.And); break;
				case '|': Add(Match('=') ? Token.OrEqual     : Match('|') ? Token.OrOr :        Token.Or); break;
				
				case ' ': case '\r': case '\t': case '\n': break;
	
				case '"': case '\'': String(c); break;
				default:
					if (IsDigit(c)) Number();
					else if (IsAlpha(c)) Id();
					else throw new UnexpectedCharacterException(c);
					break;
				}
			}

//			if (tokens[tcount - 1] != Token.Eof) Add(Token.Eof);
		}

		static void Number() {
			int start = pos - 1;
			while (pos < length && IsDigit(text[pos])) pos += 1;
			if (pos + 1 < length && text[pos] == '.' && IsDigit(text[pos + 1])) {
				pos += 1;  // Decimal point
				while (pos < length && IsDigit(text[pos])) pos += 1;
			}
			Add(Token.Number, double.Parse(text.Substring(start, pos - start)));
		}

		static void String(char quote) {
			int start = pos;
			while (pos < length && text[pos] != quote) pos += 1;
			Add(Token.String, text.Substring(start, pos - start));
			pos += 1;  // Close quote
		}

		static void Id() {
			int start = pos - 1;
			while (pos < length && (IsAlpha(text[pos]) || IsDigit(text[pos]))) pos += 1;
			string id = text.Substring(start, pos - start);
			if (keywords.ContainsKey(id)) Add(keywords[id]);
			else Add(Token.Id, id);
		}

		static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public static string Recant() {
			sb.Clear();

			int i = 0;
			while (i < tcount) {
				switch (tokens[i++]) {
				case Token.LParen:      sb.Append("("); break;
				case Token.RParen:      sb.Append(")"); break;
				case Token.LCurly:      sb.Append("{"); break;
				case Token.RCurly:      sb.Append("}"); break;
				case Token.LSquare:     sb.Append("["); break;
				case Token.RSquare:     sb.Append("]"); break;
				case Token.Comma:       sb.Append(","); break;
				case Token.Dot:         sb.Append("."); break;
				case Token.Semi:        sb.Append(";"); break;

				case Token.Minus:       sb.Append("-"); break;
				case Token.MinusEqual:  sb.Append("-="); break;
				case Token.Plus:        sb.Append("+"); break;
				case Token.PlusEqual:   sb.Append("+="); break;
				case Token.Slash:       sb.Append("/"); break;
				case Token.SlashEqual:  sb.Append("/="); break;
				case Token.Star:        sb.Append("*"); break;
				case Token.StarEqual:   sb.Append("*="); break;

				case Token.Bang:        sb.Append("!"); break;
				case Token.BangEqual:   sb.Append("!="); break;
				case Token.Equal:       sb.Append("="); break;
				case Token.EqualEqual:  sb.Append("=="); break;

				case Token.LAngle:      sb.Append("<"); break;
				case Token.LAngleEqual: sb.Append("<="); break;
				case Token.LAngleAngle: sb.Append("<<"); break;
				case Token.RAngle:      sb.Append(">"); break;
				case Token.RAngleEqual: sb.Append(">="); break;
				case Token.RAngleAngle: sb.Append(">>"); break;
				case Token.And:         sb.Append("&"); break;
				case Token.AndEqual:    sb.Append("&="); break;
				case Token.AndAnd:      sb.Append("&&"); break;
				case Token.Or:          sb.Append("|"); break;
				case Token.OrEqual:     sb.Append("|="); break;
				case Token.OrOr:        sb.Append("||"); break;

				case Token.Id:          sb.Append(strings[tokens[i++] | tokens[i++] << 8]); break;
				case Token.String:      sb.Append("'"); sb.Append(strings[tokens[i++] | tokens[i++] << 8]); sb.Append("'"); break;
				case Token.Number:      sb.Append(numbers[tokens[i++] | tokens[i++] << 8]); break;

				case Token.True:        sb.Append("true"); break;
				case Token.False:       sb.Append("false"); break;
				case Token.Null:        sb.Append("null"); break;
				case Token.If:          sb.Append("if"); break;
				case Token.Else:        sb.Append("else"); break;
				case Token.While:       sb.Append("while"); break;
				case Token.Do:          sb.Append("do"); break;
				case Token.For:         sb.Append("for"); break;
				case Token.Return:      sb.Append("return"); break;
				case Token.Break:       sb.Append("break"); break;
				case Token.Continue:    sb.Append("continue"); break;
				case Token.Var:         sb.Append("var"); break;
				case Token.Function:    sb.Append("function"); break;
				case Token.Print:       sb.Append("print"); break;
				case Token.Eof:         sb.Append("eof"); break;
					
				default:
					throw new UnrecognizedTokenException(tokens[i - 1]);
				}

				sb.Append(" ");
			}

			sb.Append("\n");
			return sb.ToString();
		}
	}
}
