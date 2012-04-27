using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello {
	public static class Extensions {
		public static void ForEach<T>(this IEnumerable<T> list, Action<T> action) {
			foreach (T t in list)
				action(t);
		}

		public static void ForEach<T>(this IEnumerable<T> list, Action<T, int> action) {
			int i = 0;
			foreach (T t in list)
				action(t, i++);
		}

		public static IEnumerable<TResult> Fight<T, TResult>(this IEnumerable<T> list, Func<T, T, TResult> func) where T : class { 
			foreach (T t1 in list)
				foreach (T t2 in list)
					if (t1 != t2)
						yield return func(t1, t2);
		}

		public static void Fight<T>(this IEnumerable<T> list, Action<T, T> action) where T : class { 
			foreach (T t1 in list)
				foreach (T t2 in list)
					if (t1 != t2)
						action(t1, t2);
		}

		public static int Opp(this int color) {
			return color ^ 3;
		}
	}
}
