using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Visuals
{
	public static class BodyPropertiesUtil
	{
		/*
		 * KeyPart1:
		 * xxxx xxxx xxcc cccc
		 * eeee eexx xxxx rrrr
		 * rraa aaaa oooo oott
		 * tttt bbbb bbhh hhhh
		 * c: skin color
		 * e: (left) eye color
		 * r: hair color
		 * a: tatoo
		 * o: tatoo color
		 * t: face tex
		 * b: beard mesh
		 * h: (back) hair mesh
		 */

		/*
		 * KeyPart7:
		 * xxxx pppp ppss ssss
		 * xxxx xxxx xxxx xxxx
		 * xxgg gggg ffff ffee
		 * eeee llll llrr rrrr
		 * p: new: hair pin mesh
		 * s: new: sock mesh
		 * g: new: gradient color of hair
		 * f: new: factor of gradient
		 * e: new: right eye color. (ignore if two eyes are same color)
		 * l: new: left eye type
		 * r: new: right eye type. (ignore if two eyes are same type)
		 */

		/*
		 * KeyPart8:
		 * oooo oomm mmmm iiii
		 * ssss saaa aaaa cccc
		 * xppp ppph hhhh hubb
		 * bbbb tttt ttvv vvvv
		 * o: new: top underwear mesh
		 * m: new: bottom underwear mesh
		 * i: new: 4-bits bitmap. from high to low: is hair gradient; are eyes two colors; are eyes two types; are eyes two light colors
		 * s: new: side hair mesh. 5-bits, only 31 variations available
		 * a: new: front hair mesh. 7-bits. 127 variations available
		 * c: new: breast. 4-bits. [0.0, 1.0] range with 0.0625 precision
		 * p: voice pitch
		 * h: height
		 * u: unknown 1-bit bool
		 * b: eyebrows mesh
		 * t: teeth material
		 * v: voice
		 */

		private static uint[] _skinColorLookup;

		static BodyPropertiesUtil()
		{
			var skinColorList = MBBodyProperties.GetSkinColorGradientPoints(1, 20);
			var skinColorRange = skinColorList.Count - 1;
			_skinColorLookup = new uint[64];
			for (int i = 0; i < 64; i++)
			{
				var fIndex = i / 63f * skinColorRange;
				var index1 = (int) Math.Floor(fIndex);
				var index2 = Math.Min(index1 + 1, skinColorRange);
				var color1 = skinColorList[index1];
				var color2 = skinColorList[index2];
				var interpolation = fIndex - index1;
				_skinColorLookup[i] = GetInterpolatedColor(color1, color2, interpolation);
			}
		}

		private static uint GetInterpolatedColor(uint c1, uint c2, float lerp)
		{
			lerp = MBMath.ClampFloat(lerp, 0f, 1f);
			Vec3 color = MBMath.Lerp(UIntToVec3(c1), UIntToVec3(c2), lerp, MBMath.Epsilon);
			return MBMath.ColorFromRGBA(color.X, color.Y, color.Z, 1f);
		}

		private static Vec3 UIntToVec3(uint color)
		{
			return new Vec3(((color >> 16) & 0xFF) / 255f, ((color >> 8) & 0xFF) / 255f, (color & 0xFF) / 255f);
		}

		public static int GetSkinColorIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart1 >> 48) & 0x3F);
		}

		public static uint GetSkinColor(this BodyProperties bodyProperties)
		{
			var index = GetSkinColorIndex(bodyProperties);
			return _skinColorLookup[index];
		}

		public static int GetHairColorIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart1 >> 30) & 0x3F);
		}

		public static int GetTatooIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart1 >> 24) & 0x3F);
		}

		public static int GetTatooColorIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart1 >> 18) & 0x3F);
		}

		public static int GetFaceMaterialIndex(this BodyProperties bodyProperties)
		{
			return (int) ((bodyProperties.KeyPart1 >> 12) & 0x3F);
		}

		public static int GetHairFrontIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart8 >> 36) & 0x7F);
		}

		public static void SetHairFrontIndex(this ref WriteableStaticBodyProperties bodyProperties, int index)
		{
			SetProperty(ref bodyProperties.KeyPart8, index, 0x7F, 36);
		}

		public static int GetHairBackIndex(this BodyProperties bodyProperties)
		{
			return (int)(bodyProperties.KeyPart1 & 0x3F);
		}

		public static float GetHeight(this BodyProperties bodyProperties)
		{
			var index = (int)((bodyProperties.KeyPart8 >> 19) & 0x3F);
			return MBMath.ClampFloat(index / 63f, 0f, 1f);
		}

		public static float GetBreastSize(this BodyProperties bodyProperties)
		{
			var index = (int)((bodyProperties.KeyPart8 >> 32) & 0xF);
			return MBMath.ClampFloat(index / 15f, 0f, 1f);
		}

		public static void SetBreastSize(this ref WriteableStaticBodyProperties bodyProperties, float size)
		{
			SetProperty(ref bodyProperties.KeyPart8, size, 15, 0xF, 32);
		}

		public static int GetEyebrowsIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart8 >> 12) & 0x3F);
		}

		public static int GetEyeColorIndex(this BodyProperties bodyProperties)
		{
			return (int)((bodyProperties.KeyPart1 >> 42) & 0x3F);
		}

		public static uint GetEyeColor(this BodyProperties bodyProperties)
		{
			return SkinManager.EyeColors[bodyProperties.GetEyeColorIndex()].Color.ToUnsignedInteger();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetProperty(ref ulong keyPart, int value, int unoffsetMask, int offset)
		{
			keyPart = (keyPart & ~((ulong)unoffsetMask << offset)) | ((ulong)value << offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetProperty(ref ulong keyPart, float value, int maxValue, int unoffsetMask, int offset)
		{
			SetProperty(ref keyPart, MBMath.Round(MBMath.ClampFloat(value, 0, 1) * maxValue), unoffsetMask, offset);
		}

		public struct WriteableStaticBodyProperties
		{
			public ulong KeyPart1;
			public ulong KeyPart2;
			public ulong KeyPart3;
			public ulong KeyPart4;
			public ulong KeyPart5;
			public ulong KeyPart6;
			public ulong KeyPart7;
			public ulong KeyPart8;

			public WriteableStaticBodyProperties(StaticBodyProperties bodyProperties)
			{
				KeyPart1 = bodyProperties.KeyPart1;
				KeyPart2 = bodyProperties.KeyPart2;
				KeyPart3 = bodyProperties.KeyPart3;
				KeyPart4 = bodyProperties.KeyPart4;
				KeyPart5 = bodyProperties.KeyPart5;
				KeyPart6 = bodyProperties.KeyPart6;
				KeyPart7 = bodyProperties.KeyPart7;
				KeyPart8 = bodyProperties.KeyPart8;
			}

			public static explicit operator StaticBodyProperties(WriteableStaticBodyProperties bodyProperties)
			{
				return new StaticBodyProperties(
					bodyProperties.KeyPart1,
					bodyProperties.KeyPart2,
					bodyProperties.KeyPart3,
					bodyProperties.KeyPart4,
					bodyProperties.KeyPart5,
					bodyProperties.KeyPart6,
					bodyProperties.KeyPart7,
					bodyProperties.KeyPart8
				);
			}
		}
	}
}