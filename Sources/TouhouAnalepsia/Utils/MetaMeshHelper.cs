using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TaleWorlds.Engine;

namespace TouhouAnalepsia.Utils
{
	public static class MetaMeshHelper
	{
		public static IEnumerable<Mesh> Enumerate([NotNull] this MetaMesh metaMesh)
		{
			if (metaMesh == null)
				throw new ArgumentNullException(nameof(metaMesh));

			return new MetaMeshEnumerable(metaMesh);
		}

		private class MetaMeshEnumerable : IEnumerable<Mesh>
		{
			private readonly MetaMesh _metaMesh;

			public MetaMeshEnumerable(MetaMesh metaMesh)
			{
				_metaMesh = metaMesh;
			}

			public IEnumerator<Mesh> GetEnumerator()
			{
				return new MetaMeshEnumerator(_metaMesh);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class MetaMeshEnumerator : IEnumerator<Mesh>
		{
			private MetaMesh _metaMesh;

			private int _pointer = -1;

			public Mesh Current => _metaMesh.GetMeshAtIndex(_pointer);

			object IEnumerator.Current => _metaMesh.GetMeshAtIndex(_pointer);

			public MetaMeshEnumerator(MetaMesh metaMesh)
			{
				_metaMesh = metaMesh;
			}

			public void Dispose()
			{
				_metaMesh = null;
			}

			public bool MoveNext()
			{
				return ++_pointer < _metaMesh.MeshCount;
			}

			public void Reset()
			{
				_pointer = -1;
			}
		}
	}
}