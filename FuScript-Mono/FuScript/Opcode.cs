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
		public const byte BIN_BIT_SHL = 39, BIN_BIT_SHR = 40, BIN_BIT_AND = 41, BIN_BIT_OR = 42, BIN_BIT_XOR = 43;

		public const byte POP_INT = 50, POP_FLO = 51, POP_BOO = 52, POP_STR = 53;

		public const byte POP_NEW_VAR_INT = 60, POP_NEW_VAR_FLO = 61, POP_NEW_VAR_BOO = 62, POP_NEW_VAR_STR = 63, POP_NEW_VAR_NIL = 64;
		public const byte PUSH_VAR_INT = 65, PUSH_VAR_FLO = 66, PUSH_VAR_BOO = 67, PUSH_VAR_STR = 68, PUSH_VAR_NIL = 69;
	}
}
