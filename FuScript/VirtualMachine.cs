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
				case Opcode.BinaryDivide:   dsp -= 1; dataStack[dsp].num /= dataStack[dsp + 1].num; break;
				case Opcode.BinaryMultiply: dsp -= 1; dataStack[dsp].num *= dataStack[dsp + 1].num; break;
				case Opcode.BinaryAdd:      dsp -= 1; 
					if (dataStack[dsp].type == Value.String) dataStack[dsp].Set((string)dataStack[dsp].obj + dataStack[dsp + 1].AsString()); 
					else dataStack[dsp].num += dataStack[dsp + 1].num; 
					break;

				case Opcode.UnaryNot:       dataStack[dsp].Set(dataStack[dsp].IsFalsy()); break;
				case Opcode.UnaryNegative:  dataStack[dsp].Set(-dataStack[dsp].num); break;

				case Opcode.PushSmallInt:   dataStack[++dsp].Set(EatOperand()); break;
				case Opcode.Jump:           pc = EatOperand(); break;
					
				case Opcode.PushNumber:     dataStack[++dsp].Set(Compiler.numbers[EatOperand()]); break;
				case Opcode.PushString:     dataStack[++dsp].Set(Compiler.strings[EatOperand()]); break;
					
				case Opcode.PushVar:        dataStack[++dsp] = env[Compiler.strings[EatOperand()]].value; break;
				case Opcode.PopVar:         env[Compiler.strings[EatOperand()]].value = dataStack[dsp--]; break;
				case Opcode.PeekVar:        env[Compiler.strings[EatOperand()]].value = dataStack[dsp]; break;
				case Opcode.PopNewVar:      env[Compiler.strings[EatOperand()]] = new ValueRef(dataStack[dsp--]); break;
				case Opcode.PopDiscard:     dsp--; break;

				case Opcode.CloneEnv:       envStack[++esp] = new Env(env); break;
				case Opcode.RestoreEnv:     env = envStack[esp--]; break;
					
				case Opcode.Print:          System.Console.WriteLine(dataStack[dsp--]); break;

				case Opcode.PushConstTrue:  dataStack[++dsp].type = Value.True; break;
				case Opcode.PushConstFalse: dataStack[++dsp].type = Value.False; break;
				case Opcode.PushConstNull:  dataStack[++dsp].type = Value.Null; break;

				case Opcode.BranchIfFalsy:  if (dataStack[dsp--].IsFalsy()) pc = EatOperand(); else EatOperand(); break;
					
				case Opcode.MakeFunction:  // ic (MF n)
					n = EatOperand();
					ic = dataStack[dsp].sys1;
					dataStack[dsp].Set(n, ic, new Env(env));
					break;
				case Opcode.CallFunction:  // (CF n)
					n = EatOperand();

					if (dataStack[dsp - n].type != Value.Function) throw new System.Exception("Value not callable ");
					if (dataStack[dsp - n].sys1 != n) throw new System.Exception("Function argument count mismatch");

					ic = dataStack[dsp - n].sys2;

					envStack[++esp] = env;  // Non copy
					env = (Env)dataStack[dsp - n].obj;  // Load closure

					dataStack[dsp - n].Set(pc);
					pc = ic;
					break;
				case Opcode.Return:  // ret val
					ic = dataStack[--dsp].sys2;  // Return addr
					dataStack[dsp] = dataStack[dsp + 1];  // Copy val
					env = envStack[esp--];  // Restore env

					pc = ic;
					break;

				default:
					throw new System.Exception("Unrecognized instruction " + Compiler.insts[pc - 1]);
				}

				System.Console.Write(string.Format("{0,5} {1,3} [ ", lastPc, Compiler.marks[lastPc]));
				for (int i = 0; i <= dsp; i++) System.Console.Write(dataStack[i] + ", ");
				System.Console.Write("]\n{0,9} {{ ", esp);
				foreach (var pair in env) System.Console.Write(pair.Key + ": " + pair.Value + ", ");
				System.Console.WriteLine("}");
			}
		}
	}
}

