using System;
namespace FuScript {
	public static class Opcode {
		public const byte PUSH_CONST_INT = 1, PUSH_CONST_FLO = 2, PUSH_CONST_STR = 3, PUSH_NULL = 4, PUSH_TRUE = 5, PUSH_FALSE = 6;
		public const byte UNARY_NOT = 10, UNARY_NEG_INT = 11, UNARY_NEG_FLO = 12;

		public const byte BIN_DIV_INT = 20, BIN_DIV_FLO = 21, BIN_MUL_INT = 22, BIN_MUL_FLO = 23;
		public const byte BIN_SUB_INT = 24, BIN_SUB_FLO = 25, BIN_ADD_INT = 26, BIN_ADD_FLO = 27, BIN_ADD_STR = 28;

		public const byte BIN_LT_INT = 29, BIN_LT_FLO = 30, BIN_LEQ_INT = 31, BIN_LEQ_FLO = 32;
		public const byte BIN_GT_INT = 29, BIN_GT_FLO = 30, BIN_GEQ_INT = 31, BIN_GEQ_FLO = 32;
		public const byte BIN_EQ_INT = 33, BIN_EQ_FLO = 34, BIN_EQ_BOO = 35, BIN_EQ_STR = 36;

		public const byte BIN_AND = 37, BIN_OR = 38;
	}
}
