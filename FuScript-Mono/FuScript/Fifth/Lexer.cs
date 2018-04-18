namespace FuScript.Fifth {
	public class UnexpectedCharacterException : System.Exception {
		public UnexpectedCharacterException(char c) : base("Unexpected character '" + c + "'") { }
	}

	public static class Lexer {
		static readonly System.Collections.Generic.Dictionary<string, ushort> keywords = new System.Collections.Generic.Dictionary<string, ushort>();

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

		static readonly System.Collections.Generic.Dictionary<int, ushort>    intDict = new System.Collections.Generic.Dictionary<int, ushort>();
		static readonly System.Collections.Generic.Dictionary<float, ushort>  floatDict = new System.Collections.Generic.Dictionary<float, ushort>();
		static readonly System.Collections.Generic.Dictionary<string, ushort> stringDict = new System.Collections.Generic.Dictionary<string, ushort>();
		public static readonly int[]    ints = new int[256];
		public static readonly float[]  floats =  new float[256];
		public static readonly string[] strings = new string[256];
		public static ushort intCount, floatCount, stringCount;

		public static readonly ushort[] tokens = new ushort[1024];
		public static ushort tokenCount;

		static void Log(string str) {
			System.Console.Write(str);
		}

		static void Add(ushort t) {
			tokens[tokenCount++] = t;
		}

		static void Add(ushort t, int i) {
			tokens[tokenCount++] = t;

			if (!intDict.ContainsKey(i)) {
				ints[intCount] = i;
				intDict[i] = intCount++;
			}

			tokens[tokenCount++] = intDict[i];
		}

		static void Add(ushort t, float f) {
			tokens[tokenCount++] = t;

			if (!floatDict.ContainsKey(f)) {
				floats[floatCount] = f;
				floatDict[f] = floatCount++;
			}

			tokens[tokenCount++] = floatDict[f];			
		}

		static void Add(ushort t, string s) {
			tokens[tokenCount++] = t;

			if (!stringDict.ContainsKey(s)) {
				strings[stringCount] = s;
				stringDict[s] = stringCount++;
			}

			tokens[tokenCount++] = stringDict[s];
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

		public static void Reset() {
			stringCount = 0; intCount = 0; floatCount = 0; tokenCount = 0;
			pos = 0;
		}

		public static void Scan(string text) {
			Lexer.text = text; length = text.Length; 
			pos = 0;

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
					case ':': Add(Token.Colon); break;
					case '?': Add(Token.Quest); break;
					case '~': Add(Token.Tilde); break;
					case '#': pos += 1; while (text[pos - 1] != '\n') pos += 1; break;

					case '+': Add(Match('=') ? Token.PlusEqual   : Match('+') ? Token.PlusPlus   : Token.Plus); break;
					case '-': Add(Match('=') ? Token.MinusEqual  : Match('-') ? Token.MinusMinus : Token.Minus); break;
					case '*': Add(Match('=') ? Token.StarEqual   : Token.Star); break;
					case '^': Add(Match('=') ? Token.CaretEqual  : Token.Caret); break;
					case '%': Add(Match('=') ? Token.ModEqual    : Token.Mod); break;

					case '!': Add(Match('=') ? Token.BangEqual   : Token.Bang); break;
					case '=': Add(Match('=') ? Token.EqualEqual  : Token.Equal); break;

					case '&': Add(Match('=') ? Token.AndEqual    : Match('&') ? Token.AndAnd : Token.And); break;
					case '|': Add(Match('=') ? Token.OrEqual     : Match('|') ? Token.OrOr :   Token.Or); break;
						
					case '<': Add(Match('=') ? Token.LAngleEqual : Match('<') ? Match('=') ? Token.LAngleAngleEqual : Token.LAngleAngle : Token.LAngle); break;
					case '>': Add(Match('=') ? Token.RAngleEqual : Match('>') ? Match('=') ? Token.RAngleAngleEqual : Token.RAngleAngle : Token.RAngle); break;

					case ' ': case '\r': case '\t': case '\n': break;

					case '"': case '\'': String(c); break;
						
					case '/':
						if      (Match('/')) { pos += 1; while (text[pos - 1] != '\n') pos += 1; } 
						else if (Match('*')) { pos += 2; while (text[pos - 2] == '*' && text[pos - 1] == '/') pos += 1; }
						else Add(Match('=') ? Token.SlashEqual  : Token.Slash); 
						break;
						
					default:
						if      (IsDigit(c)) Number();
						else if (IsAlpha(c)) Id();
						else throw new UnexpectedCharacterException(c);
						break;
				}
			}

			if (tokens[tokenCount - 1] != Token.Eof) tokens[tokenCount] = Token.Eof;
		}

		static void Number() {
			int start = pos - 1;
			while (pos < length && IsDigit(text[pos])) pos += 1;
			bool isInt = true;
			if (pos + 1 < length && text[pos] == '.' && IsDigit(text[pos + 1])) {
				isInt = false;
				pos += 1;  // Decimal point
				while (pos < length && IsDigit(text[pos])) pos += 1;
			}

			if (isInt) Add(Token.Int,   int.Parse(text.Substring(start, pos - start)));
			else       Add(Token.Float, float.Parse(text.Substring(start, pos - start)));
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

		public static string Recant() {
			var sb = new System.Text.StringBuilder();

			int i = 0;
			while (i < tokenCount) {
				switch (tokens[i++]) {
					case Token.Int:         sb.Append(ints[tokens[i++]]); break;
					case Token.Float:       sb.Append(floats[tokens[i++]]); break;
					case Token.Id:          sb.Append(strings[tokens[i++]]); break;
					case Token.String:      sb.Append('"'); sb.Append(strings[tokens[i++]]); sb.Append('"'); break;

					default:
						sb.Append(Token.Recant(tokens[i - 1]));
						break;
				}
				sb.Append(' ');
			}
			
			return sb.ToString();
		}
	}
}
