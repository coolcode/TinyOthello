using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello.Engines {
	public class EndGameEngine : BaseEngine {
		private const int MaxScore = Constants.HighestScore * 100;


		public override SearchResult Search(Board board, int color,int depth) {
			SearchResult searchResult = new SearchResult();
			//board.CurrentColor = color;
			//PrepareToSolve(board);
			int alpha = -MaxScore - 1;
			int beta = MaxScore + 1;
			int opp =  color.Opp();

			Clock clock = new Clock();
			clock.Start();
			//Roulette roulette = new Roulette();
			//board.RefreshHash();

			int score = -MaxScore;
			int eval;
			//是否调用零窗口的标志
			bool foundPv = false;

			var moves = rule.FindFlips(board, color).ToList();

			if (moves.Count == 0) {
				return new SearchResult() { Move = -1 };
			}

			//if (moves.Count == 1) {
			//    return new SearchResult() { Move = moves.First().Pos };
			//}

			for (int i = 0; i < moves.Count; i++) {
				var pos = moves[i].Pos;
				//下棋 
				//---------------------------
				int flipCount = board.MakeMove(pos, color);

				searchResult.Nodes++;
				//检测
				if (foundPv) {
					//调用零窗口
					eval = -FastestFirstSolve(board, -alpha - 1, -alpha, depth - 1, searchResult, opp, color);
					if ((eval > alpha) && (eval < beta)) {
						eval = -FastestFirstSolve(board, -beta, -eval, depth - 1, searchResult, opp, color);
						//eval = -FastestFirstMidSolve( -beta, -alpha, oppcolor, depth - 1);
					}
				}
				else {
					eval = -FastestFirstSolve(board, -beta, -alpha, depth - 1, searchResult, opp, color);
				}

				//em.ReLink();
				//---------------------------
				//Eval.StepsPop(color);
				//恢复到上一步
				board.Reback(pos, flipCount, opp);

                searchResult.EvalList.Add(new EvalItem { Move = pos, Score = eval });

				searchResult.Message += string.Format("({0}:{1})", pos, eval);
				if (eval > score) {
					score = eval;
					//更新位置
					searchResult.Move = pos;
					searchResult.Score = score;

					if (eval > alpha) {
						if (eval >= beta) {
							//剪枝
							break;
						}
						alpha = eval;
						foundPv = true;
					}
				}
			}

			clock.Stop();

			searchResult.TimeSpan = clock.Elapsed;

			return searchResult;
		}

		/// <summary>
		/// 最快优先搜索
		/// </summary> 
		private int FastestFirstSolve(Board board, int alpha, int beta, int depth, SearchResult searchResult, int cur, int opp, bool prevmove = true) {
			lock (this) {
				//计算搜索的结点数
				searchResult.Nodes++;
				/*
				//尝试置换表裁剪，并得到置换表走法
				HashResult hashResult = board.ReadHash(alpha, beta, depth);
				if (hashResult != null) {
					Message.Hits++;
					return hashResult.Score;
				}
				*/

				if (board.EmptyCount == 0) {//游戏结束
					return EndEval(board,cur);
				}

				//叶子节点，局面估分
				if (depth == 0) {
					return Eval(board, cur);
				}

				int mvBest = 0;
				int score = -MaxScore;
				//是否调用零窗口的标志
				bool foundPv = false;
				int eval;

				#region hash code
				/*
				//int mvHash = hashResult.Move;
				//初始化最佳值和最佳走法

				HashType hashType = HashType.LOWER_BOUND;

				//初始化走法排序结构
				board.InitMoves(); //board.InitMoves(mvHash);

				int quickMove = board.GetQuickMove();

				//            if (moveDefend > Evalation.Defend_FreeThree)
				//                return moveDefend;
				//if (board.MoveHash != 0)
				//    quickMove = board.MoveHash;
				//else if ((board.MoveKiller1 != board.MoveHash) && (board.MoveKiller1 != 0) && board.CanMove(board.MoveKiller1))
				//    quickMove = board.MoveKiller1;
				//else if ((board.MoveKiller2 != board.MoveHash) && (board.MoveKiller2 != 0) && board.CanMove(board.MoveKiller2))
				//    quickMove = board.MoveKiller2;

				if (quickMove != 0) {
					//---------------------------
					board.MakeMove(quickMove);
					eval = -FastestFirstSolve(board, -beta, -alpha, depth - 1);
					//恢复到上一步
					board.Reback(quickMove);

					if (eval > alpha) {
						if (eval >= beta) {
							//----------------------
							hashType = HashType.UPPER_BOUND;
							// 记录到置换表
							board.StoreHash(hashType, eval, depth, quickMove);
							//TODO:有点怀疑是否需要这个条件
							if (mvBest != 0) {
								// 如果不是Alpha走法，就将最佳走法保存到历史表
								board.StoreBestMove(quickMove, depth);
							}
							//剪枝
							return eval;
						}
						//----------------
						hashType = HashType.EXACT;

						alpha = eval;
						foundPv = true;
					}
				}

				Roulette roulette = new Roulette();
				int gness;
				int moves = roulette.CalculateGoodness(out gness);

				if (gness == Evalation.InfScore) {
					return gness;
				}
				*/

				#endregion

				var moves = rule.FindFlips(board, cur).ToList();

				if (moves.Count == 0) {
					if (!prevmove) {//游戏结束
						return EndEval(board,cur);
						/*
						if (discdiff > 0)//自己赢
							return Constants.HighestScore;
						if (discdiff < 0)//对方赢
							return -Constants.HighestScore;
						return 0;//平手
					  */
					}
					else {
						/* I pass: */
						score = -FastestFirstSolve(board, -beta, -alpha, depth, searchResult, opp, cur, false);
					}

					return score;
				}

				for (int i = 0; i < moves.Count; i++) {
					var pos = moves[i].Pos;
					//下棋 
					//---------------------------
					int flipCount = board.MakeMove(pos, cur);

					searchResult.Nodes++;
					//检测
					if (foundPv) {
						//调用零窗口
						eval = -FastestFirstSolve(board, -alpha - 1, -alpha, depth - 1, searchResult, opp, cur);
						if ((eval > alpha) && (eval < beta)) {
							eval = -FastestFirstSolve(board, -beta, -eval, depth - 1, searchResult, opp, cur);
						}
					}
					else {
						eval = -FastestFirstSolve(board, -beta, -alpha, depth - 1, searchResult, opp, cur);
					}

					//em.ReLink();
					//---------------------------
					//Eval.StepsPop(color);
					//恢复到上一步
					board.Reback(pos, flipCount, opp);

					if (eval > score) {
						score = eval;
						//mvBest = sqnum;

						if (eval > alpha) {
							if (eval >= beta) {
								//剪枝
								return score;
								//hashType = HashType.UPPER_BOUND;
								//break;
							}
							//----------------
							//hashType = HashType.EXACT;

							alpha = eval;
							foundPv = true;
						}
					}


					/*
					// 记录到置换表
					board.StoreHash(hashType, score, depth, mvBest);
					if (mvBest != 0) {
						// 如果不是Alpha走法，就将最佳走法保存到历史表
						board.StoreBestMove(mvBest, depth);
					}
					 */
				}
				return score;
			}
		}

		protected virtual int Eval(Board board, int color) {
			return board.Eval(color);
		}

		protected virtual int EndEval(Board board, int color) {
			return board.EndEval(color);
		}
	}
}
