namespace FuScript {
	public static class Opcode {
		public const byte BinarySubtract = 2, BinaryAdd = 3, BinaryDivide = 4, BinaryMultiply = 5;
		public const byte UnaryNot = 6, UnaryNegative = 7;

		public const byte PushNumber = 8, PushString = 20, PushSmallInt = 21;
		public const byte PopNewVar = 17, PopVar = 9, PushVar = 10;

		public const byte CloneEnv = 11, RestoreEnv = 12;
		public const byte Print = 13;

		public const byte PushConstNull = 14, PushConstFalse = 15, PushConstTrue = 16;

		public const byte Jump = 18, Return = 19;
		public const byte MakeFunction = 22, CallFunction = 23;
		// 23
	}
}

