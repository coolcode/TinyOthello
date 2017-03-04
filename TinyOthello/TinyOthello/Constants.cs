using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public class Constants {
		public const int Line = 4;
		public const int StoneCount = Line * Line;
        public const int MaxEmptyNum = StoneCount - 4;
		public const int HighestScore = 10000000;
        public static readonly int MaxEndGameDepth = Math.Min((StoneCount - 4), 20);

    }
}
