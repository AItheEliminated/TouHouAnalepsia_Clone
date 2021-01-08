using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Engine;
using System.Reflection;
using Module = TaleWorlds.MountAndBlade.Module;
using TouhouAnalepsia.Editor.FastScene;
using TouhouAnalepsia.Editor.Playground;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase {

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			HarmonyHandler.PatchAll();

			HarmonyHandler.patchPrefix("SandBox.View.Map.PartyVisual", "TaleWorlds.CampaignSystem.IPartyVisual.OnStartup", "TouhouAnalepsia.SubModule", "DebugPatchPrefix");
			//HarmonyHandler.patchPrefix("StoryMode.Behaviors.Quests.FirstPhase.BannerInvestigationQuestBehavior.BannerInvestigationQuest", "InitializeNotablesToTalkList", "TouhouAnalepsia.SubModule", "RemoveMethod");

			CampaignOptions.EnableCampaignTutorials = false;

			if (Utilities.EditModeEnabled)
			{
				Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
					"Playground", new TextObject("Playground"), 1,
					() => { MBGameManager.StartNewGame(new FastSceneManager(
						() =>
						{
							PlaygroundMission.OpenPlaygroundMission("scn_playground", "");
						}
						)); }, false));
			}
		}
		public static bool DebugPatchPrefix(ref PartyBase party) {
			//Logger.Log(party.Name.GetID() + ": " + party.Name.ToString());

			return true;
		}
		public static bool RemoveMethod() {
			return false;
		}

		//Some actually important Harmony Patches
		[HarmonyPatch(typeof(psai.net.PsaiCore), "LoadSoundtrackFromProjectFile")]
		class MusicPatch {
			static bool Prefix(psai.net.PsaiCore __instance, ref string pathToProjectFile) {
				//if (pathToProjectFile.Equals(BasePath.Name + "music/soundtrack.xml")) pathToProjectFile = BasePath.Name + "Modules/TouhouAnalepsia/music/soundtrack.xml";
				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BackstoryCampaignBehavior), "OnNewGameCreated")] //Makes reference to certain characters who now have their information removed from the files.
		class BackstoryCampaignBehaviorPatch {
			static bool Prefix(ref CampaignGameStarter campaignGameStarter) {
				return false;
			}
		}
		//StoryMode Patches
		[HarmonyPatch(typeof(StoryMode.Behaviors.FirstPhaseCampaignBehavior), "OnNewGameCreated")]
		class FirstPhaseCampaignBehaviorPatch1 {
			static bool Prefix(ref CampaignGameStarter campaignGameStarter) {//, ref Location ____inperialMentorHouse, ref Location ____antiImperialMentorHouse
				return false;
			}
		}
		[HarmonyPatch(typeof(StoryMode.Behaviors.FirstPhaseCampaignBehavior), "OnMainStoryLineSideChosen")]
		class FirstPhaseCampaignBehaviorPatch2 {
			static bool Prefix(ref StoryMode.MainStoryLineSide side) {
				return false;
			}
		}
		[HarmonyPatch(typeof(StoryMode.Behaviors.TutorialPhaseCampaignBehavior), "InitializeTutorial")]
		class FirstPhaseCampaignBehaviorPatch3 {
			static bool Prefix() {
				return false;
			}
		}

		//Debug Harmony patches
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.GameComponents.Map.DefaultMapDistanceModel), "GetDistance", new Type[] { typeof(Settlement), typeof(Settlement) })]
		class DebugPatch1 {
			static bool Prefix(ref Settlement fromSettlement, ref Settlement toSettlement) {
				string message = (fromSettlement==null).ToString()+", "+ (toSettlement == null).ToString();

				bool toLogOrNotToLog = false;

				if (fromSettlement!=null) {
					message += ", from: "+fromSettlement.StringId;
				} else toLogOrNotToLog = true;

				if (toSettlement != null) {
					message += ", to: " + toSettlement.StringId;
				} else toLogOrNotToLog = true;

				if (toLogOrNotToLog) {
					Logger.Log(message);
				}

				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.UrbanCharactersCampaignBehavior), "CreateCompanion")]
		class DebugPatch2 {
			static bool Prefix(ref CharacterObject companionTemplate) {
				if (companionTemplate == null) {
					return true;
				}


				while (companionTemplate.Culture == null || !companionTemplate.Culture.StringId.ToString().Equals("human")) {
					Settlement chosen = (from settlement in Settlement.All
											  where settlement.Culture != null && settlement.IsTown
											  select settlement).GetRandomElement<Settlement>();
					companionTemplate.Culture = chosen.Culture;
				}

				string message = companionTemplate.StringId + ": " + companionTemplate.Culture.StringId.ToString();

				//Console.WriteLine(message);
				//Logger.Log(message);

				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.GameComponents.Map.DefaultDiplomacyModel), "GetHeroesForEffectiveRelation")]
		class DebugPatch3 {
			static bool Prefix(Hero hero1, Hero hero2) {
				string message = (hero1==null).ToString()+", "+(hero2==null).ToString();

				//Logger.Log(message);

				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.WorkshopsCampaignBehavior), "BuildWorkshopsAtGameStart")]
		class DebugPatch4 {
			static bool Prefix() {
				return false;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.UrbanCharactersCampaignBehavior), "SpawnNotablesIfNeeded")]
		class DebugPatch5 {
			static bool Prefix(ref Settlement settlement) {
				if (settlement.IsTown || settlement.IsVillage) {
					float randomFloat = MBRandom.RandomFloat;
					int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement);
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
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Actions.EnterSettlementAction), "ApplyInternal")]
		class DebugPatch6 {
			static bool Prefix(ref Hero hero, ref MobileParty mobileParty, ref Settlement settlement) {
				string message = settlement.StringId;
				if (hero != null) message += ": " + hero.StringId;
				else message += ": null";
				//Logger.Log(message);


				return true;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames.TournamentCampaignBehavior), "InitializeTournamentLeaderboard")]
		class DebugPatch7 {
			static bool Prefix() { 
				return false;
			}
		}
		[HarmonyPatch(typeof(TroopRoster), "AddToCounts")]
		class DebugPatch8 {
			static bool Prefix(ref CharacterObject character) {
				string message = "";
				if (character != null) message += character.StringId;
				else message += "null";

				//Logger.Log(message);


				return true;
			}
		}
		[HarmonyPatch(typeof(StoryMode.Behaviors.FirstPhaseCampaignBehavior), "OnStoryModeTutorialEnded")] 
		class DebugPatch9 {
			static bool Prefix() {
				return false;
			}
		}
		[HarmonyPatch(typeof(StoryMode.Behaviors.TutorialPhaseCampaignBehavior), "OnStoryModeTutorialEnded")]
		class DebugPatch10 {
			static bool Prefix() {
				PartyBase.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 2, true);
				Hero.MainHero.Heal(PartyBase.MainParty, Hero.MainHero.MaxHitPoints, false);
				//GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, Hero.MainHero.Gold, true);
				//GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 1000, true);
				return false;
			}
		}
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			// skip the shit intro video while developing. I almost get smithing-phobia for it.
			if (Utilities.EditModeEnabled)
			{
				var field = typeof(Module).GetField("_splashScreenPlayed", BindingFlags.Instance | BindingFlags.NonPublic);
				field?.SetValue(Module.CurrentModule, true);
			}
		}

		public override void OnMissionBehaviourInitialize(Mission mission) {
			mission.AddMissionBehaviour(new AbilityBattleLogic());

			base.OnMissionBehaviourInitialize(mission);
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
			if (game.GameType is Campaign) {
				FactionCulture.initializeFactionCultures();

				TouhouModelManager.initializeTouhouModels(gameStarterObject);

				loadLocalizationData((CampaignGameStarter)gameStarterObject);
				//game.ObjectManager.RegisterType<Race>("Race", "Races", 50U, true);
			}

			base.OnGameStart(game, gameStarterObject);
		}
		public override void OnCampaignStart(Game game, object starterObject) {
			if (game.GameType is Campaign) {
				CampaignOptions.EnableCampaignTutorials = false;

				CampaignGameStarter campaignGameStarter = (CampaignGameStarter)starterObject;

				campaignGameStarter.AddBehavior(new CompanionHandler.CompanionCampaignBehavior());
			}

			base.OnCampaignStart(game, starterObject);
		}

		private void loadLocalizationData(CampaignGameStarter gameStarter) {
			gameStarter.LoadGameTexts(BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/module_strings.xml");
		}
	}
}
