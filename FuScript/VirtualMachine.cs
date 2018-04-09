namespace FuScript {
	public static class VirtualMachine {
		static int pc, length;

		static readonly double[] stack = new double[512];
		static int sp;

		public static void Init() {
			length = Compiler.icount;
			pc = 0; sp = -1;
		}

		public static void Run() {
			byte inst;
			while (pc < length) {
				switch (inst = Compiler.insts[pc++]) {
				case Inst.BinarySubtract: sp -= 1; stack[sp] -= stack[sp + 1]; break;
				case Inst.BinaryAdd:      sp -= 1; stack[sp] += stack[sp + 1]; break;
				case Inst.BinaryDivide:   sp -= 1; stack[sp] /= stack[sp + 1]; break;
				case Inst.BinaryMultiply: sp -= 1; stack[sp] *= stack[sp + 1]; break;

				case Inst.UnaryNot:       stack[sp] = -stack[sp]; break;
				case Inst.UnaryNegative:  stack[sp] = -stack[sp]; break;

				case Inst.PushConst:      stack[++sp] = Compiler.numbers[Compiler.insts[pc++]]; break;
					
				default:
					throw new System.Exception("Unrecognized instruction " + Compiler.insts[pc - 1]);
				}

				System.Console.Write(string.Format("{0,3}: ", inst));
				for (int i = 0; i <= sp; i++) System.Console.Write(stack[i] + " ");
				System.Console.WriteLine();
			}
		}
	}
}

