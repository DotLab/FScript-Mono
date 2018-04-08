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
		public const byte LAngle = 13, LAngleEqual = 14, LAngleAngle = 42;
		public const byte RAngle = 15, RAngleEqual = 16, RAngleAngle = 43;
		public const byte And = 38, AndAnd = 39;
		public const byte Or = 40, OrOr = 41;

		// Literals
		public const byte Id = 17, String = 18, Number = 19;

		// Keywords
		public const byte KClass = 21, KElse = 22, KFalse = 23, KFunc = 24, KFor = 25, KIf = 26, KNull = 27;
		public const byte KPrint = 29, KReturn = 30, KSuper = 31, KThis = 32, KTrue = 33, KVar = 34, KWhile = 35;

		// EOF
		public const byte Eof = 255;

		public readonly byte type;
		public readonly string stringLiteral;
		public readonly float numberLiteral;

		public Token(byte type) {
			this.type = type;
		}

		public Token(byte type, string stringLiteral) {
			this.type = type;
			this.stringLiteral = stringLiteral;
		}

		public Token(byte type, float numberLiteral) {
			this.type = type;
			this.numberLiteral = numberLiteral;
		}

		public override string ToString() {
			switch (type) {
			case Token.LParen:      return "(";
			case Token.RParen:      return ")";
			case Token.LCurly:      return "{";
			case Token.RCurly:      return "}";
			case Token.Comma:       return ",";
			case Token.Dot:         return ".";
			case Token.Minus:       return "-";
			case Token.Plus:        return "+";
			case Token.Semi:        return ";";
			case Token.Slash:       return "/";
			case Token.Star:        return "*";
			case Token.Bang:        return "!";
			case Token.BangEqual:   return "!=";
			case Token.Equal:       return "=";
			case Token.EqualEqual:  return "==";
			case Token.LAngle:      return "<";
			case Token.LAngleEqual: return "<=";
			case Token.LAngleAngle: return "<<";
			case Token.RAngle:      return ">";
			case Token.RAngleEqual: return ">=";
			case Token.RAngleAngle: return ">>";
			case Token.And:         return "&";
			case Token.AndAnd:      return "&&";
			case Token.Or:          return "|";
			case Token.OrOr:        return "||";
			case Token.Id:          return stringLiteral;
			case Token.String:      return "'" + stringLiteral + "'";
			case Token.Number:      return numberLiteral.ToString();
			case Token.KClass:      return "CLASS";
			case Token.KElse:       return "ELSE";
			case Token.KFalse:      return "FALSE";
			case Token.KFunc:       return "FUNC";
			case Token.KFor:        return "FOR";
			case Token.KIf:         return "IF";
			case Token.KNull:       return "NULL";
			case Token.KPrint:      return "PRINT";
			case Token.KReturn:     return "RETURN";
			case Token.KSuper:      return "SUPER";
			case Token.KThis:       return "THIS";
			case Token.KTrue:       return "TRUE";
			case Token.KVar:        return "VAR";
			case Token.KWhile:      return "WHILE";
			case Token.Eof:         return "#";
			default:
				throw new System.Exception("Code path not possible");
			}
		}
	}

	public static class Lexer {
		static string _text;
		static int _pos, _length;

		static readonly List<Token> _list = new List<Token>();
		static readonly StringBuilder _sb = new StringBuilder();
		static readonly Dictionary<string, byte> _keywords = new Dictionary<string, byte>();

		static Lexer() {    
			_keywords.Add("and",      Token.AndAnd);
			_keywords.Add("class",    Token.KClass);
			_keywords.Add("else",     Token.KElse);
			_keywords.Add("false",    Token.KFalse);
			_keywords.Add("for",      Token.KFor);
			_keywords.Add("fun", Token.KFunc);
			_keywords.Add("function", Token.KFunc);
			_keywords.Add("if",       Token.KIf);
			_keywords.Add("null",     Token.KNull);
			_keywords.Add("or",       Token.OrOr);
			_keywords.Add("print",    Token.KPrint);
			_keywords.Add("return",   Token.KReturn);
			_keywords.Add("super",    Token.KSuper);
			_keywords.Add("this",     Token.KThis);
			_keywords.Add("true",     Token.KTrue);
			_keywords.Add("var",      Token.KVar);
			_keywords.Add("while",    Token.KWhile);
		}

		static void Add(byte type) {
			_list.Add(new Token(type));
		}

		static void Add(byte type, string stringLiteral) {
			_list.Add(new Token(type, stringLiteral));
		}
		static void Add(byte type, float numberLiteral) {
			_list.Add(new Token(type, numberLiteral));
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
				case '/': Add(Token.Slash); break;
				case '*': Add(Token.Star); break;
				case '!': Add(Match('=') ? Token.BangEqual : Token.Bang); break;
				case '=': Add(Match('=') ? Token.EqualEqual : Token.Equal); break;
				case '<': Add(Match('=') ? Token.LAngleEqual : Match('<') ? Token.LAngleAngle : Token.LAngle); break;
				case '>': Add(Match('=') ? Token.RAngleEqual : Match('>') ? Token.LAngleAngle : Token.RAngle); break;
				case '&': Add(Match('&') ? Token.AndAnd : Token.And); break;
				case '|': Add(Match('|') ? Token.OrOr : Token.Or); break;
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
					else throw new System.Exception("Unexpected character " + c);
					break;
				}
			}

			Add(Token.Eof);
			return _list.ToArray();
		}

		public static string ToString(Token[] tokens) {
			_sb.Clear();
			int length = tokens.Length;
			for (int i = 0; i < length; i++) {
				_sb.Append(tokens[i].ToString());
				_sb.Append(' ');
			}
			return _sb.ToString();
		}
	}
}

