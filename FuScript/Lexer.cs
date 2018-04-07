using System.Text;
using System.Collections.Generic;

namespace FuScript {
	public sealed class Token {
		// Single character
		public const byte LParen = 0, RParen = 1, LCurly = 2, RCurly = 3;
		public const byte Comma = 4, Dot = 5, Minus = 6, Plus = 7, Semi = 8, Slash = 9, Star = 10;

		// One or two characters
		public const byte Bang = 11, BangEqual = 12;
		public const byte Equal = 36, EqualEqual = 37;
		public const byte LAngle = 13, LAngleEqual = 14;
		public const byte RAngle = 15, RAngleEqual = 16;
		public const byte BAnd = 38, And = 39;
		public const byte BOr = 40, Or = 41;

		// Literals
		public const byte Id = 17, String = 18, Number = 19;

		// Keywords
//		public const byte KAnd = 20, KClass = 21, KElse = 22, KFalse = 23, KFunc = 24, KFor = 25, KIf = 26, KNull = 27, KOr = 28;
		public const byte KClass = 21, KElse = 22, KFalse = 23, KFunc = 24, KFor = 25, KIf = 26, KNull = 27;
		public const byte KPrint = 29, KReturn = 30, KSuper = 31, KThis = 32, KTrue = 33, KVar = 34, KWhile = 35;

		// EOF
		public const byte Eof = 255;

		public int type;
		public string stringLiteral;
		public float numberLiteral;
	}

	public static class Lexer {
		static string _text;
		static int _pos, _length;

		static readonly List<Token> _list = new List<Token>();
		static readonly StringBuilder _sb = new StringBuilder();
		static readonly Dictionary<string, int> _keywords = new Dictionary<string, int>();

		static Lexer() {    
			_keywords.Add("and",      Token.And);
			_keywords.Add("class",    Token.KClass);
			_keywords.Add("else",     Token.KElse);
			_keywords.Add("false",    Token.KFalse);
			_keywords.Add("for",      Token.KFor);
			_keywords.Add("function", Token.KFunc);
			_keywords.Add("if",       Token.KIf);
			_keywords.Add("null",     Token.KNull);
			_keywords.Add("or",       Token.Or);
			_keywords.Add("print",    Token.KPrint);
			_keywords.Add("return",   Token.KReturn);
			_keywords.Add("super",    Token.KSuper);
			_keywords.Add("this",     Token.KThis);
			_keywords.Add("true",     Token.KTrue);
			_keywords.Add("var",      Token.KVar);
			_keywords.Add("while",    Token.KWhile);
		}

		static void Add(int type) {
			_list.Add(new Token{type = type});
		}

		static void Add(int type, string stringLiteral) {
			_list.Add(new Token{type = type, stringLiteral = stringLiteral});
		}
		static void Add(int type, float numberLiteral) {
			_list.Add(new Token{type = type, numberLiteral = numberLiteral});
		}

		static bool Match(char c) {
			if (_text[_pos] != c) return false;
			_pos += 1;
			return true;
		}

		static void String(char quote) {
			int start = _pos;
			while (_pos < _length && _text[_pos] != quote) _pos += 1;
			Add(Token.String, _text.Substring(start, _pos - start));
			_pos += 1;  // Close quote
		}

		static bool IsDigit(char c) {
			return '0' <= c && c <= '9';
		}

		static bool IsAlpha(char c) {
			return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_';
		}

		static void Number() {
			int start = _pos - 1;
			while (_pos < _length && IsDigit(_text[_pos])) _pos += 1;
			if (_pos + 1 < _length && _text[_pos] == '.' && IsDigit(_text[_pos + 1])) {
				_pos += 1;  // Decimal point
				while (_pos < _length && IsDigit(_text[_pos])) _pos += 1;
			}
			Add(Token.Number, float.Parse(_text.Substring(start, _pos - start)));
		}

		static void Id() {
			int start = _pos - 1;
			while (_pos < _length && (IsAlpha(_text[_pos]) || IsDigit(_text[_pos]))) _pos += 1;

			string id = _text.Substring(start, _pos - start);
			if (_keywords.ContainsKey(id)) Add(_keywords[id]);
			else Add(Token.Id, id);
		}

