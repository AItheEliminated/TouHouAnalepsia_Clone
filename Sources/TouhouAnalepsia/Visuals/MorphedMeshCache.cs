using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Visuals
{
	public static class MorphedMeshCache
	{
		private const int MAX_CACHE_NUM = 128;

		private const int CACHE_LEVEL_NUM = 16;

		private static int _cacheNum = 0;

		private static Dictionary<string, MetaMesh[]> _caches = new Dictionary<string, MetaMesh[]>(256);

		private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		public static MetaMesh GetMorphedCopy(string metaMeshName, float morphTarget)
		{
			MetaMesh[] cache = null;
			MetaMesh result = null;
			int index = GetIndex(morphTarget);

			_lock.EnterReadLock();
			try
			{
				if (_caches.TryGetValue(metaMeshName, out cache))
				{
					result = cache[index];
				}
			}
			finally
			{
				_lock.ExitReadLock();
			}

			if (result == null)
			{
				_lock.EnterUpgradeableReadLock();
				try
				{
					if (_caches.TryGetValue(metaMeshName, out cache))
					{
						result = cache[index];
					}
					if (result == null)
					{
						_lock.EnterWriteLock();
						try
						{
							cache = new MetaMesh[CACHE_LEVEL_NUM];
							result = CreateMorphedCopy(metaMeshName, morphTarget);
							cache[index] = result;
							_caches[metaMeshName] = cache;
						}
						finally
						{
							_lock.ExitWriteLock();
						}
					}
				}
				finally
				{
					_lock.ExitUpgradeableReadLock();
				}
			}

			return result.CreateCopy();
		}

		public static void Clear()
		{
			_lock.EnterWriteLock();
			try
			{
				foreach (var pair in _caches)
				{
					var cache = pair.Value;
					for (var i = 0; i < cache.Length; i++)
					{
						var metaMesh = cache[i];
						if (metaMesh != null)
						{
							var count = metaMesh.MeshCount;
							for (int j = 0; j < count; j++)
							{
								metaMesh.GetMeshAtIndex(j).ReleaseEditDataUser();
							}
							metaMesh.ManualInvalidate();
						}
					}
				}
				_caches.Clear();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		// XXX: same to the one in BodyMeshCache. should merge them to one?
		private static MetaMesh CreateMorphedCopy(string metaMeshName, float time)
		{
			var metamesh = MetaMesh.GetCopy(metaMeshName);
			time = MBMath.ClampFloat(time, 0f, 1f);
			for (int i = 0, j = metamesh.MeshCount; i < j; i++)
			{
				var mesh = metamesh.GetMeshAtIndex(i);
				mesh.AddEditDataUser();
				mesh.MorphTime = time;
			}
			metamesh.CheckDeformToTime();
			metamesh.ValidateRenderBuffers(true);
			return metamesh;
		}

		private static int GetIndex(float time)
		{
			return (int)(MBMath.ClampFloat(time, 0f, 1f) * (CACHE_LEVEL_NUM - 1));
		}
	}
}