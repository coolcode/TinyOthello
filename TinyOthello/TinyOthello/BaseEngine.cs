using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public abstract class BaseEngine : IEngine {
		protected readonly Rule rule = new Rule();

		public virtual string Name {
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// 中局搜索
		/// </summary>
		public abstract SearchResult Search(Board board, int color,int depth);

		//protected abstract int SearchInternal(Board board, int alpha, int beta, int depth);
	}
}
