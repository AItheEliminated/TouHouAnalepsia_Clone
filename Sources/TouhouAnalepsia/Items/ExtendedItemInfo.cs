using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TouhouAnalepsia.BattleSystem.Firearms;
using TouhouAnalepsia.Utils.Inits;

namespace TouhouAnalepsia.Items
{
	public class ExtendedItemInfo
	{
		private static ExtendedItemInfo emptyItem = new ExtendedItemInfo()
		{
			IsEmpty = true,
			MuzzlePosition = FirearmBehavior.DEFAULT_MUZZLE_POSITION,
			GunshotFlash = FirearmBehavior.DEFAULT_GUNSHOT_FLASH
			/*ArmCover = CoverType.None,
			BodyCover = CoverType.None,
			FootHorizontalCover = CoverType.None,
			FootVerticalCover = CoverType.None,
			LegCover = CoverType.None,
			HigherFoot = false*/
		};

		private static readonly string[] emptyParams = new string[0];

		private static Dictionary<MBGUID, ExtendedItemInfo> extendedItems = 
					new Dictionary<MBGUID, ExtendedItemInfo>();

		private const int OFFSET_IS_EMPTY = 0;
		private const int OFFSET_IS_HIGHER_FOOT = 1;
		private const int OFFSET_IS_HIDE_UNDERWEAR_TOP = 2;
		private const int OFFSET_IS_HIDE_UNDERWEAR_BOTTOM = 3;
		private const int OFFSET_BODY_COVER = 16;
		private const int OFFSET_ARM_COVER = 18;
		private const int OFFSET_FOOT_HORIZONTAL_COVER = 20;
		private const int OFFSET_FOOT_VERTICAL_COVER = 22;
		private const int OFFSET_LEG_COVER = 24;
		private const int OFFSET_FIREARM_TRIGGER = 26;

		protected int _storage1;

		public bool IsEmpty {
			internal set => SetStorage1(OFFSET_IS_EMPTY, value);
			get => GetStorage1Bool(OFFSET_IS_EMPTY);
		}

		public string ItemScript { internal set; get; } = string.Empty;

		public string[] ItemScriptParams { internal set; get; } = emptyParams;

		public CoverType BodyCover
		{
			internal set => SetStorage1(OFFSET_BODY_COVER, value);
			get => GetStorage1CoverType(OFFSET_BODY_COVER);
		}

		public CoverType ArmCover
		{
			internal set => SetStorage1(OFFSET_ARM_COVER, value);
			get => GetStorage1CoverType(OFFSET_ARM_COVER);
		}

		public CoverType FootHorizontalCover
		{
			internal set => SetStorage1(OFFSET_FOOT_HORIZONTAL_COVER, value);
			get => GetStorage1CoverType(OFFSET_FOOT_HORIZONTAL_COVER);
		}

		public CoverType FootVerticalCover
		{
			internal set => SetStorage1(OFFSET_FOOT_VERTICAL_COVER, value);
			get => GetStorage1CoverType(OFFSET_FOOT_VERTICAL_COVER);
		}

		public CoverType LegCover
		{
			internal set => SetStorage1(OFFSET_LEG_COVER, value);
			get => GetStorage1CoverType(OFFSET_LEG_COVER);
		}

		public bool HigherFoot
		{
			internal set => SetStorage1(OFFSET_IS_HIGHER_FOOT, value);
			get => GetStorage1Bool(OFFSET_IS_HIGHER_FOOT);
		}

		public bool HideUnderwearTop
		{
			internal set => SetStorage1(OFFSET_IS_HIDE_UNDERWEAR_TOP, value);
			get => GetStorage1Bool(OFFSET_IS_HIDE_UNDERWEAR_TOP);
		}

		public bool HideUnderwearBottom
		{
			internal set => SetStorage1(OFFSET_IS_HIDE_UNDERWEAR_BOTTOM, value);
			get => GetStorage1Bool(OFFSET_IS_HIDE_UNDERWEAR_BOTTOM);
		}

		public FirearmTrigger WeaponTrigger
		{
			internal set => SetStorage1(OFFSET_FIREARM_TRIGGER, value);
			get => GetStorage1FirearmTrigger(OFFSET_FIREARM_TRIGGER);
		}

		public int ClipCapacity { internal set; get; } = 1;

		[CanBeNull]
		public string GunshotFlash { internal set; get; } = null; // null for no gunshot flash

		public int GunshotSoundCode { internal set; get; } = 0; // -1 for no sound, 0 for default sound, 1 for others

