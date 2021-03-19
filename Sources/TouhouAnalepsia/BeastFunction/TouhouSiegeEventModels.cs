using HarmonyLib;
using SandBox;
using SandBox.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.ObjectSystem;

namespace TouhouAnalepsia.BeastFunction {
	class TouhouSiegeEventModel : DefaultSiegeEventModel {
		//Harmony Patches
		[HarmonyPatch(typeof(CustomBattleMenuVM), "GetSiegeWeaponType")]
		class CustomBattleDataPatch1 {
			static void Postfix(SiegeEngineType siegeWeaponType, ref SiegeEngineType __result) {
				if (siegeWeaponType == TouhouSiegeEngineTypes.BeastUnit) __result = TouhouSiegeEngineTypes.BeastUnit;
			}
		}
		[HarmonyPatch(typeof(CampaignMissionManager), "GetSiegeWeaponType")]
		class CampaignMissionManagerPatch1 {
			static void Postfix(SiegeEngineType siegeWeaponType, ref Type __result) {
				if (siegeWeaponType == TouhouSiegeEngineTypes.BeastUnit) __result = typeof(BeastUnit);
			}
		}
		[HarmonyPatch(typeof(CampaignMapSiegePrefabEntityCache), "GetSiegeEngineEntity")]
		class CampaignMapSiegePrefabEntityCachePatch1 {
			static bool Prefix(ref SiegeEngineType type) {
				if (type == TouhouSiegeEngineTypes.BeastUnit) {
					type = DefaultSiegeEngineTypes.FireBallista;
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(OrderSiegeMachineVM), "GetSiegeType")]
		class OrderSiegeMachineVMPatch1 {
			static void Postfix(Type t, ref SiegeEngineType __result) {
				if (t == typeof(BeastUnit)) __result = TouhouSiegeEngineTypes.BeastUnit;
			}
		}
		[HarmonyPatch(typeof(BannerlordMissions), "GetSiegeWeaponType")]
		class BannerlordMissionsPatch1 {
			static void Postfix(SiegeEngineType siegeWeaponType, ref Type __result) {
				if (siegeWeaponType == TouhouSiegeEngineTypes.BeastUnit) __result = typeof(BeastUnit);
			}
		}

		[HarmonyPatch(typeof(OrderSiegeMachineItemWidget), "UpdateMachineIcon")]
		class OrderSiegeMachineItemWidgetPatch1 {
			static bool Prefix(OrderSiegeMachineItemWidget __instance, ref bool ____isRemainingCountVisible) {
				if (__instance.MachineIconWidget == null) return true;
				____isRemainingCountVisible = true;
				string machineClass = __instance.MachineClass;
				if (machineClass == "beast_unit") {
					__instance.MachineIconWidget.SetState("FireBallista");
					return false;
				}
				return true;
			}
		}

		//Actual class

		public override string GetSiegeEngineMapPrefabName(SiegeEngineType type, int wallLevel, BattleSideEnum side) {
			if (type == TouhouSiegeEngineTypes.BeastUnit) return ((side == BattleSideEnum.Attacker) ? "ballista_a_fire_mapicon" : "ballista_b_fire_mapicon");
			return base.GetSiegeEngineMapPrefabName(type, wallLevel, side);
		}
		public override string GetSiegeEngineMapProjectilePrefabName(SiegeEngineType type) {
			if (type == TouhouSiegeEngineTypes.BeastUnit) return "ballista_fire_mapicon_projectile";
			return base.GetSiegeEngineMapProjectilePrefabName(type);
		}
		public override sbyte GetSiegeEngineMapProjectileBoneIndex(SiegeEngineType type, BattleSideEnum side) {
			if (type == TouhouSiegeEngineTypes.BeastUnit) return 7;
			return base.GetSiegeEngineMapProjectileBoneIndex(type, side);
		}
		public override IEnumerable<SiegeEngineType> GetAvailableAttackerRangedSiegeEngines(PartyBase party) {
			List<SiegeEngineType> types = new List<SiegeEngineType>();
			types.Add(TouhouSiegeEngineTypes.BeastUnit);
			return base.GetAvailableAttackerRangedSiegeEngines(party).Concat(types);
		}
	}
	class TouhouSiegeEngineTypes {
		//Harmony Patches
		[HarmonyPatch(typeof(CustomBattleData), "GetAllAttackerRangedMachines")]
		class CustomBattleDataPatch1 {
			static IEnumerable<SiegeEngineType> Postfix(IEnumerable<SiegeEngineType> values) {
				foreach(SiegeEngineType engineType in values) {
					yield return engineType;
				}
				yield return TouhouSiegeEngineTypes.BeastUnit;
			}
		}

		//Actual class
		private static readonly TouhouSiegeEngineTypes Instance = new TouhouSiegeEngineTypes();
		public static SiegeEngineType BeastUnit {
			get {
				return TouhouSiegeEngineTypes.Instance.BeastUnitEngine;
			}
		}

		public TouhouSiegeEngineTypes() {
			RegisterAll();
		}

		private void RegisterAll() {
			MBObjectManager.Instance.LoadXML("SiegeEngines", null, false);
			BeastUnitEngine = GetSiegeEngine("beast_unit");
		}
		private SiegeEngineType GetSiegeEngine(string id) {
			return MBObjectManager.Instance.GetObject<SiegeEngineType>(id);
		}

		internal SiegeEngineType BeastUnitEngine;
	}
}
