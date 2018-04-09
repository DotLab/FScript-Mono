using Env = System.Collections.Generic.Dictionary<string, FuScript.ValueRef>;

namespace FuScript {
	public static class VirtualMachine {
		static ushort pc, length;

		static readonly Value[] dataStack = new Value[512];
		static int dsp = -1;

		static readonly Env[] envStack = new Env[512];
		static int esp = -1;

		public static void Init() {
			length = Compiler.icount;
//			pc = 0; dsp = -1; esp = -1;
		}

		static Env env = new Env();

		static ushort EatOperand() {
			return (ushort)(Compiler.insts[pc++] | Compiler.insts[pc++] << 8);
		}

		public static void Run() {
			int lastPc;
			ushort ic, n;
//			var list = new System.Collections.Generic.List<string>();
//			string[] strarr;
			while (pc < length) {
				lastPc = pc;
				switch (Compiler.insts[pc++]) {
				case Opcode.BinarySubtract: dsp -= 1; dataStack[dsp].num -= dataStack[dsp + 1].num; break;
				case Opcode.BinaryAdd:      dsp -= 1; dataStack[dsp].num += dataStack[dsp + 1].num; break;
				case Opcode.BinaryDivide:   dsp -= 1; dataStack[dsp].num /= dataStack[dsp + 1].num; break;
				case Opcode.BinaryMultiply: dsp -= 1; dataStack[dsp].num *= dataStack[dsp + 1].num; break;

				case Opcode.UnaryNot:       dataStack[dsp] = new Value(dataStack[dsp].type); break;
				case Opcode.UnaryNegative:  dataStack[dsp] = new Value(-dataStack[dsp].num); break;

				case Opcode.PushSmallInt:   dataStack[++dsp] = new Value(EatOperand()); break;
				case Opcode.Jump:           pc = EatOperand(); break;
					
				case Opcode.PushNumber:     dataStack[++dsp] = new Value(Compiler.numbers[EatOperand()]); break;
				case Opcode.PushString:     dataStack[++dsp] = new Value(Compiler.strings[EatOperand()]); break;
					
				case Opcode.PushVar:        dataStack[++dsp] = env[Compiler.strings[EatOperand()]].value; break;
				case Opcode.PopVar:         env[Compiler.strings[EatOperand()]].value = dataStack[dsp--]; break;
				case Opcode.PeekVar:        env[Compiler.strings[EatOperand()]].value = dataStack[dsp]; break;
				case Opcode.PopNewVar:      env[Compiler.strings[EatOperand()]] = new ValueRef(dataStack[dsp--]); break;
				case Opcode.PopDiscard:     dsp--; break;

				case Opcode.CloneEnv:       envStack[++esp] = new Env(env); break;
				case Opcode.RestoreEnv:     env = envStack[esp--]; break;
					
				case Opcode.Print:          System.Console.WriteLine(dataStack[dsp--]); break;

				case Opcode.PushConstNull:  dataStack[++dsp].type = Value.Null; break;

				case Opcode.MakeFunction:  // n (MF ic)
					ic = EatOperand();
					n = dataStack[dsp--].sys1;

					dataStack[++dsp].type = Value.Function;
					dataStack[dsp].sys1 = ic;
					dataStack[dsp].sys2 = n;
					break;
				case Opcode.CallFunction:  // (CF n)
					if (dataStack[dsp].type != Value.Function) throw new System.Exception("Value not callable");

					ic = dataStack[dsp].sys1;
					n = dataStack[dsp--].sys2;
					if (EatOperand() != n) throw new System.Exception("Function argument count mismatch");

					dataStack[++dsp].type = Value.Number;
					dataStack[dsp].sys1 = pc;  // Return addr
					pc = ic;
					break;
				case Opcode.Return:  // ret val
					ic = dataStack[--dsp].sys1;  // Return addr
					dataStack[dsp] = dataStack[dsp + 1];  // Copy val

					pc = ic;
					break;

				default:
					throw new System.Exception("Unrecognized instruction " + Compiler.insts[pc - 1]);
				}

				System.Console.Write(string.Format("{0,5} {1,3} [ ", lastPc, Compiler.marks[lastPc]));
				for (int i = 0; i <= dsp; i++) System.Console.Write(dataStack[i] + ", ");
				System.Console.Write("]\n\t{0,3} {{ ", esp);
				foreach (var pair in env) System.Console.Write(pair.Key + ": " + pair.Value + ", ");
				System.Console.WriteLine("}");
			}
		}
	}
}

