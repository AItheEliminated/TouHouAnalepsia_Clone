using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TouhouAnalepsia.Utils.Inits;

namespace TouhouAnalepsia.Visuals
{
	public static class SkinManager
	{
		private static Regex _colorCheckerMatcher;

		private static PaletteColor[] _hairColors;

		private static PaletteColor[] _eyeColors;

		private static List<string> _legMeshNames;

		public static ReadOnlyCollection<PaletteColor> HairColors { private set; get; }

		public static ReadOnlyCollection<PaletteColor> EyeColors { private set; get; }

		public static int SkinColorCount;

		public static int HairColorCount;

		public static int EyeColorCount;

		private const int NUM_HAIR_COLORS = 64;

		private const int NUM_EYE_COLORS = 64;

		[InitOnSubModuleLoad]
		public static void Init()
		{
			_colorCheckerMatcher = new Regex(@"^#[0-9a-f]{8}?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			_hairColors = ReadColors("hair colors", "custom_hair_colors", 
				"hair_colors", "hair_color", NUM_HAIR_COLORS);
			_eyeColors = ReadColors("eye colors", "custom_eye_colors",
				"eye_colors", "eye_color", NUM_EYE_COLORS);

			HairColors = Array.AsReadOnly(_hairColors);
			EyeColors = Array.AsReadOnly(_eyeColors);

			HairColorCount = _hairColors.Length;
			EyeColorCount = _eyeColors.Length;

			_legMeshNames = new List<string>(64);
			_legMeshNames.Add("feet_female_th");
		}

		[InitOnBeforeInitialModuleScreenSetAsRoot]
		public static void PostInit()
		{
			SkinColorCount = MBBodyProperties.GetSkinColorCount(1, 24);
		}

		private static PaletteColor[] ReadColors(
			string type, string filename, 
			string rootName, string childName,
			int expectedNum)
		{
			var result = new PaletteColor[expectedNum];
			var xmlExt = MBObjectManager.GetMergedXmlForNative("custom_hair_colors", out var usedPaths);
			var colors = xmlExt.SelectSingleNode("/hair_colors")?.SelectNodes("hair_color");
			if (colors?.Count > 0)
			{
				var count = colors.Count;
				if (count > expectedNum)
				{
					count = expectedNum;
				}

				for (var i = 0; i < count; i++)
				{
					var hairColorNode = (XmlElement) colors[i];
					var id = hairColorNode.GetAttribute("id");
					var color = hairColorNode.GetAttribute("color");
					var tags = hairColorNode.GetAttribute("tags");

					if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var intId))
					{
						// todo: log
						continue;
					}

					if (intId < 0 || intId >= NUM_HAIR_COLORS)
					{
						// todo: log
						continue;
					}

					if (string.IsNullOrWhiteSpace(color) || !_colorCheckerMatcher.IsMatch(color))
					{
						// todo: log
						continue;
					}

					var splitTags = tags?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					result[intId] = new PaletteColor(intId, Color.ConvertStringToColor(color), splitTags);
				}
			}
			else
			{
				// todo: log
			}

			for (var i = 0; i < result.Length; i++)
			{
				if (result[i] == null)
				{
					// todo: log
					result[i] = new PaletteColor(i, Color.Black);
				}
			}

			return result;
		}

		public static MetaMesh GetLegsMetaMeshCopy(int sockIndex, bool higher)
		{
			if (sockIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(sockIndex), sockIndex, "SockIndex < 0");
			if (sockIndex >= _legMeshNames.Count)
				sockIndex = 0;

			var legName = _legMeshNames[sockIndex];
			MetaMesh metaMesh = null;

			if (higher)
			{
				metaMesh = MetaMesh.GetCopy(legName + "_higher", false, true);
			}

			if (metaMesh == null)
			{
				metaMesh = MetaMesh.GetCopy(legName);
			}

			return metaMesh;
		}

		public class PaletteColor
		{
			public int Id { private set; get; }

			public Color Color { private set; get; }

			public string[] Tags { private set; get; }

			internal PaletteColor(int id, Color color, params string[] tags)
			{
				this.Id = id;
				this.Color = color;
				if (tags?.Length > 0)
				{
					this.Tags = tags.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
				}
				else
				{
					this.Tags = new string[0];
				}
			}

			public override string ToString()
			{
				return Color.ToString();
			}
		}
	}
}