using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils
{
	public sealed class WeightedRandom<T>
	{
		private static Random _random = new Random();

		private T[] _items;
		private float[] _weights;
		private float _totalWeight;

		public WeightedRandom([NotNull] params Tuple<T, float>[] items)
		{
			_totalWeight = 0f;
			var tempList = new List<Tuple<T, float>>();

			for (var i = 0; i < items.Length; i++)
			{
				var item = items[i].Item1;
				var weight = items[i].Item2;
				if (IsWeightValid(weight))
				{
					tempList.Add(Tuple.Create(item, _totalWeight));
					_totalWeight += weight;
				}
			}

			_items = new T[tempList.Count];
			_weights = new float[tempList.Count];
			for (var i = 0; i < tempList.Count; i++)
			{
				var tuple = tempList[i];
				_items[i] = tuple.Item1;
				_weights[i] = tuple.Item2;
			}
		}

		public T Get()
		{
			if (_weights.Length == 0)
				return default(T);
			float expect = (float) (_random.NextDouble() * _totalWeight);
			int low = 0, high = _weights.Length - 1;
			while (low <= high)
			{
				int mid = low + ((high - low) >> 1);
				float weight = _weights[mid];
				if (weight <= expect)
				{
					int next = mid + 1;
					if (next >= _weights.Length || _weights[next] > expect)
					{
						return _items[mid];
					}
					low = mid + 1;
				}
				else
				{
					high = mid - 1;
				}
			}

			return _items[high];
		}

		private static bool IsWeightValid(float weight)
		{
			return !float.IsNaN(weight) && weight >= float.Epsilon && weight <= float.MaxValue;
		}
	}
}