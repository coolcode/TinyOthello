/*----------------------------------------------------------------
          // Copyright (C) 2007-2012 Bruce Lee
          // Authors：Bruce Lee (李明锋) 
          // Email: coolcode@live.com
          // Created:  Oct 14, 2009
          // Modified: Apr 22, 2012
          // Description： 棋子类
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public static class StoneType {
		public const int Empty = 0;
		public const int Black = 1;
		public const int White = 2;
		public const int Dummy = 10;
	}

	public static class Direct {
		public const int Top = 0;
		public const int RightTop = 1;
		public const int Right = 2;
		public const int RightBottom = 3;
		public const int Bottom = 4;
		public const int LeftBottom = 5;
		public const int Left = 6;
		public const int LeftTop = 7;
	}

	public class Stone : IEquatable<Stone> {
		public int Type { get; set; }

		private Stone[] neighbors = new Stone[8];

		public static readonly Stone Empty = new Stone(StoneType.Empty);

		public static readonly Stone Dummy = new Stone(StoneType.Dummy);

		public Stone()
			: this(StoneType.Empty) {
		}

		public Stone(int value) {
			Type = value;
		}

		public Stone this[int direct] {
			get {
				return neighbors[direct];
			}
			set {
				neighbors[direct] = value;
			}
		}

		public int Pos { get; internal set; }

		public int Row { get; set; }

		public int Column { get; set; }

		#region IEquatable<Stone> Members

		public bool Equals(Stone other) {
			return this.Type == other.Type;
		}

		public bool IsEmpty {
			get {
				return this.Type == StoneType.Empty;
			}
		}

		public bool IsDummy {
			get {
				return this.Type == StoneType.Dummy;
			}
		}

		public bool IsBorder {
			get {
				return Row == 0 || Row == Constants.Line - 1 || Column == 0 || Column == Constants.Line - 1;
			}
		}

		public bool IsOpponent(int color) {
			return (this.Type ^ color) == 3;
		}

		public bool Flip(int color) {
			if (IsOpponent(color)) {
				this.Type = color;
				return true;
			}
			return false;
		}

		public void Change(int color) {
			this.Type = color;
		}

		#endregion
	}
}
