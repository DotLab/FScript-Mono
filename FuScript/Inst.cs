namespace FuScript {
	public static class Inst {
		public const byte Return = 0;

		// 
		public const byte MoveConstToReg = 1;

		public const byte BinarySubtract = 2, BinaryAdd = 3, BinaryDivide = 4, BinaryMultiply = 5;
		public const byte UnaryNot = 6, UnaryNegative = 7;

		public const byte PushConst = 8;
		public const byte PopNewVar = 17, PopVar = 9, PushVar = 10;

		public const byte CloneEnv = 11, RestoreEnv = 12;
		public const byte Print = 13;

		public const byte PushConstNull = 14, PushConstFalse = 15, PushConstTrue = 16;

		// 17
	}
}

