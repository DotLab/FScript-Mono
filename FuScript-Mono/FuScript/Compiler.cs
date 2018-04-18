namespace FuScript {
	public static class Compiler {
		static void Log(string str) {
			System.Console.WriteLine(str);
		}

		public static void Reset() {
			
		}

		public static void Compile(ushort node) {
			ushort i, len;
			switch (Parser.nodes[node]) {
				case Node.Null: Log("push null"); break;
				case Node.True: Log("push true"); break;
				case Node.False: Log("push false"); break;
					
				case Node.Var: Log("push false"); break;
					
				case Node.ExprStmt: 
					Compile(Parser.nodes[node + 1]); 
					Log("pop");
					break;
				case Node.Program:
					len = Parser.nodes[node + 1]; node += 2;
					for (i = 0; i < len; ++i) Compile(Parser.nodes[node + i]);
					break;
				default:
					throw new System.Exception("Unknown node #" + Parser.nodes[node]);
			}
		}
	}
}
