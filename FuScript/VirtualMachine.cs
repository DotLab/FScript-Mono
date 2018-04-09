using Env = System.Collections.Generic.Dictionary<string, FuScript.ValueRef>;

namespace FuScript {
	public static class VirtualMachine {
		static int pc, length;

		static readonly Value[] dataStack = new Value[512];
		static int dsp = -1;

		static readonly Env[] envStack = new Env[512];
		static int esp = -1;

		public static void Init() {
			length = Compiler.icount;
//			pc = 0; dsp = -1; esp = -1;
		}

		static Env env = new Env();

		public static void Run() {
			int lastPc;
			ushort ic, n;
			var list = new System.Collections.Generic.List<string>();
			string[] strarr;
			while (pc < length) {
				lastPc = pc;
				switch (Compiler.insts[pc++]) {
				case Opcode.BinarySubtract: dsp -= 1; dataStack[dsp].num -= dataStack[dsp + 1].num; break;
				case Opcode.BinaryAdd:      dsp -= 1; dataStack[dsp].num += dataStack[dsp + 1].num; break;
				case Opcode.BinaryDivide:   dsp -= 1; dataStack[dsp].num /= dataStack[dsp + 1].num; break;
				case Opcode.BinaryMultiply: dsp -= 1; dataStack[dsp].num *= dataStack[dsp + 1].num; break;

				case Opcode.UnaryNot:       dataStack[dsp] = new Value(dataStack[dsp].type); break;
				case Opcode.UnaryNegative:  dataStack[dsp] = new Value(-dataStack[dsp].num); break;

				case Opcode.PushSmallInt:   dataStack[++dsp] = new Value(Compiler.insts[pc++] | Compiler.insts[pc++] << 8); break;
				case Opcode.Jump:           pc = Compiler.insts[pc++] | Compiler.insts[pc++] << 8; break;
					
				case Opcode.PushNumber:     dataStack[++dsp] = new Value(Compiler.numbers[Compiler.insts[pc++] | Compiler.insts[pc++] << 8]); break;
				case Opcode.PushString:     dataStack[++dsp] = new Value(Compiler.strings[Compiler.insts[pc++] | Compiler.insts[pc++] << 8]); break;
					
				case Opcode.PushVar:        dataStack[++dsp] = env[Compiler.strings[Compiler.insts[pc++] | Compiler.insts[pc++] << 8]].value; break;
				case Opcode.PopVar:         env[Compiler.strings[Compiler.insts[pc++] | Compiler.insts[pc++] << 8]].value = dataStack[dsp--]; break;
				case Opcode.PopNewVar:      env[Compiler.strings[Compiler.insts[pc++] | Compiler.insts[pc++] << 8]] = new ValueRef(dataStack[dsp--]); break;
					
				case Opcode.CloneEnv:       envStack[++esp] = new Env(env); break;
				case Opcode.RestoreEnv:     env = envStack[esp--]; break;
					
				case Opcode.Print:          System.Console.WriteLine(dataStack[dsp--]); break;

				case Opcode.PushConstNull:  dataStack[++dsp].type = Value.Null; break;

				case Opcode.MakeFunction:  // fname arg1 arg2 ... argn n ic
					ic = (ushort)dataStack[dsp--].num;
					n = (ushort)dataStack[dsp--].num;
					strarr = new string[n];
					for (int i = n - 1; i >= 0; --i) strarr[i] = (string)dataStack[dsp--].obj;
					dataStack[++dsp].type = Value.Function;
					dataStack[dsp].num = ic;
					dataStack[dsp].obj = strarr;
					break;
					
				default:
					throw new System.Exception("Unrecognized instruction " + Compiler.insts[pc - 1]);
				}

				System.Console.Write(string.Format("{0,3} [ ", Compiler.marks[lastPc]));
				for (int i = 0; i <= dsp; i++) System.Console.Write(dataStack[i] + ", ");
				System.Console.Write("]\n\t{0,3} {{ ", esp);
				foreach (var pair in env) System.Console.Write(pair.Key + ": " + pair.Value + ", ");
				System.Console.WriteLine("}");
			}
		}
	}
}

