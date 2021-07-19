using System;
using System.Xml;
using System.Xml.Schema;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TouhouAnalepsia.BattleSystem.Firearms;

namespace TouhouAnalepsia.Items
{
	public static class ItemObjectPatch
	{
		[HarmonyPatch(typeof(TaleWorlds.Core.ItemObject))]
		[HarmonyPatch("Deserialize")]
		static class DeserializePatch
		{
			static void Postfix(ref TaleWorlds.Core.ItemObject __instance, XmlNode __1)
			{
				ExtendedItemInfo itemInfo = null;
				var hasContent = false;

				if (__1.Attributes["item_script"] is XmlAttribute scriptNode)
				{
					hasContent = true;
					itemInfo = itemInfo ?? new ExtendedItemInfo();
					itemInfo.ItemScript = scriptNode.Value;
					var paramNodes = __1.Attributes["item_script_params"];
					var paramNode = paramNodes ?? __1.Attributes["item_script_param"];
					if (paramNode != null)
						itemInfo.ItemScriptParams = paramNode.Value.Split(';');
				}

				if (__1.Attributes["vector_argument_1"] is XmlAttribute vecArgNode1)
				{
					hasContent = true;
					itemInfo = itemInfo ?? new ExtendedItemInfo();
					itemInfo.VectorArgument1 = Vec3.Parse(vecArgNode1.Value);
				}

				if (__1.Attributes["vector_argument_2"] is XmlAttribute vecArgNode2)
				{
					hasContent = true;
					itemInfo = itemInfo ?? new ExtendedItemInfo();
					itemInfo.VectorArgument2 = Vec3.Parse(vecArgNode2.Value);
				}

				if (__instance.ItemComponent is ArmorComponent)
				{
					var armorNode = __1.SelectSingleNode("ItemComponent/Armor");
					if (armorNode != null)
					{
						itemInfo = itemInfo ?? new ExtendedItemInfo();
						foreach (XmlAttribute attribute in armorNode.Attributes)
						{
							switch (attribute.Name)
							{
								case "body_cover_type":
									Enum.TryParse<ExtendedItemInfo.CoverType>(
										attribute.Value, true, out var bodyCoverType);
									itemInfo.BodyCover = bodyCoverType;
									break;
								case "arm_cover_type":
									Enum.TryParse<ExtendedItemInfo.CoverType>(
										attribute.Value, true, out var armCoverType);
									itemInfo.ArmCover = armCoverType;
									break;
								case "foot_horizontal_cover_type":
									Enum.TryParse<ExtendedItemInfo.CoverType>(
										attribute.Value, true, out var footHCoverType);
									itemInfo.FootHorizontalCover = footHCoverType;
									break;
								case "foot_vertical_cover_type":
									Enum.TryParse<ExtendedItemInfo.CoverType>(
										attribute.Value, true, out var footVCoverType);
									itemInfo.FootVerticalCover = footVCoverType;
									break;
								case "leg_cover_type":
									Enum.TryParse<ExtendedItemInfo.CoverType>(
										attribute.Value, true, out var legCoverType);
									itemInfo.LegCover = legCoverType;
									break;
								case "higher_foot":
									bool.TryParse(attribute.Value, out var higher_foot);
									itemInfo.HigherFoot = higher_foot;
									break;
								case "hide_underwear_top":
									bool.TryParse(attribute.Value, out var hide_underwear_top);
									itemInfo.HideUnderwearTop = hide_underwear_top;
									break;
								case "hide_underwear_bottom":
									bool.TryParse(attribute.Value, out var hide_underwear_bottom);
									itemInfo.HideUnderwearBottom = hide_underwear_bottom;
									break;
							}
						}

						if (itemInfo.BodyCover != ExtendedItemInfo.CoverType.None ||
						    itemInfo.ArmCover != ExtendedItemInfo.CoverType.None ||
						    itemInfo.FootHorizontalCover != ExtendedItemInfo.CoverType.None ||
						    itemInfo.FootVerticalCover != ExtendedItemInfo.CoverType.None ||
						    itemInfo.LegCover != ExtendedItemInfo.CoverType.None ||
						    itemInfo.HigherFoot)
						{
							hasContent = true;
						}
					}
				}

				if (__instance.ItemComponent is WeaponComponent)
				{
					var weaponNode = __1.SelectSingleNode("ItemComponent/Weapon");
					if (weaponNode != null)
					{
						itemInfo = itemInfo ?? new ExtendedItemInfo();
						string gunshotSound = null;
						string gunshotFlash = null;
						foreach (XmlAttribute attribute in weaponNode.Attributes)
						{
							switch (attribute.Name)
							{
								case "firearm_trigger":
									Enum.TryParse<ExtendedItemInfo.FirearmTrigger>(
										attribute.Value, true, out var firearmTrigger);
									itemInfo.WeaponTrigger = firearmTrigger;
									break;
								case "clip_capacity":
									int.TryParse(attribute.Value, out var clip);
									clip = Math.Max(clip, 1);
									itemInfo.ClipCapacity = clip;
									break;
								case "muzzle_position":
									Vec3 muzzlePosition = FirearmBehavior.DEFAULT_MUZZLE_POSITION;
									try
									{
										muzzlePosition = Vec3.Parse(attribute.Value);
									}
									catch (Exception e)
									{
										// TODO: log
									}
									itemInfo.MuzzlePosition = muzzlePosition;
									break;
								case "gunshot_flash":
									gunshotFlash = attribute.Value;
									break;
								case "gunshot_sound":
									gunshotSound = attribute.Value;
									break;
							}
						}

						if (itemInfo.WeaponTrigger != ExtendedItemInfo.FirearmTrigger.Single ||
						    itemInfo.ClipCapacity > 1 ||
						    !Vec3Equals(itemInfo.MuzzlePosition, FirearmBehavior.DEFAULT_MUZZLE_POSITION) ||
						    gunshotFlash != null ||
						    gunshotSound != null)
						{
							hasContent = true;
							if (!string.IsNullOrWhiteSpace(gunshotSound))
							{
								itemInfo.GunshotSoundCode = SoundEvent.GetEventIdFromString(gunshotSound);
							}
							else
							{
								itemInfo.GunshotSoundCode = -1;
							}

							if (!string.IsNullOrWhiteSpace(gunshotFlash))
							{
								itemInfo.GunshotFlash = gunshotFlash;
							}
							else
							{
								itemInfo.GunshotFlash = null;
							}
						}
					}
				}

				if (hasContent)
					ExtendedItemInfo.Set(__instance, itemInfo);
			}
		}

		private static bool Vec3Equals(Vec3 a, Vec3 b)
		{
			return MBMath.ApproximatelyEquals(a.x, b.x) &&
			       MBMath.ApproximatelyEquals(a.y, b.y) &&
			       MBMath.ApproximatelyEquals(a.z, b.z);
		}
	}
}