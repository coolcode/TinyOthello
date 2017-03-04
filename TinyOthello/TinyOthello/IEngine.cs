using System;
using System.Collections.Generic;

namespace TinyOthello {
	public interface IEngine {
		string Name { get; }
		SearchResult Search(Board board, int color, int depth);
	}

	public class SearchResult {
		public int Move { get; set; }
		public int Score { get; set; }
		public string Message { get; set; }
		public TimeSpan TimeSpan { get; set; }
		public int Nodes { get; set; }
        public List<EvalItem> EvalList { get; set; } = new List<EvalItem>();

		public SearchResult() {
			Message = string.Empty;
		}

		public override string ToString() {
			return string.Format("Best Move:{0}, Score:{1:N}, Nodes:{2}, TimeSpan:{3}, Message:{4}, NPS:{5}",
			                     Move,
			                     Score,
			                     Nodes,
			                     TimeSpan,
			                     Message,
								 Nodes/ (TimeSpan.TotalSeconds + 0.000001));
		}
	}

    public class EvalItem
    {
        public int Move { get; set; }
        public int Score { get; set; }
    }
}
