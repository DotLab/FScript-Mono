namespace FuScript {
	public static class Opcode {
		public const int Move = 0;      // Copy a value between registers
		public const int LoadK = 1;     // Load a constant into a register
		public const int LoadBool = 2;  // Load a boolean into a register
		public const int LoadNil = 3;   // Load nil values into a range of registers
		public const int GetUpval = 4;  // Read an upvalue into a register
		public const int GetGlobal = 5; // Read a global variable into a register
		public const int GetTable = 6;  // Read a table element into a register
		public const int SetGlobal = 7; // Write a register value into a global variable
		public const int SetUpval = 8;  // Write a register value into an upvalue
		public const int SetTable = 9;  // Write a register value into a table element
		public const int NewTable = 10; // Create a new table
		public const int Self = 11;     // Prepare an object method for calling
		public const int Add = 12;      // Addition operator
		public const int Sub = 13;      // Subtraction operator
		public const int Mul = 14;      // Multiplication operator
		public const int Div = 15;      // Division operator
		public const int Mod = 16;      // Modulus (remainder) operator
		public const int Pow = 17;      // Exponentiation operator
		public const int Unm = 18;      // Unary minus operator
		public const int Not = 19;      // Logical NOT operator
		public const int Len = 20;      // Length operator
		public const int Concat = 21;   // Concatenate a range of registers
		public const int Jmp = 22;      // Unconditional jump
		public const int Eq = 23;       // Equality test
		public const int Lt = 24;       // Less than test
		public const int Le = 25;       // Less than or equal to test
		public const int Test = 26;     // Boolean test, with conditional jump
		public const int TestSet = 27;  // Boolean test, with conditional jump and assignment
		public const int Call = 28;     // Call a closure
		public const int TailCall = 29; // Perform a tail call
		public const int Return = 30;   // Return from function call
		public const int ForLoop = 31;  // Iterate a numeric for loop
		public const int ForPrep = 32;  // Initialization for a numeric for loop
		public const int TForLoop = 33; // Iterate a generic for loop
		public const int SetList = 34;  // Set a range of array elements for a table
		public const int Close = 35;    // Close a range of locals being used as upvalues
		public const int Closure = 36;  // Create a closure of a function prototype
		public const int VarArg = 37;   // Assign vararg function arguments to registers

		public const int OpcodeMask = 0b111111;
		public const int AShift = 6, AMask = 0b11111111;
		public const int CShift = 14, CMask = 0b111111111;
		public const int BShift = 23, BMask = 0b111111111;
		public const int BxShift = 14, BxMask = 0b111111111111111111, BxMaskS = 0b11111111111111111;
		public const int SBxMask = 1 << 31;
	}
}