		public Vec3 MuzzlePosition { internal set; get; } = Vec3.Zero;

		public Vec3 VectorArgument1 { internal set; get; } = new Vec3();

		public Vec3 VectorArgument2 { internal set; get; } = new Vec3();

		private const float EPSILON = 0.01f;

		public float GetBodyCoverShaderParam()
		{
			switch (BodyCover)
			{
				default:
					return 0;
				case CoverType.Type1:
					return 0.25f + EPSILON;
				case CoverType.Type2:
					return 0.5f + EPSILON;
				case CoverType.Type3:
					return 1 + EPSILON;
			}
		}

		public float GetArmCoverShaderParam()
		{
			switch (ArmCover)
			{
				default:
					return 0;
				case CoverType.Type1:
					return 0.25f + EPSILON;
				case CoverType.Type2:
					return 0.5f + EPSILON;
				case CoverType.Type3:
					return 0.75f + EPSILON;
			}
		}

		public float GetFootHorizontalCoverShaderParam()
		{
			switch (FootHorizontalCover)
			{
				default:
					return 0;
				case CoverType.Type1:
					return 0.25f + EPSILON;
				case CoverType.Type2:
					return 0.5f + EPSILON;
				case CoverType.Type3:
					return 0.75f + EPSILON;
			}
		}

		public float GetFootVerticalCoverShaderParam()
		{
			switch (FootVerticalCover)
			{
				default:
					return 0;
				case CoverType.Type1:
					return 0.25f + EPSILON;
				case CoverType.Type2:
					return 0.5f + EPSILON;
				case CoverType.Type3:
					return 0.75f + EPSILON;
			}
		}

		public float GetLegCoverShaderParam()
		{
			switch (LegCover)
			{
				default:
					return 0;
				case CoverType.Type1:
					return 0.25f + EPSILON;
				case CoverType.Type2:
					return 0.5f + EPSILON;
				case CoverType.Type3:
					return 0.75f + EPSILON;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetStorage1(int offset, bool v)
		{
			if (v)
				_storage1 |= (1 << offset);
			else
				_storage1 &= ~(1 << offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetStorage1(int offset, CoverType v)
		{
			_storage1 = (_storage1 & ~((int)CoverType.Mask << offset)) | ((int)v << offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetStorage1(int offset, FirearmTrigger v)
		{
			_storage1 = (_storage1 & ~((int)FirearmTrigger.Mask << offset)) | ((int)v << offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetStorage1Bool(int offset)
		{
			return (_storage1 & (1 << offset)) > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private CoverType GetStorage1CoverType(int offset)
		{
			return (CoverType)((_storage1 >> offset) & (int)CoverType.Mask);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private FirearmTrigger GetStorage1FirearmTrigger(int offset)
		{
			return (FirearmTrigger)((_storage1 >> offset) & (int)FirearmTrigger.Mask);
		}

		[InitOnGameCreate]
		public static void Clear()
		{
			extendedItems.Clear();
		}

		public static void Set(ItemObject item, ExtendedItemInfo itemInfo)
		{
			Set(item.Id, itemInfo);
		}

		public static void Set(MBGUID guid, ExtendedItemInfo itemInfo)
		{
			extendedItems[guid] = itemInfo;
		}

		public static ExtendedItemInfo Get([CanBeNull] ItemObject item)
		{
			if (item == null)
				return emptyItem;
			var key = item.Id;
			return Get(key);
		}

		public static ExtendedItemInfo Get(MBGUID guid)
		{
			if (extendedItems.TryGetValue(guid, out var itemInfo))
				return itemInfo;
			return emptyItem;
		}

		public static bool TryGet([CanBeNull] ItemObject item, out ExtendedItemInfo itemInfo)
		{
			if (item == null)
			{
				itemInfo = emptyItem;
				return false;
			}
			var key = item.Id;
			return TryGet(key, out itemInfo);
		}

		public static bool TryGet(MBGUID guid, out ExtendedItemInfo itemInfo)
		{
			return extendedItems.TryGetValue(guid, out itemInfo);
		}

		public enum CoverType
		{
			None = 0,
			Type1 = 1,
			Type2 = 2,
			Type3 = 3,
			Mask = 0x3
		}

		public enum FirearmTrigger
		{
			Single = 0, // musket
			Bolt = 1, // mosin, 98k
			Semi = 2, // m1, m14, sks
			//Auto // sorry no automatic firearm for now
			Mask = 0x7
		}
	}
}