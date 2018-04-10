using Math = System.Math;
using Env = System.Collections.Generic.Dictionary<string, FuScript.ValueRef>;
using Obj = System.Collections.Generic.Dictionary<string, FuScript.Value>;

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

		static VirtualMachine() {
//			env.Add("pi", new ValueRef(new Value(Math.PI)));
//			env.Add("e", new ValueRef(new Value(Math.E)));

//			env.Add("abs",      new ValueRef(new Value(1, args => new Value(Math.Abs(args[0].num)))));
//			env.Add("acos",     new ValueRef(new Value(1, args => new Value(Math.Acos(args[0].num)))));
//			env.Add("asin",     new ValueRef(new Value(1, args => new Value(Math.Asin(args[0].num)))));
//			env.Add("atan",     new ValueRef(new Value(1, args => new Value(Math.Atan(args[0].num)))));
//			env.Add("atan2",    new ValueRef(new Value(2, args => new Value(Math.Atan2(args[0].num, args[1].num)))));
//			env.Add("ceiling",  new ValueRef(new Value(1, args => new Value(Math.Ceiling(args[0].num)))));
//			env.Add("cos",      new ValueRef(new Value(1, args => new Value(Math.Cos(args[0].num)))));
//			env.Add("cosh",     new ValueRef(new Value(1, args => new Value(Math.Cosh(args[0].num)))));
//			env.Add("exp",      new ValueRef(new Value(1, args => new Value(Math.Exp(args[0].num)))));
//			env.Add("floor",    new ValueRef(new Value(1, args => new Value(Math.Floor(args[0].num)))));
//			env.Add("ln",       new ValueRef(new Value(1, args => new Value(Math.Log(args[0].num)))));
//			env.Add("log",      new ValueRef(new Value(2, args => new Value(Math.Log(args[0].num, args[1].num)))));
//			env.Add("log10",    new ValueRef(new Value(1, args => new Value(Math.Log10(args[0].num)))));
//			env.Add("max",      new ValueRef(new Value(2, args => new Value(Math.Max(args[0].num, args[1].num)))));
//			env.Add("min",      new ValueRef(new Value(2, args => new Value(Math.Min(args[0].num, args[1].num)))));
//			env.Add("pow",      new ValueRef(new Value(2, args => new Value(Math.Pow(args[0].num, args[1].num)))));
//			env.Add("round",    new ValueRef(new Value(1, args => new Value(Math.Round(args[0].num)))));
//			env.Add("sign",     new ValueRef(new Value(1, args => new Value(Math.Sign(args[0].num)))));
//			env.Add("sin",      new ValueRef(new Value(1, args => new Value(Math.Sin(args[0].num)))));
//			env.Add("sinh",     new ValueRef(new Value(1, args => new Value(Math.Sinh(args[0].num)))));
//			env.Add("sqrt",     new ValueRef(new Value(1, args => new Value(Math.Sqrt(args[0].num)))));
//			env.Add("tan",      new ValueRef(new Value(1, args => new Value(Math.Tan(args[0].num)))));
//			env.Add("tanh",     new ValueRef(new Value(1, args => new Value(Math.Tanh(args[0].num)))));
//			env.Add("truncate", new ValueRef(new Value(1, args => new Value(Math.Truncate(args[0].num)))));
		}

		static ushort EatOperand() {
			return (ushort)(Compiler.insts[pc++] | Compiler.insts[pc++] << 8);
		}

		public static void Run() {
			int lastPc;
			ushort ic, n;
			Value[] args = null;
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

				case Opcode.BinaryLogicOr:        dsp -= 1; if ( dataStack[dsp].IsFalsy()) dataStack[dsp] = dataStack[dsp + 1]; break;
				case Opcode.BinaryLogicAnd:       dsp -= 1; if (!dataStack[dsp].IsFalsy()) dataStack[dsp] = dataStack[dsp + 1]; break;

				case Opcode.BinaryLess:           dsp -= 1; dataStack[dsp].Set(dataStack[dsp].num <  dataStack[dsp + 1].num); break;
				case Opcode.BinaryLessEqual:      dsp -= 1; dataStack[dsp].Set(dataStack[dsp].num <= dataStack[dsp + 1].num); break;
				case Opcode.BinaryGreater:        dsp -= 1; dataStack[dsp].Set(dataStack[dsp].num >  dataStack[dsp + 1].num); break;
				case Opcode.BinaryGreaterEqual:   dsp -= 1; dataStack[dsp].Set(dataStack[dsp].num >= dataStack[dsp + 1].num); break;

				case Opcode.BinaryEqual:          dsp -= 1;
					if (dataStack[dsp].type == dataStack[dsp + 1].type) {
						if (dataStack[dsp].type == Value.Number) dataStack[dsp].Set(dataStack[dsp].num == dataStack[dsp + 1].num);
						else                                     dataStack[dsp].Set(dataStack[dsp].obj == dataStack[dsp + 1].obj);
					} else dataStack[dsp].Set(false);
					break;

				case Opcode.UnaryNot:       dataStack[dsp].Set(dataStack[dsp].IsFalsy()); break;
				case Opcode.UnaryNegative:  dataStack[dsp].num = -dataStack[dsp].num; break;

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

				case Opcode.ObjectMemberSet: ((Obj)dataStack[dsp].obj)[Compiler.strings[EatOperand()]] = dataStack[--dsp]; break;
				case Opcode.ObjectMemberGet: dataStack[dsp] = ((Obj)dataStack[dsp].obj)[Compiler.strings[EatOperand()]]; break;

				case Opcode.MakeObject:      dataStack[++dsp].Set(new Obj()); break;

				case Opcode.MakeFunction:  // ic (MF n)
					n = EatOperand();
					ic = dataStack[dsp].sys1;
					dataStack[dsp].Set(n, ic, new Env(env));
					break;
				case Opcode.CallFunction:  // (CF n)
					n = EatOperand();

					if (dataStack[dsp - n].type != Value.Function && dataStack[dsp - n].type != Value.Interop) throw new System.Exception("Value not callable ");
					if (dataStack[dsp - n].sys1 != n) throw new System.Exception("Function argument count mismatch");

					if (dataStack[dsp - n].type == Value.Function) {
						ic = dataStack[dsp - n].sys2;

						envStack[++esp] = env;  // Non copy
						env = (Env)dataStack[dsp - n].obj;  // Load closure

						dataStack[dsp - n].Set(pc);
						pc = ic;
					} else {
						args = new Value[n];
						for (int i = n - 1; i >= 0; --i) args[i] = dataStack[dsp--];
						dataStack[dsp] = ((InteropFunction)dataStack[dsp].obj)(args);
					}
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

				System.Console.Write("{0,5} {1,3} [ ", lastPc, Compiler.marks[lastPc]);
				for (int i = 0; i <= dsp; i++) System.Console.Write(dataStack[i] + ", ");
				System.Console.Write("]\n");

				// System.Console.Write("{0,9} {{ ", esp);
				// foreach (var pair in env) System.Console.Write(pair.Key + ": " + pair.Value + ", ");
				// System.Console.Write("}\n");
			}
		}
	}
}

