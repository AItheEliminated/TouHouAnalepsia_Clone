using HarmonyLib;
using SandBox;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.ModuleManager;
using static TouhouAnalepsia.HarmonyHandler;

namespace TouhouAnalepsia.Patches {
	class MapPatches {
		public static void Patch() {

		}
		[HarmonyPatch(typeof(DefaultMapDistanceModel), "GetDistance", new Type[] { typeof(MobileParty), typeof(MobileParty) })]
		class DebugPatch29 {
			public static bool Prefix(MobileParty fromParty, MobileParty toParty) {
				if (fromParty.CurrentNavigationFace.FaceIndex < 0) {
					Logger.Log("fromParty: " + fromParty.Name.ToString() + " is on an invalid navmesh face.\nThe Current location is at " + fromParty.GetPosition());
				}
				if (toParty.CurrentNavigationFace.FaceIndex < 0) {
					Logger.Log("toParty: " + toParty.Name.ToString() + " is on an invalid navmesh face.\nThe Current location is at " + toParty.GetPosition());
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Campaign), "MapWidth")]
		public static bool MapWidthPatch(ref float __result) {
			__result = 2700f;
			return false;
		}
		[HarmonyPatch(typeof(Campaign), "MapHeight")]
		public static bool MapHeightPatch(ref float __result) {
			__result = 2700f;
			return false;
		}
		[HarmonyPatch(typeof(Campaign), "MapDiagonal")]
		public static bool MapDiagonalPatch(ref float __result) {
			__result = (float)Math.Sqrt(14580000.0);
			return false;
		}

		//Debug Harmony patches
		[HarmonyPatch(typeof(SettlementPositionScript), "get_SettlementsDistanceCacheFilePath")]
		class SettlementsDistanceCacheFilePathPatch {
			static bool Prefix(ref string __result) {
				__result = ModuleHelper.GetModuleFullPath("TouhouAnalepsia") + "ModuleData/settlements_distance_cache.bin";
				return false;
			}
		}
		[HarmonyPatch(typeof(SettlementPositionScript), "get_SettlementsXmlPath")]
		class SettlementsXmlPathPatch {
			static bool Prefix(ref string __result) {
				__result = ModuleHelper.GetModuleFullPath("TouhouAnalepsia") + "ModuleData/settlements.xml";
				return false;
			}
		}

		[HarmonyPatch(typeof(ConversationManager), "StartNew")]
		class DebugPatch30 {
			public static bool Prefix() {
				Logger.Log("~~ At ConversationManager.StartNew ~~");

				MobileParty playerParty = MobileParty.MainParty;

				Logger.Log("The current navmesh ID is " + playerParty.CurrentNavigationFace.FaceIndex + ".");
				Logger.Log("It is located at " + Campaign.Current.MapSceneWrapper.GetNavigationMeshCenterPosition(playerParty.CurrentNavigationFace));
				Logger.Log("Who has the terrain type " + Campaign.Current.MapSceneWrapper.GetFaceTerrainType(playerParty.CurrentNavigationFace) + ".");

				return true;
			}
		}

		/*
		[HarmonyPatch(typeof(DefaultMapDistanceModel), "GetDistance", new Type[] { typeof(MobileParty), typeof(Settlement) })]
		[HarmonyPatch(typeof(DefaultMapDistanceModel), "GetDistance", new Type[] { typeof(MobileParty), typeof(MobileParty) })]
		class DebugPatch27 {
			static Exception Finalizer(MobileParty fromParty, Exception __exception) {
				if (__exception?.Message == null) return __exception;
				Logger.Log("Begin Exception Log for DefaultMapDistanceModel.GetDistance");
				if (__exception?.Message != null) {
					Logger.Log(__exception.Message);
					Logger.Log(__exception.StackTrace);
				} else Logger.Log("No exception message");
				Logger.Log(fromParty.StringId);
				foreach (TroopRosterElement element in fromParty.MemberRoster.GetTroopRoster()) {
					if (element.Character == null) {
						Logger.Log("A character object is null!");
					} else {
						if (element.Character.Culture != null) Logger.Log(element.ToString() + ": This character's culture is " + element.Character.Culture.StringId);
						//else Logger.Log(element.ToString() + ": This character's culture is null.");
						else {
							Logger.Log(element.Character?.StringId + ": This character's culture is null.");
						}
					}
				}
				Logger.Log("End Exception Log for DefaultMapDistanceModel.GetDistance");

				return __exception;
			}
		}
		[HarmonyPatch(typeof(SiegeEvent))]
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPatch(new Type[] { typeof(Settlement), typeof(MobileParty) })]
		class DebugPatch28 {
			static bool Prefix(Settlement settlement, MobileParty besiegerParty) {
				Logger.Log("Begin Log for SiegeEvent");

				Logger.Log(settlement?.StringId);
				Logger.Log(besiegerParty?.StringId);

				Logger.Log("End Log for SiegeEvent");
				return true;
			}
		}*/
	}
}
