using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AForge.Neuro;

namespace TinyOthello.Engines {
	public class NeuralEngine : EndGameEngine {
		private readonly Dictionary<int, Network> networks = new Dictionary<int, Network>();

		public NeuralEngine(string networkFile) {
			LoadFile(networkFile);
		}

		public NeuralEngine() {
			var files = Directory.GetFiles(Environment.CurrentDirectory, "*.net").OrderByDescending(c => c);

			files.ForEach(LoadFile);
		}

		private void LoadFile(string file) {
			var fileName = Path.GetFileName(file);
			var depth = Convert.ToInt32(fileName.Split('-')[1]);
			if (!networks.ContainsKey(depth)) {
				networks.Add(depth, Network.Load(file));
			}
		}

		public override SearchResult Search(Board board, int color, int depth) {
			if (board.EmptyCount > 6) {
				depth = 2;
			}

			return base.Search(board, color, depth);
		}

		protected override int Eval(Board board, int color) {
			if (!networks.ContainsKey(board.EmptyCount)) {
				return base.Eval(board, color);
			}

			var quantification = board.Select(c => c.Type == StoneType.Black ? 0.9 : (c.Type == StoneType.White ? 0.1 : 0.5)).ToArray();
			var output = networks[board.EmptyCount].Compute(quantification);
			var eval = (output[0] * 2 * Constants.StoneCount - Constants.StoneCount);

			return (int)(color == StoneType.Black ? eval : -eval);

		}

		protected override int EndEval(Board board, int color) {
			//Console.WriteLine("Not support!");
			return base.EndEval(board, color);
		}
	}
}
