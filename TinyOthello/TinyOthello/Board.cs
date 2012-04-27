using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public class Board : IEnumerable<Stone>, ICloneable {
		protected Stone[] Stones { get; private set; }
		private const int stoneCount = Constants.StoneCount;
		private readonly Rule rule = new Rule();

		private static readonly int[] Pow = new int[73];
		//public int CurrentColor { get; set; }

		public int EmptyCount { get; private set; }

		static Board() {
			for (int i = 0; i < Pow.Length; i++) {
				Pow[i] = i * Constants.HighestScore;
			}
		}

		public Board() {
			Initial();
			EmptyCount = Stones.Count(c => c.Type == StoneType.Empty);
		}

		public Board(string boardText)
			: this(boardText.Select(c => (c == '●' ? StoneType.Black : (c == '○' ? StoneType.White : StoneType.Empty)))) {
		}

		public Board(IEnumerable<int> board)
			: this() {
			board.ForEach((c, i) => Stones[i].Type = c);
			EmptyCount = board.Count(c => c == StoneType.Empty);
		}


		public Stone this[int index] {
			get {
				return Stones[index];
			}
			set {
				Stones[index] = value;
			}
		}

		public Stone this[int row, int col] {
			get {
				return Stones[row * Constants.Line + col];
			}
			set {
				Stones[row * Constants.Line + col] = value;
			}
		}

		private void Initial() {
			Stones = new Stone[stoneCount];
			for (int i = 0; i < stoneCount; i++) {
				Stones[i] = new Stone {
					Type = StoneType.Empty,
					Row = i / Constants.Line,
					Column = i % Constants.Line,
					Pos = i
				};
			}

			Border = Stones.Where(c => c.IsBorder).ToList();

			for (int i = 0; i < stoneCount; i++) {
				Stone stone = Stones[i];
				stone[Direct.Top] = (stone.Row == 0 ? Stone.Dummy : Stones[i - Constants.Line]);
				stone[Direct.RightTop] = (stone.Row == 0 || stone.Column == Constants.Line - 1 ? Stone.Dummy : Stones[i - Constants.Line + 1]);
				stone[Direct.Right] = (stone.Column == Constants.Line - 1 ? Stone.Dummy : Stones[i + 1]);
				stone[Direct.RightBottom] = (stone.Row == Constants.Line - 1 || stone.Column == Constants.Line - 1 ? Stone.Dummy : Stones[i + Constants.Line + 1]);
				stone[Direct.Bottom] = (stone.Row == Constants.Line - 1 ? Stone.Dummy : Stones[i + Constants.Line]);
				stone[Direct.LeftBottom] = (stone.Row == Constants.Line - 1 || stone.Column == 0 ? Stone.Dummy : Stones[i + Constants.Line - 1]);
				stone[Direct.Left] = (stone.Column == 0 ? Stone.Dummy : Stones[i - 1]);
				stone[Direct.LeftTop] = (stone.Row == 0 || stone.Column == 0 ? Stone.Dummy : Stones[i - Constants.Line - 1]);
			}

			int flipsStartIndex = (Constants.Line / 2 - 1) * (Constants.Line + 1);

			Stones[flipsStartIndex].Type = StoneType.Black;
			Stones[flipsStartIndex + 1].Type = StoneType.White;
			Stones[flipsStartIndex + Constants.Line].Type = StoneType.White;
			Stones[flipsStartIndex + Constants.Line + 1].Type = StoneType.Black;
		}

		public List<Stone> Border { get; private set; }

		#region IEnumerable<Stone> Members

		public IEnumerator<Stone> GetEnumerator() {
			return this.Stones.ToList().GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		#endregion

		#region ICloneable Members

		public object Clone() {
			return Copy();
		}

		public Board Copy() {
			return new Board(this.Select(c => c.Type));
		}

		#endregion

		public int Count(int color) {
			return this.Where(c => c.Type == color).Count();
		}

		public override string ToString() {
			Dictionary<int, string> conv = new Dictionary<int, string>();
			conv.Add(StoneType.Black, "●");
			conv.Add(StoneType.White, "○");
			conv.Add(StoneType.Empty, "□");
			conv.Add(StoneType.Dummy, "×");
			StringBuilder sb = new StringBuilder();
			this.ForEach(c => {
				sb.Append(conv[c.Type]);
				if (c.Column == Constants.Line - 1) {
					sb.Append(Environment.NewLine);
				}
			});

			return sb.ToString();
		}
		/// <summary>
		/// <example>Sample: □○○○○●●●●○●□□□○●</example>
		/// </summary>
		/// <returns></returns>
		public  string ToFlatString() {
			return this.ToString().Replace(Environment.NewLine,"");
		}

		public int MakeMove(int pos, int color) {
			int flipCount = rule.DoFlip(this, pos, color);
			Stones[pos].Type = color;
			//Stones[pos].Type = this.CurrentColor;
			//轮换
			//ChangeColor();
			EmptyCount--;
			return flipCount;
		}

		public void Reback(int pos, int flipCount, int color) {
			rule.UndoFlip(this, flipCount, color);
			Stones[pos].Type = StoneType.Empty;
			EmptyCount++;
			//轮换
			//ChangeColor();
		}

		//private void ChangeColor() {
		//    int oppColor = 3 ^ this.CurrentColor;
		//    if (rule.CanFlip(this, oppColor)) {
		//        this.CurrentColor = oppColor;
		//    }
		//}

		//TODO:收益评估
		public int Eval(int color) {
			var eval = Diff(color);

			return eval;
		}

		//TODO:收盘收益计算
		public int EndEval(int color) {
			int diff = Diff(color);

			return Pow[diff + Constants.StoneCount] - Pow[Constants.StoneCount];
			//return diff * Constants.HighestScore;
		}

		public int Diff(int color) {
			var diff = this.Count(c => c.Type == StoneType.Black) - this.Count(c => c.Type == StoneType.White);

			if (color == StoneType.White) {
				diff = -diff;
			}
			/*
			int num = EmptyCount + this.Count(c => c.Type == StoneType.Black) + this.Count(c => c.Type == StoneType.White);

			if(num!= Constants.StoneCount)
				throw new NotSupportedException("EmptyCount");
			//if (this.CurrentColor == StoneType.White) {
			//    diff = -diff;
			//}
			 */
			return diff;
		}
	}
}