		public static Token[] Scan(string text) {
			_text = text;
			_pos = 0;
			_length = text.Length;
			_list.Clear();

			while (_pos < _length) {
				char c = _text[_pos++];
				switch (c) {
				case '(': Add(Token.LParen); break;
				case ')': Add(Token.RParen); break;
				case '{': Add(Token.LCurly); break;
				case '}': Add(Token.RCurly); break;
				case ',': Add(Token.Comma); break;
				case '.': Add(Token.Dot); break;
				case '-': Add(Token.Minus); break;
				case '+': Add(Token.Plus); break;
				case ';': Add(Token.Semi); break;
				case '*': Add(Token.Star); break;
				case '!': Add(Match('=') ? Token.BangEqual : Token.Bang); break;
				case '=': Add(Match('=') ? Token.EqualEqual : Token.Equal); break;
				case '<': Add(Match('=') ? Token.LAngleEqual : Token.LAngle); break;
				case '>': Add(Match('=') ? Token.RAngleEqual : Token.RAngle); break;
				case '&': Add(Match('&') ? Token.And : Token.BAnd); break;
				case '|': Add(Match('|') ? Token.Or : Token.BOr); break;
				case ' ':
				case '\r':
				case '\t':
				case '\n':
					break;
				case '"':
				case '\'':
					String(c); break;
				default:
					if (IsDigit(c)) Number();
					else if (IsAlpha(c)) Id();
					else throw new System.Exception("Unexpected character '" + c + "'.");
					break;
				}
			}

			Add(Token.Eof);
			return _list.ToArray();
		}

		public static string Print(Token[] tokens) {
			_sb.Clear();
			int length = tokens.Length;
			for (int i = 0; i < length; i++) {
				switch (tokens[i].type) {
				case Token.LParen:      _sb.Append('('); break;
				case Token.RParen:      _sb.Append(')'); break;
				case Token.LCurly:      _sb.Append('{'); break;
				case Token.RCurly:      _sb.Append('}'); break;
				case Token.Comma:       _sb.Append(','); break;
				case Token.Dot:         _sb.Append('.'); break;
				case Token.Minus:       _sb.Append('-'); break;
				case Token.Plus:        _sb.Append('+'); break;
				case Token.Semi:        _sb.Append(';'); break;
				case Token.Slash:       _sb.Append('/'); break;
				case Token.Star:        _sb.Append('*'); break;
				case Token.Bang:        _sb.Append('!'); break;
				case Token.BangEqual:   _sb.Append("!="); break;
				case Token.Equal:       _sb.Append('='); break;
				case Token.EqualEqual:  _sb.Append("=="); break;
				case Token.LAngle:      _sb.Append('<'); break;
				case Token.LAngleEqual: _sb.Append("<="); break;
				case Token.RAngle:      _sb.Append('>'); break;
				case Token.RAngleEqual: _sb.Append(">="); break;
				case Token.BAnd:        _sb.Append("&"); break;
				case Token.And:         _sb.Append("&&"); break;
				case Token.BOr:         _sb.Append("|"); break;
				case Token.Or:          _sb.Append("||"); break;
				case Token.Id:          _sb.Append(tokens[i].stringLiteral); break;
				case Token.String:      _sb.Append(tokens[i].stringLiteral); break;
				case Token.Number:      _sb.Append(tokens[i].numberLiteral.ToString("N1")); break;
				case Token.KClass:      _sb.Append("CLASS"); break;
				case Token.KElse:       _sb.Append("ELSE"); break;
				case Token.KFalse:      _sb.Append("FALSE"); break;
				case Token.KFunc:       _sb.Append("FUNC"); break;
				case Token.KFor:        _sb.Append("FOR"); break;
				case Token.KIf:         _sb.Append("IF"); break;
				case Token.KNull:       _sb.Append("NULL"); break;
				case Token.KPrint:      _sb.Append("PRINT"); break;
				case Token.KReturn:     _sb.Append("RETURN"); break;
				case Token.KSuper:      _sb.Append("SUPER"); break;
				case Token.KThis:       _sb.Append("THIS"); break;
				case Token.KTrue:       _sb.Append("TRUE"); break;
				case Token.KVar:        _sb.Append("VAR"); break;
				case Token.KWhile:      _sb.Append("WHILE"); break;
				case Token.Eof:         _sb.Append('/'); break;
				default:
					throw new System.Exception("Code path not possible");
				}
				_sb.Append(' ');
			}
			return _sb.ToString();
		}
	}
}

