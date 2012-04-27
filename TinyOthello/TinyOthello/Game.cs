using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace TinyOthello {
	public interface IGame {
		IEnumerable<IEngine> FindEngines();
		void Run(IEnumerable<IEngine> engines, int count = 1);
	}

	public class Game : IGame {
		protected readonly Rule rule = new Rule();

		public Game() {

		}

		#region IGame Members

		public IEnumerable<IEngine> FindEngines() {
			var enginesPath = Path.Combine(Environment.CurrentDirectory, "engines");
			foreach (var file in Directory.GetFiles(enginesPath)) {
				var ass = Assembly.LoadFrom(file);
				var engines = ass.GetTypes().Where(t => t.IsSubclassOf(typeof(IEngine)));
				foreach (var engine in engines) {
					yield return (IEngine)Activator.CreateInstance(engine);
				}
			}
		}

		public void Run(IEnumerable<IEngine> engines, int count = 1) {
			var targetPath = Path.Combine(Environment.CurrentDirectory, "fight-results");
			if (!Directory.Exists(targetPath)) {
				Directory.CreateDirectory(targetPath);
			}

			int i = 0;
			while (i++ < count) {
				engines.Fight((e1, e2) => {
					Fight(e1, e2, targetPath, GetDefaultBoard());
					Fight(e2, e1, targetPath, GetDefaultBoard());
				});

			}
		}

		public void Fight(IEngine engineA, IEngine engineB, string targetPath, Board board, int color = StoneType.Black) {
			var targetFile = Path.Combine(targetPath,
										  string.Format("{0:yyyy-MM-dd HH-mm} {1}-{2}.txt", DateTime.Now, engineA.Name, engineB.Name));

			using (TextWriter writer = new StreamWriter(targetFile, true)) {
				Console.SetOut(writer);

				Console.WriteLine("################### Begin #######################");
				Console.WriteLine("{0} ({2}) vs {1} ({3})", engineA.Name, engineB.Name, "Black", "White");

				var fightResult = Fight(engineA, engineB, board, color);

				Console.WriteLine("################### Result #######################");
				Console.WriteLine("{0}", fightResult);
				Console.WriteLine("#################### End #######################");

				writer.Flush();
			}

			Console.SetOut(Console.Error);
		}


		private FightResult Fight(IEngine engineA, IEngine engineB) {
			return Fight(engineA, engineB, GetDefaultBoard());
		}

		private Board GetDefaultBoard() {
			Board board = new Board();
			/*
			if (Constants.Line == 4) {
				board[1, 1].Type = StoneType.Black;
				board[1, 2].Type = StoneType.White;
				board[2, 1].Type = StoneType.White;
				board[2, 2].Type = StoneType.Black;

			}
			else if (Constants.Line == 6) {
				board[2, 2].Type = StoneType.Black;
				board[2, 3].Type = StoneType.White;
				board[3, 2].Type = StoneType.White;
				board[3, 3].Type = StoneType.Black;
			}
			*/
			return board;
		}

		private FightResult Fight(IEngine engineA, IEngine engineB, Board board, int color = StoneType.Black) {
			Clock clock = new Clock();
			clock.Start();

			IEngine[] engines = new[] { engineA, engineB };
			int[] stoneTypes = new[] { StoneType.Black, StoneType.White };
			int turn = color - 1; //0;
			Console.WriteLine(board);

			while (true) {
				//board.CurrentColor = stoneTypes[turn];
				var searchResult = engines[turn].Search(board.Copy(), stoneTypes[turn], (Constants.StoneCount - 4) / 2);
				Console.WriteLine("[{0}] {1}", engines[turn].Name, searchResult);
				Console.Out.Flush();

				if (searchResult.Move < 0 ||
					searchResult.Move >= Constants.StoneCount ||
					!rule.CanFlip(board[searchResult.Move], stoneTypes[turn])) {
					clock.Stop();
					//下棋异常
					Console.WriteLine(board);

					return new FightResult() {
						WinnerName = engines[1 - turn].Name,
						LoserName = engines[turn].Name,
						WinnerStoneType = (turn == 0 ? StoneType.Black : StoneType.White),
						Score = 1,
						TimeSpan = clock.TotalMilliseconds
					};
				}
				else {
					board.MakeMove(searchResult.Move, stoneTypes[turn]);
					Console.WriteLine(board);
				}

				turn = 1 ^ turn;

				var canFlip = rule.CanFlip(board, stoneTypes[turn]);

				if (!canFlip) {
					//对方无棋可下，轮换
					turn = 1 ^ turn;
					canFlip = rule.CanFlip(board, stoneTypes[turn]);

					if (!canFlip) {
						//双方都无棋可下，结束
						break;
					}
				}
			}

			clock.Stop();
			//Console.WriteLine(board);

			return new FightResult(board, engines) {
				TimeSpan = clock.TotalMilliseconds
			};
		}

		#endregion
	}

	public class FightResult {

		public FightResult() {

		}

		public FightResult(Board board, IEngine[] engines) {
			var diffNum = board.Count(StoneType.Black) - board.Count(StoneType.White);

			int winnerIndex = diffNum > 0 ? 0 : 1;

			WinnerName = engines[winnerIndex].Name;
			LoserName = engines[1 - winnerIndex].Name;
			WinnerStoneType = diffNum > 0 ? StoneType.Black : StoneType.White;
			Score = Math.Abs(diffNum);
		}

		public string WinnerName { get; set; }
		public string LoserName { get; set; }
		public int WinnerStoneType { get; set; }
		public int Score { get; set; }
		public double TimeSpan { get; set; }

		public override string ToString() {
			return string.Format("Winner:{0},{1} Loser:{2}, Score:{3}, TimeSpan:{4}",
								 WinnerName,
								 (WinnerStoneType == StoneType.Black ? "Black" : "White"),
								 LoserName,
								 Score,
								 TimeSpan);
		}
	}
}
