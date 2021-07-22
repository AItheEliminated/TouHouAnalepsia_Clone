using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.BattleSystem.Firearms
{
	public static class FirearmPatch
	{
		private static MethodInfo methodIsFirearm = AccessTools.Method(
			typeof(FirearmPatch), nameof(IsFirearm));

		private static MethodInfo methodGetMissileHitGroundParticle = AccessTools.Method(
			typeof(FirearmPatch), nameof(GetMissileHitGroundParticle));

		private static MethodInfo methodGetWeaponClass = AccessTools.PropertyGetter(
			typeof(WeaponComponentData), nameof(WeaponComponentData.WeaponClass));

		public static bool IsFirearm(this WeaponClass weaponClass)
		{
			switch (weaponClass)
			{
				case WeaponClass.Pistol:
				case WeaponClass.Musket:
					return true;
				default:
					return false;
			}
		}

		public static int GetMissileHitGroundParticle(Mission.Missile missile)
		{
			switch (missile.Weapon.Item.PrimaryWeapon.WeaponClass)
			{
				case WeaponClass.Cartridge:
					return ParticleSystemManager.GetRuntimeIdByName("ps_bullet_hit");
				default:
					return ParticleSystemManager.GetRuntimeIdByName("psys_game_broken_arrow");
			}
		}

		[HarmonyPatch(typeof(TaleWorlds.Core.ItemObject))]
		[HarmonyPatch("GetAmmoTypeForItemType")]
		static class GetAmmoTypeForItemTypePatch
		{
			static bool Prefix(ItemObject.ItemTypeEnum __0, ref ItemObject.ItemTypeEnum __result)
			{
				if (__0 == ItemObject.ItemTypeEnum.Musket)
				{
					__result = ItemObject.ItemTypeEnum.Bullets;
					return false; // skip original method
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.MissionEquipment))]
		[HarmonyPatch("CheckLoadedAmmos")]
		static class CheckLoadedAmmosPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Bne_Un_S)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Call, methodIsFirearm);
						yield return new CodeInstruction(OpCodes.Ldc_I4_1);

					}
					yield return code;
				}
			}
		}

		[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.Mission))]
		[HarmonyPatch("MissileHitCallback")]
		static class MissileHitCallbackPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Call &&
					    (code.operand as MethodInfo)?.Name == "GetRuntimeIdByName")
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Call, methodGetMissileHitGroundParticle);

					}
					else
						yield return code;
				}
			}
		}

		[HarmonyPatch(typeof(TaleWorlds.Core.WeaponComponentData))]
		[HarmonyPatch("SetDamageFactors")]
		static class SetDamageFactorsPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var isFinished = false;
				foreach (var code in instructions)
				{
					yield return code;
					if (!isFinished &&
					    code.opcode == OpCodes.Beq_S &&
					    code.operand is Label label)
					{
						isFinished = true;
						WeaponClass[] newClasses = new[]
						{
							WeaponClass.Cartridge,
							WeaponClass.Pistol,
							WeaponClass.Musket
						};
						foreach (var weaponClass in newClasses)
						{
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Call, methodGetWeaponClass);
							yield return new CodeInstruction(OpCodes.Ldc_I4_S, (int) weaponClass);
							yield return new CodeInstruction(OpCodes.Beq_S, label);
						}
					}
				}
			}
		}
	}
}