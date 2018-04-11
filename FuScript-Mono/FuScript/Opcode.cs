using System;
namespace FuScript {
	public static class Opcode {
		public const byte PUSH_CONST_INT = 1, PUSH_CONST_FLOAT = 2, PUSH_CONST_STRING = 3, PUSH_NULL = 4, PUSH_TRUE = 5, PUSH_FALSE = 6;
		public const byte UNARY_NOT = 10, UNARY_NEG_INT = 11, UNARY_NEG_FLOAT = 12;

		public const byte BIN_DIV_INT = 20, BIN_DIV_FLOAT = 21, BIN_MUL_INT = 22, BIN_MUL_FLOAT = 23;
	}
}
