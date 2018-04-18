namespace FuScript {
	public class UnexpectedCharacterException : System.Exception {
		public UnexpectedCharacterException(char c) : base("Unexpected character '" + c + "'") { }
	}

	public static class Lexer {
		static readonly System.Collections.Generic.Dictionary<string, byte> keywords = new System.Collections.Generic.Dictionary<string, byte>();

		static Lexer() {
			keywords.Add("true",     Token.KTrue);
			keywords.Add("false",    Token.KFalse);
			keywords.Add("null",     Token.KNull);
			keywords.Add("if",       Token.KIf);
			keywords.Add("else",     Token.KElse);
			keywords.Add("while",    Token.KWhile);
			keywords.Add("do",       Token.KDo);
			keywords.Add("for",      Token.KFor);
			keywords.Add("return",   Token.KReturn);
			keywords.Add("break",    Token.KBreak);
			keywords.Add("continue", Token.KContinue);
			keywords.Add("var",      Token.KVar);
			keywords.Add("function", Token.KFunction);
			keywords.Add("print",    Token.KPrint);

			keywords.Add("eof",      Token.Eof);

			keywords.Add("and",      Token.AndAnd);
			keywords.Add("or",       Token.OrOr);
			keywords.Add("xor",      Token.Caret);

			keywords.Add("begin",    Token.LCurly);
			keywords.Add("end",      Token.RCurly);
			keywords.Add("fun",      Token.KFunction);
			keywords.Add("func",     Token.KFunction);

			keywords.Add("let",      Token.KVar);
			keywords.Add("const",    Token.KVar);
			keywords.Add("auto",     Token.KVar);
			keywords.Add("int",      Token.KVar);
			keywords.Add("long",     Token.KVar);
			keywords.Add("float",    Token.KVar);
			keywords.Add("double",   Token.KVar);
		}

		static string text;
		static int pos, length;

		static byte token;
		static string str; static float num;

		static void Log(string msg) {
			System.Console.Write(msg);
		}

		static void Emit(byte t) {
			token = t;
			str = null; num = 0;
		}

		static void Emit(byte t, float n) {
			token = t;
			str = null; num = n;		
		}

		static void Emit(byte t, string s) {
			token = t;
			str = s; num = 0;
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

		public static void Reset(string text) {
			Lexer.text = text; length = text.Length; pos = 0;
		}

		static void Next() {
			reentry:
			if (pos >= length) {
				Emit(Token.Eof);
				return;
			}

			char c = text[pos++];
			switch (c) {
				case '(': Emit(Token.LParen); break;
				case ')': Emit(Token.RParen); break;
				case '{': Emit(Token.LCurly); break;
				case '}': Emit(Token.RCurly); break;
				case '[': Emit(Token.LSquare); break;
				case ']': Emit(Token.RSquare); break;
				case ',': Emit(Token.Comma); break;
				case '.': Emit(Token.Dot); break;
				case ';': Emit(Token.Semi); break;
				case ':': Emit(Token.Colon); break;
				case '?': Emit(Token.Quest); break;
				case '~': Emit(Token.Tilde); break;
				case '#': pos += 1; while (text[pos - 1] != '\n') pos += 1; break;

				case '+': Emit(Match('=') ? Token.PlusEqual   : Match('+') ? Token.PlusPlus   : Token.Plus); break;
				case '-': Emit(Match('=') ? Token.MinusEqual  : Match('-') ? Token.MinusMinus : Token.Minus); break;
				case '*': Emit(Match('=') ? Token.StarEqual   : Token.Star); break;
				case '^': Emit(Match('=') ? Token.CaretEqual  : Token.Caret); break;
				case '%': Emit(Match('=') ? Token.ModEqual    : Token.Mod); break;

				case '!': Emit(Match('=') ? Token.BangEqual   : Token.Bang); break;
				case '=': Emit(Match('=') ? Token.EqualEqual  : Token.Equal); break;

				case '&': Emit(Match('=') ? Token.AndEqual    : Match('&') ? Token.AndAnd : Token.And); break;
				case '|': Emit(Match('=') ? Token.OrEqual     : Match('|') ? Token.OrOr :   Token.Or); break;
					
				case '<': Emit(Match('=') ? Token.LAngleEqual : Match('<') ? Match('=') ? Token.LAngleAngleEqual : Token.LAngleAngle : Token.LAngle); break;
				case '>': Emit(Match('=') ? Token.RAngleEqual : Match('>') ? Match('=') ? Token.RAngleAngleEqual : Token.RAngleAngle : Token.RAngle); break;

				case ' ': case '\r': case '\t': case '\n': goto reentry;

				case '"': case '\'': String(c); break;
					
				case '/':
					if (Match('/')) { pos += 1; while (text[pos - 1] != '\n') pos += 1; goto reentry; } 
					if (Match('*')) { pos += 2; while (text[pos - 2] == '*' && text[pos - 1] == '/') pos += 1; goto reentry; }
					Emit(Match('=') ? Token.SlashEqual  : Token.Slash); 
					break;
					
				default:
					if      (IsDigit(c)) Number();
					else if (IsAlpha(c)) Id();
					else throw new UnexpectedCharacterException(c);
					break;
			}
		}

		static void Number() {
			int start = pos - 1;
			while (pos < length && IsDigit(text[pos])) pos += 1;
			if (pos + 1 < length && text[pos] == '.' && IsDigit(text[pos + 1])) {
				pos += 1;  // Decimal point
				while (pos < length && IsDigit(text[pos])) pos += 1;
			}
			Emit(Token.Float, float.Parse(text.Substring(start, pos - start)));
		}

		static void String(char quote) {
			int start = pos;
			while (pos < length && text[pos] != quote) pos += 1;
			Emit(Token.String, text.Substring(start, pos - start));
			pos += 1;  // Close quote
		}

		static void Id() {
			int start = pos - 1;
			while (pos < length && (IsAlpha(text[pos]) || IsDigit(text[pos]))) pos += 1;
			string id = text.Substring(start, pos - start);
			if (keywords.ContainsKey(id)) Emit(keywords[id]); else Emit(Token.Id, id);
		}
	}
}
