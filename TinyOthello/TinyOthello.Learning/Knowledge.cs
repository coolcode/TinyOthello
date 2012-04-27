using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyOthello;
using TinyOthello.Engines;
using System.IO;

namespace TinyOthello.Learning {
	public class Knowledge {

		protected IEngine Engine = new EndGameEngine();
		protected Rule rule = new Rule();

		public int EndGameDepth { get; set; }
		public int Records { get; private set; }
		public int Limit { get; set; }

		public Knowledge(int limit) {
			EndGameDepth = Math.Min((Constants.StoneCount - 4) / 2, 20);
			Limit = limit;
		}

		public void Generate() {
			var targetPath = Path.Combine(Environment.CurrentDirectory, "learning-results");
			if (!Directory.Exists(targetPath)) {
				Directory.CreateDirectory(targetPath);
			}

			string fileName = string.Format("{0}-{1}-{2:yyyy-MM-dd}.know",
											Constants.Line,
											EndGameDepth,
											DateTime.Now);

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

		private void Gen(TextWriter writer, Board board, int color) {
			if (Records >= Limit) {
				return;
			}

			if (board.EmptyCount == EndGameDepth) {
				var searchResult = Engine.Search(board.Copy(), color,16);
				WriteRecord(writer, board, searchResult, color);

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

			foreach (var move in moves) {
				int flipCount = board.MakeMove(move.Pos, color);

				Gen(writer, board, opp);

				board.Reback(move.Pos, flipCount, opp);
			}
		}

		private void WriteRecord(TextWriter writer, Board board, SearchResult searchResult, int color) {
			var text = string.Format("{0} {1} {2} {3}",
				board.ToFlatString(),
				searchResult.Score / Constants.HighestScore,
				(color == StoneType.Black ? "●" : "○"),
				searchResult.Move);

			writer.WriteLine(text);
		}
	}
}
