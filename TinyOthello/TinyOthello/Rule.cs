using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public class Rule {

		/// <summary>
		/// 被翻转的棋子索引栈
		/// </summary>
		private readonly Stack<int> FlipStack = new Stack<int>();

		public void Clear() {
			FlipStack.Clear();
		}

		#region Board Rules

		public bool CanFlip(Board board, int color) {
			return board.Any(s => CanFlip(s, color));
		}

		public IEnumerable<Stone> FindFlips(Board board, int color) {
			return board.Where(s => CanFlip(s, color));
		}

		public int DoFlip(Board board, int pos, int color) {
			return DoFlip(board[pos], color);
		}

		/// <summary>
		/// 恢复翻转的棋子，但不包括下棋的子
		/// </summary> 
		public void UndoFlip(Board board, int flipCount, int oppcolor) {
			//判断flipCount是否为奇数
			if ((flipCount & 1) == 1) {
				flipCount--;
				board[FlipStack.Pop()].Change(oppcolor);
			}
			while (flipCount > 0) {
				flipCount -= 2;
				board[FlipStack.Pop()].Change(oppcolor);
				board[FlipStack.Pop()].Change(oppcolor);
			}
		}

		#endregion

		#region Stone Rules

		public bool CanFlip(Stone current, int color) {
			return current.IsEmpty && Can(current, color, CanFlip);
		}

		private bool CanFlip(Stone current, int color, int dir) {
			Stone p = current[dir];
			if(!p.IsOpponent(color)) {
				return false;
			}

			do {
				p = p[dir];
			} while (p.IsOpponent(color));

			bool result = (p.Type == color);

			return result;
		}

		public int DoFlip(Stone current, int color) {
			int oldCount = FlipStack.Count;
			Do(current, color, DoFlip);
			return FlipStack.Count - oldCount;
		}

		private void DoFlip(Stone current, int color, int dir) {
			if (!CanFlip(current, color, dir))
				return;
			Stone p = current[dir];
			while (p.Flip(color)) {
				FlipStack.Push(p.Pos);
				p = p[dir];
			}
		}

		public int CountFlips(Stone current, int color) {
			int result = Count(current, color, CountFlips);
			return result;
		}

		private int CountFlips(Stone current, int color, int dir) {
			if (!CanFlip(current, color, dir)) {
				return 0;
			}

			Stone p = current[dir];
			int count = 0;
			while (p.IsOpponent(color)) {
				p = p[dir];
				count++;
			}
			if (p.Type == color) {
				return count;
			}

			return 0;
		}

		#endregion

		#region Helper for Can Do Count

		private bool Can(Stone current, int color, Func<Stone, int, int, bool> func) {
			bool result = func(current, color, Direct.Top) || func(current, color, Direct.RightTop) || func(current, color, Direct.Right) ||
				func(current, color, Direct.RightBottom) || func(current, color, Direct.Bottom) || func(current, color, Direct.LeftBottom) ||
				func(current, color, Direct.Left) || func(current, color, Direct.LeftTop);

			return result;
		}

		private void Do(Stone current, int color, Action<Stone, int, int> action) {
			action(current, color, Direct.Top);
			action(current, color, Direct.RightTop);
			action(current, color, Direct.Right);
			action(current, color, Direct.RightBottom);
			action(current, color, Direct.Bottom);
			action(current, color, Direct.LeftBottom);
			action(current, color, Direct.Left);
			action(current, color, Direct.LeftTop);
		}

		private int Count(Stone current, int color, Func<Stone, int, int, int> func) {
			int result = func(current, color, Direct.Top) + func(current, color, Direct.RightTop) + func(current, color, Direct.Right) +
				func(current, color, Direct.RightBottom) + func(current, color, Direct.Bottom) + func(current, color, Direct.LeftBottom) +
				func(current, color, Direct.Left) + func(current, color, Direct.LeftTop);

			return result;
		}

		#endregion
	}
}
