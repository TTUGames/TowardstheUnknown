using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListShuffler<T>
{
    public static void Shuffle(List<T> list) {
		list.Sort(new RandomComparer<T>());
	}

	private class RandomComparer<T> : Comparer<T> {
		public override int Compare(T x, T y) {
			return Random.Range(0, 2) == 0 ? -1 : 1;
		}
	}
}
