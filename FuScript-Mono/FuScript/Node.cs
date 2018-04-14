namespace FuScript {
	public static class Node {
		public const ushort Null = 1, True = 2, False = 3, Var = 4, Int = 5, Float = 6, String = 7;

		public const ushort Member = 10, Call = 11, Subscript = 12;
		public const ushort Object = 13, Array = 14, Function = 15;

		public const ushort Not = 40, And = 41, Or = 42, Conditional = 43, Assign = 44;

		public const ushort BitNot = 20, BitOr = 21, BitAnd = 22, BitXor = 23, ShiftLeft = 24, ShiftRight = 25;
		public const ushort AssignBitNot = 30, AssignBitOr = 31, AssignBitAnd = 32, AssignBitXor = 33, AssignShiftLeft = 34, AssignShiftRight = 35;

		public const ushort Neg = 60, Mod = 61, Mul = 62, Div = 63, Add = 64, Sub = 65;
		public const ushort AssignMod = 51, AssignMul = 52, AssignDiv = 53, AssignAdd = 54, AssignSub = 55;

		public const ushort LessThan = 70, GreaterThan = 71, LessEqual = 72, GreaterEqual = 73, Equal = 74, NotEqual = 75;

		public const ushort PreInc = 80, PreDec = 81, PostInc = 82, PostDec = 83;

		// stmt
		public const ushort Expression = 100;
		public const ushort Block = 200, VarDecl = 201, FunctionDecl = 202;
	}
}
