using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello.Engines {
	public class RandomEngine : BaseEngine {
		private Random rand = new Random();

		private string name;
		public override string  Name {
			get { 
				 return name?? base.Name;
			}
		}

		public RandomEngine() {
			
		}

		public RandomEngine(string name) {
			this.name = name;
		}


		public override SearchResult Search(Board board, int color, int depth) {
			var moves = rule.FindFlips(board, color).ToList();
			var randMoveIndex = rand.Next(0, moves.Count);

			return new SearchResult {
				Move = moves[randMoveIndex].Pos,
				Score = 2,
				Message = "random move"
			};
		}
	}
}
