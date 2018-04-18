namespace FuScript {
	public static class Compiler {
		static void Log(string str) {
			System.Console.WriteLine(str);
		}

		public static void Reset() {
			
		}

		public static void Compile(ushort node) {
			switch (Parser.nodes[node]) {
				case Node.ExprStmt: ExprStmt(node); break;
				default:
					throw new System.Exception("Unknown node #" + Parser.nodes[node]);
			}
		}

		static void ExprStmt(ushort node) {
			ushort expr = Parser.nodes[node + 1];
			ushort op = Parser.nodes[expr];
		}
	}
}
