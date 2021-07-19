using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Visuals
{
	public class TouhouFaceGenerationParams
	{
		private FaceGenerationParams _vanillaParams;

		public int Seed
		{
			set => _vanillaParams.seed_ = value;
			get => _vanillaParams.seed_;
		}

		public int Gender
		{
			set => _vanillaParams._currentGender = value;
			get => _vanillaParams._currentGender;
		}

		/*public int CurBeard
		{
			set => _vanillaParams._curBeard = value;
			get => _vanillaParams._curBeard;
		}*/

		public int HairFront { set; get; }

		public int HairBack
		{
			set => _vanillaParams._currentHair = value;
			get => _vanillaParams._currentHair;
		}

		public int Eyebrow
		{
			set => _vanillaParams._curEyebrow = value;
			get => _vanillaParams._curEyebrow;
		}

		public int FaceTexture
		{
			set => _vanillaParams._curFaceTexture = value;
			get => _vanillaParams._curFaceTexture;
		}

		public int MouthTexture
		{
			set => _vanillaParams._curMouthTexture = value;
			get => _vanillaParams._curMouthTexture;
		}

		public int FaceTattoo
		{
			set => _vanillaParams._curFaceTattoo = value;
			get => _vanillaParams._curFaceTattoo;
		}

		public int Voice
		{
			set => _vanillaParams._currentVoice = value;
			get => _vanillaParams._currentVoice;
		}

		public int HairFilter
		{
			set => _vanillaParams.hair_filter_ = value;
			get => _vanillaParams.hair_filter_;
		}

		public int BeardFilter
		{
			set => _vanillaParams.beard_filter_ = value;
			get => _vanillaParams.beard_filter_;
		}

		public int TattooFilter
		{
			set => _vanillaParams.tattoo_filter_ = value;
			get => _vanillaParams.tattoo_filter_;
		}

		public int FaceTextureFilter
		{
			set => _vanillaParams.face_texture_filter_ = value;
			get => _vanillaParams.face_texture_filter_;
		}

		public float TattooZeroProbability
		{
			set => _vanillaParams._tattooZeroProbability = value;
			get => _vanillaParams._tattooZeroProbability;
		}

		public float[] KeyWeights
		{
			get => _vanillaParams.KeyWeights;
		}

		public float Age
		{
			set => _vanillaParams._curAge = value;
			get => _vanillaParams._curAge;
		}

		public float Weight
		{
			set => _vanillaParams._curWeight = value;
			get => _vanillaParams._curWeight;
		}

		public float Build
		{
			set => _vanillaParams._curBuild = value;
			get => _vanillaParams._curBuild;
		}

		public int SkinColorIndex
		{
			set => _vanillaParams._curSkinColorOffset = MathF.Clamp(value / (float)(SkinManager.SkinColorCount - 1), 0, 1);
			get => (int) Math.Round(_vanillaParams._curSkinColorOffset * (SkinManager.SkinColorCount - 1));
		}

		public int HairColor1Index
		{
			set => _vanillaParams._curHairColorOffset = MathF.Clamp(value / (float)(SkinManager.HairColorCount - 1), 0, 1);
			get => (int)Math.Round(_vanillaParams._curHairColorOffset * (SkinManager.HairColorCount - 1));
		}

		public int HairColor2Index { set; get; }

		public int EyeColor1Index
		{
			set => _vanillaParams._curEyeColorOffset = MathF.Clamp(value / (float)(SkinManager.EyeColorCount - 1), 0, 1);
			get => (int)Math.Round(_vanillaParams._curEyeColorOffset * (SkinManager.EyeColorCount - 1));
		}

		public int EyeColor2Index { set; get; }

		public float FaceDirt
		{
			set => _vanillaParams.face_dirt_amount_ = value;
			get => _vanillaParams.face_dirt_amount_;
		}

		public float FaceTattooColorOffset
		{
			set => _vanillaParams._curFaceTattooColorOffset1 = value;
			get => _vanillaParams._curFaceTattooColorOffset1;
		}

		public float Height
		{
			set => _vanillaParams._heightMultiplier = value;
			get => _vanillaParams._heightMultiplier;
		}

		public float VoicePitch
		{
			set => _vanillaParams._voicePitch = value;
			get => _vanillaParams._voicePitch;
		}

		public float Breast { set; get; }

		public TouhouFaceGenerationParams()
		{
			_vanillaParams = FaceGenerationParams.Create();
		}

		public TouhouFaceGenerationParams(BodyProperties bodyProperties, bool isEarHidden = false)
		{
			MBBodyProperties.GetParamsFromKey(ref _vanillaParams, bodyProperties, isEarHidden);
		}

		public TouhouFaceGenerationParams(BodyGenerator bodyGenerator)
		{
			bodyGenerator.CurrentBodyProperties = bodyGenerator.Character.GetBodyProperties(bodyGenerator.Character.Equipment);
			FaceGenerationParams faceGenerationParams = FaceGenerationParams.Create();
			faceGenerationParams._currentGender = bodyGenerator.Character.IsFemale ? 1 : 0;
			faceGenerationParams._curAge = bodyGenerator.Character.Age;
			MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, bodyGenerator.CurrentBodyProperties, bodyGenerator.Character.Equipment.EarsAreHidden);
			faceGenerationParams.SetGenderAndAdjustParams(faceGenerationParams._currentGender, (int)faceGenerationParams._curAge);
			_vanillaParams = faceGenerationParams;
		}

		public bool EnforceConstraints()
		{
			return MBBodyProperties.EnforceConstraints(ref _vanillaParams);
		}

		public void RefreshFace(BodyGenerator bodyGenerator)
		{
			MBBodyProperties.ProduceNumericKeyWithParams(_vanillaParams, bodyGenerator.Character.Equipment.EarsAreHidden, ref bodyGenerator.CurrentBodyProperties);
			var writeableStaticProperties = new BodyPropertiesUtil.WriteableStaticBodyProperties(
				bodyGenerator.CurrentBodyProperties.StaticProperties);

			writeableStaticProperties.SetBreastSize(Breast);

			var staticProperties = (StaticBodyProperties) writeableStaticProperties;
			bodyGenerator.CurrentBodyProperties = new BodyProperties(
				bodyGenerator.CurrentBodyProperties.DynamicProperties, staticProperties);
			bodyGenerator.IsFemale = Gender == 1;
		}
	}
}
