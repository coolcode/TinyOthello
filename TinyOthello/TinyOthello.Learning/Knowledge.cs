using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyOthello;
using TinyOthello.Engines;
using System.IO;

namespace TinyOthello.Learning {
	public class Knowledge {

		public IEngine Engine = new EndGameEngine();
		protected Rule rule = new Rule();

		public int EndGameDepth { get; set; }
		public int Records { get; private set; }
		public int Limit { get; set; }
		public int SearchDepth { get; set; }

		public Knowledge(int limit, int endGameDepth, int searchDepth) {
			Limit = limit;
			EndGameDepth = endGameDepth;
			SearchDepth = searchDepth;
		}

		public void Generate(string fileName = null) {
			var targetPath = Path.Combine(Environment.CurrentDirectory, "learning-results");
			if (!Directory.Exists(targetPath)) {
				Directory.CreateDirectory(targetPath);
			}

			if (string.IsNullOrEmpty(fileName)) {
				fileName = string.Format("{0}-{1}-{2:yyyy-MM-dd}.know",
											Constants.Line,
											EndGameDepth,
											DateTime.Now);
			}


			targetPath = Path.Combine(targetPath, fileName);

			Records = 0;
			Board board = new Board();
			rule.Clear();

			Console.WriteLine("Begin. Saved Path:{0}", targetPath);

			using (TextWriter writer = new StreamWriter(targetPath, true)) {
				Gen(writer, board, StoneType.Black);
				writer.Flush();
			}

			Console.WriteLine("Write Records: {0}", Records);
		}

		private void Gen(TextWriter writer, Board board, int color, bool prevmove = true) {
			if (Records >= Limit) {
				return;
			}

			if (board.EmptyCount == EndGameDepth) {
				var searchResult = Engine.Search(board.Copy(), color, this.SearchDepth);
				WriteRecord(writer, board, searchResult.Score, searchResult.Move, color);

				Records++;

				//每1000条记录就Flush一次
				if (Records % 1000 == 0) {
					Console.WriteLine("Write Records: {0}", Records);
					writer.Flush();
				}

				return;
			}

			int opp = color.Opp();
			var moves = rule.FindFlips(board, color).ToList();

			if (moves.Count == 0) {
				if (prevmove) {
					Gen(writer, board, opp, false);
				}

				return;
			}

			foreach (var move in moves) {
				int flipCount = board.MakeMove(move.Pos, color);

				Gen(writer, board, opp);

				board.Reback(move.Pos, flipCount, opp);
			}

		}

		private void WriteRecord(TextWriter writer, Board board, int score, int move, int color) {
			if(score < Constants.HighestScore && score>- Constants.HighestScore &&(score>100 || score<-100)  ) { 
				throw new Exception("score");
			}

			if (score >= Constants.HighestScore || score <=-Constants.HighestScore) { 
				score = score / Constants.HighestScore;
			}
			 
			var text = string.Format("{0} {1} {2} {3}",
				board.ToFlatString(),
				score,
				(color == StoneType.Black ? "●" : "○"),
				move);

			writer.WriteLine(text);
		}
	}
}
