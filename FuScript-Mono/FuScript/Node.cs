namespace FuScript {
	public static class Node {
		// null | true | false
		public const ushort Null = 1, True = 2, False = 3;

		// ( var | int | float | string ) <literal>
		public const ushort Var = 10, Int = 11, Float = 12, String = 13;

		// ( preInc | preDec | postInc | postDec) node
		public const ushort PreInc = 20, PreDec = 21, PostInc = 22, PostDec = 23;

		// member node <literal> | call [ node : nodes ] | subscript node node
		public const ushort Member = 30, Call = 31, Subscript = 32;

		// ( not | neg | bitNot ) node
		public const ushort Not = 40, Neg = 41, BitNot = 42;

		// ( mod | mul | div | add | sub | bitOr | bitAnd | bitXor | shiftL | shiftR ) node node
		public const ushort Mod = 50, Mul = 51, Div = 52, Add = 53, Sub = 54;
		public const ushort BitOr = 60, BitAnd = 61, BitXor = 62, ShiftL = 63, ShiftR = 64;

		// ( lessThan | greaterThan | lessEqual | greaterEqual | equal | notEqual ) node node
		public const ushort LessThan = 70, GreaterThan = 71, LessEqual = 72, GreaterEqual = 73, Equal = 74, NotEqual = 75;

		// ( and | or ) node node
		public const ushort And = 80, Or = 81;

		// conditional node node node
		public const ushort Conditional = 90;

		// ( assign | assignAdd | assignSub | assignMod | assignMul | assignDiv | assignBitOr | assignBitAnd | assignBitXor | assignShiftL | assignShiftR ) node node
		public const ushort Assign = 100;
		public const ushort AssignMod = 110, AssignMul = 111, AssignDiv = 112, AssignAdd = 113, AssignSub = 114;
		public const ushort AssignBitOr = 120, AssignBitAnd = 121, AssignBitXor = 122, AssignShiftL = 123, AssignShiftR = 124;

		// comma [ nodes ]
		public const ushort Comma = 130;

		// array [ nodes ] | function [ node : nodes ] | object [ [ <literal>, node ] ]
		public const ushort Array = 140, Function = 141, Object = 142;

		// exprStmt node | whileStmt node node | ifStmt node node node | varDecl [ [ <literal>, node ] ]
		public const ushort ExprStmt = 150, WhileStmt = 151, IfStmt = 152, VarDecl = 153;

		// block [ nodes ]
		public const ushort Block = 160;

		// break | continue | return node | noOp
		public const ushort Break = 170, Continue = 171, Return = 172, NoOp = 173;
	}
}
