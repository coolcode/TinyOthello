using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Neuro;

namespace TinyOthello.Engines {
	public class NeuralEngine : EndGameEngine {
		private Network network;

		public NeuralEngine(string networkFile) {
			network = Network.Load(networkFile);
		}

		protected override int Eval(Board board, int color) {
			var quantification = board.Select(c => c.Type == StoneType.Black ? 0.9 : (c.Type == StoneType.White ? 0.1 : 0.5)).ToArray();
			var output = network.Compute(quantification);
			var eval = (output[0] * 2 * Constants.StoneCount - Constants.StoneCount);

			return (int)(color == StoneType.Black ? eval : -eval);
			//return base.Eval(board, color);
		}

		protected override int EndEval(Board board, int color) {
			//Console.WriteLine("Not support!");
			return base.EndEval(board, color);
		}
	}
}
