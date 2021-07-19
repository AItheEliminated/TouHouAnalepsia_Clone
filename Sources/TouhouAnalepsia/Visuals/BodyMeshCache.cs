using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Visuals
{
	public static class BodyMeshCache
	{
		private const int BODY_LEVEL_NUM = 16;

		private const int HEAD_TYPE_NUM = 4;

		private static MetaMesh[] _femaleBodyMeshCache;

		private static Tuple<Mesh, uint>[][] _headMeshes;

		private static MetaMesh[] _headMetaMeshes;

		public static void Init()
		{
			_femaleBodyMeshCache = new MetaMesh[BODY_LEVEL_NUM];

			for (int i = 0; i < BODY_LEVEL_NUM; i++)
			{
				_femaleBodyMeshCache[i] = CreateMorphedCopy("body_female_th", i / (float) (BODY_LEVEL_NUM - 1));
			}

			_headMeshes = new Tuple<Mesh, uint>[HEAD_TYPE_NUM][];
			_headMeshes[0] = CreateFaceMeshCache("head_female_th");

			_headMetaMeshes = new MetaMesh[HEAD_TYPE_NUM];
			_headMetaMeshes[0] = MetaMesh.GetCopy("head_female_th");
			//MetaMesh.GetMultiMesh("head_female_th", ref _headMetaMeshes[0]);
		}

		public static void DeInit()
		{
			foreach (var metaMesh in _femaleBodyMeshCache)
			{
				var count = metaMesh.MeshCount;
				for (int i = 0; i < count; i++)
				{
					metaMesh.GetMeshAtIndex(i).ReleaseEditDataUser();
				}
				metaMesh.ManualInvalidate();
			}

			_femaleBodyMeshCache = null;

			foreach (var meshes in _headMeshes)
			{
				if (meshes != null)
				{
					foreach (var mesh in meshes)
					{
						mesh.Item1.ManualInvalidate();
					}
				}
			}

			_headMeshes = null;

			foreach (var metaMesh in _headMetaMeshes)
			{
				if (metaMesh != null)
					metaMesh.ManualInvalidate();
			}

			_headMetaMeshes = null;
		}

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

		private static Tuple<Mesh, uint>[] CreateFaceMeshCache(string metaMeshName)
		{
			var headMetaMesh = MetaMesh.GetCopy(metaMeshName);
			var headMeshes = new List<Tuple<Mesh, uint>>();
			for (int i = 0, j = headMetaMesh.MeshCount; i < j; i++)
			{
				var mesh = headMetaMesh.GetMeshAtIndex(i);
				if (mesh.HasTag("face"))
				{
					uint lod = CountTrailingZeros((uint)headMetaMesh.GetLodMaskForMeshAtIndex(i));
					headMeshes.Add(Tuple.Create(mesh.CreateCopy(), lod));
				}
			}
			headMetaMesh.ManualInvalidate();
			return headMeshes.ToArray();
		}

		public static MetaMesh CreateBodyCopy(float time)
		{
			int index = GetIndex(time);
			return _femaleBodyMeshCache[index].CreateCopy();
		}

		/*public static Tuple<Mesh, uint>[] GetHeadMeshes(int type)
		{
			type = type < 0 || type >= HEAD_TYPE_NUM ? 0 : type;
			return _headMeshes[type];
		}*/

		public static MetaMesh GetHeadMesh(int type)
		{
			type = type < 0 || type >= HEAD_TYPE_NUM ? 0 : type;
			return _headMetaMeshes[type].CreateCopy();
		}

		public static void FixupHeadMesh(MetaMesh metaMesh, BodyProperties bodyProperties)
		{
			foreach (var mesh in metaMesh.Enumerate())
			{
				if (mesh.HasTag("face"))
				{
					mesh.Color = bodyProperties.GetSkinColor();
				}
				else if (mesh.HasTag("eye"))
				{
					//mesh.Color = bodyProperties.GetEyeColor();
				}
				else if (mesh.HasTag("outline"))
				{
					mesh.SetMaterial(Material.GetFromResource("outline_head_female"));
				}
			}
		}

		private static int GetIndex(float time)
		{
			return (int)(MBMath.ClampFloat(time, 0f, 1f) * (BODY_LEVEL_NUM - 1));
		}

		private static uint CountTrailingZeros(uint value)
		{
			if (value == 0)
				return 32;

			uint result = 0;
			if ((value & 0x0000FFFFu) == 0)
			{
				result = result + 16;
				value = value >> 16;
			}
			if ((value & 0x000000FFu) == 0)
			{
				result = result + 8;
				value = value >> 8;
			}
			if ((value & 0x0000000Fu) == 0)
			{
				result = result + 4;
				value = value >> 4;
			}
			if ((value & 0x00000003u) == 0)
			{
				result = result + 2;
				value = value >> 2;
			}
			if ((value & 0x00000001u) == 0)
			{
				result = result + 1;
			}

			return result;
		}
	}
}