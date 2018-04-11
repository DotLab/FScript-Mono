namespace FuScript.Third {
	public static class Opcode {
		public const byte BinarySubtract = 2, BinaryAdd = 3, BinaryDivide = 4, BinaryMultiply = 5;
		public const byte BinaryLogicOr = 27, BinaryLogicAnd = 28, BinaryEqual = 29, BinaryNotEqual = 30;
		public const byte BinaryLess = 31, BinaryLessEqual = 32, BinaryGreater = 33, BinaryGreaterEqual = 34;

		public const byte UnaryNot = 6, UnaryNegative = 7;

		public const byte PushNumber = 8, PushString = 20, PushSmallInt = 21;
		public const byte PopNewVar = 17, PopVar = 9, PushVar = 10, PopDiscard = 24, PeekVar = 25;

		public const byte CloneEnv = 11, RestoreEnv = 12;
		public const byte Print = 13;

		public const byte PushConstNull = 14, PushConstFalse = 15, PushConstTrue = 16;

		public const byte Jump = 18, Return = 19;
		public const byte MakeFunction = 22, CallFunction = 23;

		public const byte BranchIfFalsy = 26;

		public const byte ObjectMemberGet = 35, ObjectMemberSet = 36, ObjectMemberDoSet = 37, MakeObject = 38, MakeObjectMember = 39;
		// 39
	}
}

