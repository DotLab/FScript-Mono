namespace FuScript {
	public static class Inst {
		public const byte Return = 0;

		// 
		public const byte MoveConstToReg = 1;

		public const byte BinarySubtract = 2, BinaryAdd = 3, BinaryDivide = 4, BinaryMultiply = 5;
		public const byte UnaryNot = 6, UnaryNegative = 7;

		public const byte PushConst = 8;
	}
}

