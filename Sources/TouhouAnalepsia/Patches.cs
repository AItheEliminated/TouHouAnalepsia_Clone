using HarmonyLib;
using SandBox;
using StoryMode;
using StoryMode.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace TouhouAnalepsia {
	class Patches {
		public static void Prepatch() {

		}
		public static void DefaultPatches() {
			//HarmonyHandler.patchPrefix("SandBox.View.Map.PartyVisual", "TaleWorlds.CampaignSystem.IPartyVisual.OnStartup", "TouhouAnalepsia.Patches", "DebugPatchPrefix");

			//StoryMode patches
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnNewGameCreated", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnMainStoryLineSideChosen", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnStoryModeTutorialEnded", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "InitializeTutorial", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "OnStoryModeTutorialEnded", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "OnGameLoaded", "TouhouAnalepsia.Patches", "RemoveMethod");

			//Various deletions
			HarmonyHandler.patchPrefix("TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BackstoryCampaignBehavior", "OnNewGameCreated", "TouhouAnalepsia.Patches", "RemoveMethod"); //Makes reference to certain characters who now have their information removed from the files.
																																														//HarmonyHandler.patchPrefix("TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.WorkshopsCampaignBehavior", "BuildWorkshopsAtGameStart", "TouhouAnalepsia.Patches", "RemoveMethod");
			HarmonyHandler.patchPrefix("TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames.TournamentCampaignBehavior", "InitializeTournamentLeaderboard", "TouhouAnalepsia.Patches", "RemoveMethod");

			HarmonyHandler.patchFinalizer("ManagedCallbacks.ScriptingInterfaceOfIScene", "GetNavMeshFaceCenterPosition", "TouhouAnalepsia.Patches", "Finalizer");
			HarmonyHandler.patchPrefix("ManagedCallbacks.ScriptingInterfaceOfIScene", "GetNavMeshFaceCenterPosition", "TouhouAnalepsia.Patches", "Prefix");

		}
		static Exception Finalizer(UIntPtr scenePointer, int navMeshFace, ref Vec3 centerPos, Exception __exception) {
			//if (__exception?.Message == null) return __exception;
			Logger.Log("Begin Exception Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");
			if (__exception?.Message != null) {
				Logger.Log(__exception.Message);
				Logger.Log(__exception.StackTrace);
			} else Logger.Log("No exception message");

			Logger.Log(Convert.ToString(navMeshFace));
			Logger.Log(Convert.ToString(centerPos));

			Logger.Log("End Exception Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");

			return __exception;
		}
		static bool Prefix(UIntPtr scenePointer, int navMeshFace, ref Vec3 centerPos) {
			if (navMeshFace>=0) return true;
			Logger.Log("Begin Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");

			Logger.Log(Convert.ToString(navMeshFace));
			Logger.Log(Convert.ToString(centerPos));

			Logger.Log("End Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");

			return true;
		}
		public static bool DebugPatchPrefix(ref PartyBase party) {
			//Logger.Log(party.Name.GetID() + ": " + party.Name.ToString());

			return true;
		}
		public static bool RemoveMethod() {
			return false;
		}

		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Campaign), "MapWidth")]
		public static bool MapWidthPatch(ref float __result) {
			__result = 2700f;
			return false;
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Campaign), "MapHeight")]
		public static bool MapHeightPatch(ref float __result) {
			__result = 2700f;
			return false;
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Campaign), "MapDiagonal")]
		public static bool MapDiagonalPatch(ref float __result) {
			__result = (float)Math.Sqrt(14580000.0);
			return false;
		}

		/// <summary>
		/// Music Patch
		/// </summary>
		/*
		[HarmonyPatch(typeof(psai.net.PsaiCore), "LoadSoundtrackFromProjectFile")]
		class MusicPatch {
			static bool Prefix(psai.net.PsaiCore __instance, ref string pathToProjectFile) {
				if (pathToProjectFile.Equals(BasePath.Name + "music/soundtrack.xml")) pathToProjectFile = BasePath.Name + "Modules/TouhouAnalepsia/music/soundtrack.xml";
				return true;
			}
		}*/

		//Debug Harmony patches
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.UrbanCharactersCampaignBehavior), "CreateCompanion")]
		class DebugPatch2 {
			static bool Prefix(ref CharacterObject companionTemplate) {
				if (companionTemplate == null) {
					return true;
				}


				if (companionTemplate.Culture == null || !companionTemplate.Culture.StringId.ToString().Equals("human")) {
					Settlement chosen = Settlement.All.GetRandomElementWithPredicate((Settlement settlement) => settlement.Culture.StringId.ToString().Equals("human") && settlement.IsTown);
					companionTemplate.Culture = chosen.Culture;
				}

				string message = companionTemplate.StringId + ": " + companionTemplate.Culture.StringId.ToString();

				//Console.WriteLine(message);
				//Logger.Log(message);

				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.UrbanCharactersCampaignBehavior), "SpawnNotablesIfNeeded")]
		class DebugPatch5 {
			static bool Prefix(ref Settlement settlement) {
				if (settlement.IsTown || settlement.IsVillage) {
					float randomFloat = MBRandom.RandomFloat;
					int targetNotableCountForSettlement = settlement.IsTown ? 6 : (settlement.IsVillage ? 2 : 0);
					float num = settlement.Notables.Any<Hero>() ? ((float)(targetNotableCountForSettlement - settlement.Notables.Count) / (float)targetNotableCountForSettlement) : 1f;
					if (randomFloat <= num) {
						List<Occupation> list = new List<Occupation>();
						if (settlement.IsTown) {
							list = new List<Occupation>
							{
							Occupation.GangLeader,
							Occupation.Artisan,
							Occupation.Merchant
						};
						} else if (settlement.IsVillage) {
							list = new List<Occupation>
							{
							Occupation.RuralNotable,
							Occupation.Headman
						};
						}
						foreach (Hero hero in settlement.Notables) {
							if (list.Contains(hero.CharacterObject.Occupation)) {
								list.Remove(hero.CharacterObject.Occupation);
							}
						}
						if (list.Any<Occupation>()) {
							Occupation rOccupation = list.GetRandomElement<Occupation>();
							Hero tmpHero = HeroCreator.CreateHeroAtOccupation(rOccupation, settlement);
							if (tmpHero == null) {
								Logger.Log("tmpHero is null! Was expecting a hero with the occupation " + rOccupation + " in the settlement with id " + settlement.StringId + ".");
							} else {
								TaleWorlds.CampaignSystem.Actions.EnterSettlementAction.ApplyForCharacterOnly(tmpHero, settlement);
							}
						}
					}
				}
				return false;
			}
		}
		[HarmonyPatch(typeof(StoryMode.StoryModeSubModule), "AddBehaviors")]
		class DebugPatch19 {
			static bool Prefix(CampaignGameStarter campaignGameStarter) {
				//campaignGameStarter.AddBehavior(new LordConversationsStoryModeBehavior());
				//campaignGameStarter.AddBehavior(new TrainingFieldCampaignBehavior());
				campaignGameStarter.AddBehavior(new TutorialPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new FirstPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new SecondPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new ThirdPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new StoryModeTutorialBoxCampaignBehavior());
				//campaignGameStarter.AddBehavior(new TravelToVillageTutorialQuestBehavior());
				/*
				campaignGameStarter.AddBehavior(new TalkToTheHeadmanTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new PurchaseGrainTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new RecruitTroopsTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new LocateAndRescueTravellerTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new FindHideoutTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new BannerInvestigationQuestBehavior());
				campaignGameStarter.AddBehavior(new MeetWithIstianaQuestBehavior());
				campaignGameStarter.AddBehavior(new MeetWithArzagosQuestBehavior());
				campaignGameStarter.AddBehavior(new IstianasBannerPieceQuestBehavior());
				campaignGameStarter.AddBehavior(new ArzagosBannerPieceQuestBehavior());
				campaignGameStarter.AddBehavior(new WeakenEmpireQuestBehavior());
				campaignGameStarter.AddBehavior(new AssembleEmpireQuestBehavior());
				campaignGameStarter.AddBehavior(new SupportKingdomQuestBehavior());
				campaignGameStarter.AddBehavior(new CreateKingdomQuestBehavior());
				campaignGameStarter.AddBehavior(new ConspiracyProgressQuestBehavior());
				campaignGameStarter.AddBehavior(new DefeatTheConspiracyQuestBehavior());
				campaignGameStarter.AddBehavior(new RebuildPlayerClanQuestBehavior());
				*/
				//campaignGameStarter.AddBehavior(new RescueFamilyQuestBehavior());
				return false;
			}
		}
		[HarmonyPatch(typeof(MainStoryLine), "get_IsPlayerInteractionRestricted")]
		class DebugPatch20 {
			static bool Prefix(ref bool __result) {
				__result = false;
				return false;
			}
		}
		[HarmonyPatch(typeof(SettlementPositionScript), "get_SettlementsDistanceCacheFilePath")]
		class DebugPatch23 {
			static bool Prefix(ref string __result) {
				__result = ModuleHelper.GetModuleFullPath("TouhouAnalepsia") + "ModuleData/settlements_distance_cache.bin";
				return false;
			}
		}
		[HarmonyPatch(typeof(SettlementPositionScript), "get_SettlementsXmlPath")]
		class DebugPatch24 {
			static bool Prefix(ref string __result) {
				__result = ModuleHelper.GetModuleFullPath("TouhouAnalepsia") + "ModuleData/settlements.xml";
				return false;
			}
		}
		[HarmonyPatch(typeof(MobileParty), "DailyTick")]
		class DebugPatch25 {
			static Exception Finalizer(MobileParty __instance, Exception __exception) {
				if (__exception?.Message == null) return __exception;
				Logger.Log("Begin Exception Log for MobileParty.DailyTick");
				if (__exception?.Message != null) {
					Logger.Log(__exception.Message);
					Logger.Log(__exception.StackTrace);
				} else Logger.Log("No exception message");
				Logger.Log(__instance.StringId);
				foreach (TroopRosterElement element in __instance.MemberRoster.GetTroopRoster()) {
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
				Logger.Log("End Exception Log for MobileParty.DailyTick");

				return __exception;
			}
		}
		[HarmonyPatch(typeof(DefaultPartyWageModel), "GetTotalWage")]
		class DebugPatch26 {
			static Exception Finalizer(MobileParty mobileParty, Exception __exception) {
				if (__exception?.Message == null) return __exception;
				Logger.Log("Begin Exception Log for DefaultPartyWageModel.GetTotalWage");
				if (__exception?.Message != null) {
					Logger.Log(__exception.Message);
					Logger.Log(__exception.StackTrace);
				} else Logger.Log("No exception message");
				Logger.Log(mobileParty.StringId);
				foreach (TroopRosterElement element in mobileParty.MemberRoster.GetTroopRoster()) {
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
				Logger.Log("End Exception Log for DefaultPartyWageModel.GetTotalWage");

				return __exception;
			}
		}
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
		}
	}
}
