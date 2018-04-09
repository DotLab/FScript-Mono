﻿using Env = System.Collections.Generic.Dictionary<string, FuScript.ValueRef>;

namespace FuScript {
	public static class VirtualMachine {
		static int pc, length;

		static readonly Value[] dataStack = new Value[512];
		static int dsp;

		static readonly Env[] envStack = new Env[512];
		static int esp;

		public static void Init() {
			length = Compiler.icount;
			pc = 0; dsp = -1; esp = -1;
		}

		static Env env = new Env();

		public static void Run() {
			byte inst;
			int p;
			while (pc < length) {
				p = pc;
				switch (inst = Compiler.insts[pc++]) {
				case Inst.BinarySubtract: dsp -= 1; dataStack[dsp].num -= dataStack[dsp + 1].num; break;
				case Inst.BinaryAdd:      dsp -= 1; dataStack[dsp].num += dataStack[dsp + 1].num; break;
				case Inst.BinaryDivide:   dsp -= 1; dataStack[dsp].num /= dataStack[dsp + 1].num; break;
				case Inst.BinaryMultiply: dsp -= 1; dataStack[dsp].num *= dataStack[dsp + 1].num; break;

				case Inst.UnaryNot:       dataStack[dsp] = new Value(dataStack[dsp].type); break;
				case Inst.UnaryNegative:  dataStack[dsp] = new Value(-dataStack[dsp].num); break;

				case Inst.PushConst:      dataStack[++dsp] = new Value(Compiler.numbers[Compiler.insts[pc++]]); break;
					
				case Inst.PushVar:        dataStack[++dsp] = env[Compiler.strings[Compiler.insts[pc++]]].value; break;
				case Inst.PopVar:         env[Compiler.strings[Compiler.insts[pc++]]].value = dataStack[dsp--]; break;
				case Inst.PopNewVar:      env[Compiler.strings[Compiler.insts[pc++]]] = new ValueRef(dataStack[dsp--]); break;
					
				case Inst.CloneEnv:       envStack[++esp] = new Env(env); break;
				case Inst.RestoreEnv:     env = envStack[esp--]; break;
					
				case Inst.Print:          System.Console.WriteLine(dataStack[dsp--]); break;

				case Inst.PushConstNull:  dataStack[++dsp].type = Value.Null; break;
					
				default:
					throw new System.Exception("Unrecognized instruction " + Compiler.insts[pc - 1]);
				}

				System.Console.Write(string.Format("{0,3}: ", Compiler.marks[p]));
				for (int i = 0; i <= dsp; i++) System.Console.Write(dataStack[i] + " ");
				System.Console.Write("\n\t{0,3} {{ ", esp);
				foreach (var pair in env) System.Console.Write(pair.Key + ": " + pair.Value + ", ");
				System.Console.WriteLine("}");
			}
		}
	}
}
