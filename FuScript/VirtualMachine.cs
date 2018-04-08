namespace FuScript {
	public static class VirtualMachine {
		static byte[] inst;
		static int pc, length;

		static byte b1, b2, b3;
		static double f1, f2, f3;
		static int i1, i2, i3;

		static readonly double[] fdata = new double[1000];

		public static void Init(byte[] bytecodes) {
			inst = bytecodes;
			length = bytecodes.Length;
			pc = 0;
		}

		public static void Run() {
			while (pc < length) {
				switch (inst[pc]) {
				case ByteCode.MoveConstToReg:  // move 1, f0
					fdata[inst[pc + 2]] = inst[pc + 1];
					pc += 3; break;
				default:
					throw new System.Exception("Unknow ByteCode " + inst[pc]);
				}
			}
		}
	}
}

