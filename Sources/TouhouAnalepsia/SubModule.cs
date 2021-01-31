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
using StoryMode.Behaviors;
using SandBox;
using System.Xml;

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
				if (pathToProjectFile.Equals(BasePath.Name + "music/soundtrack.xml")) pathToProjectFile = BasePath.Name + "Modules/TouhouAnalepsia/music/soundtrack.xml";
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
				//PartyBase.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 2, true);
				Hero.MainHero.Heal(PartyBase.MainParty, Hero.MainHero.MaxHitPoints, false);
				//GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, Hero.MainHero.Gold, true);
				//GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 1000, true);
				return false;
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.Engine.GauntletUI.GauntletLayer), "LoadMovie")]
		class DebugPatch11 {
			static bool Prefix(String movieName, ViewModel dataSource) {
				//if (movieName != null) Logger.Log("LoadMovie: " + movieName);
				//else Logger.Log("LoadMovie: null");
				return true;
			}
		}
		/*
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.GameMenuItemVM), "ExecuteAction")]
		class DebugPatch12 {
			static bool Prefix(TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.GameMenuItemVM __instance, MenuContext ____menuContext) {
				Logger.Log("Made it to ExecuteAction! The GameMenuItemVM is " + __instance);
				if (__instance != null) Logger.Log("The GameMenuItemVM has a menuContext of " + ____menuContext);
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past ExecuteAction!");
			}
		}
		[HarmonyPatch(typeof(MenuContext), "InvokeConsequence")]
		class DebugPatch13 {
			static bool Prefix(MenuContext __instance, int index) {
				Logger.Log("Made it to InvokeConsequence! The MenuContext is " + __instance + ", and the Campaign's MenuContext is " + Campaign.Current.CurrentMenuContext);
				Logger.Log("The GameMenuItemVM has a index of " + index);
				Logger.Log("The Campaign's GameMenuManager is " + Campaign.Current.GameMenuManager);
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past InvokeConsequence!");
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.GameMenus.GameMenuManager), "RunConsequencesOfMenuOption")]
		class DebugPatch14 {
			static bool Prefix(MenuContext menuContext, int menuItemNumber) {
				Logger.Log("Made it to RunConsequencesOfMenuOption!");
				Logger.Log("The MenuContext is " + menuContext + ", and the item number is " + menuItemNumber);
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past RunConsequencesOfMenuOption!");
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.GameMenus.GameMenu), "RunMenuOptionConsequence")]
		class DebugPatch15 {
			static bool Prefix(MenuContext menuContext, int menuItemNumber, List<TaleWorlds.CampaignSystem.GameMenus.GameMenuOption> ____menuItems) {
				Logger.Log("Made it to RunMenuOptionConsequence!");
				Logger.Log("The MenuContext is " + menuContext + ", and the item number is " + menuItemNumber + " of " + ____menuItems.Count);
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past RunConsequencesOfMenuOption!");
			}
		}
		[HarmonyPatch(typeof(TaleWorlds.CampaignSystem.GameMenus.GameMenuOption), "RunConsequence")]
		class DebugPatch16 {
			static bool Prefix(TaleWorlds.CampaignSystem.GameMenus.GameMenuOption __instance, MenuContext menuContext) {
				Logger.Log("Made it to RunConsequence! The GameMenuOption is " + __instance);
				Logger.Log("The MenuContext is " + menuContext);
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past RunConsequence!");
			}
		}
		[HarmonyPatch(typeof(MenuContext), "OnConsequence")]
		class DebugPatch17 {
			static bool Prefix() {
				Logger.Log("Made it to OnConsequence!");
				return true;
			}
			static void Postfix() {
				Logger.Log("Made it past OnConsequence!");
			}
		}
		[HarmonyPatch(typeof(TrainingFieldCampaignBehavior), "game_menu_settlement_leave_on_consequence")]
		class DebugPatch18 {
			static bool Prefix() {
				Logger.Log("Made it to game_menu_settlement_leave_on_consequence!");
				PlayerEncounter.LeaveSettlement();
				Logger.Log("LeaveSettlement Suceded!");
				PlayerEncounter.Finish(true);
				Logger.Log("Made it past game_menu_settlement_leave_on_consequence!");
				return false;
			}
		}
		*/
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
		[HarmonyPatch(typeof(StoryMode.MainStoryLine), "get_IsPlayerInteractionRestricted")]
		class DebugPatch20 {
			static bool Prefix(ref bool __result) {
				__result = false;
				return false;
			}
		}
		/*
		[HarmonyPatch(typeof(SandBox.MapScene), "Load")]
		class DebugPatch21 {
			static bool Prefix(ref Scene ___scene) {
				Debug.Print("Creating map scene", 0, Debug.DebugColor.White, 17592186044416UL);
				___scene = Scene.CreateNewScene(false);
				___scene.SetName("MapScene");
				___scene.SetClothSimulationState(true);
				this._agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(this._scene, 4096);
				___scene.SetOcclusionMode(true);
				SceneInitializationData initData = new SceneInitializationData(true);
				initData.UsePhysicsMaterials = false;
				initData.EnableFloraPhysics = false;
				initData.UseTerrainMeshBlending = false;
				Debug.Print("reading map scene", 0, Debug.DebugColor.White, 17592186044416UL);
				___scene.Read("Main_map", initData);
				MBDebug.SetErrorReportScene(this._scene);
				___scene.DisableStaticShadows(true);
				___scene.InvalidateTerrainPhysicsMaterials();
				this.LoadAtmosphereData(___scene);
				this.DisableUnwalkableNavigationMeshes();
				MBMapScene.ValidateTerrainSoundIds();
				___scene.OptimizeScene(true, false);
				Debug.Print("Ticking map scene for first initialization", 0, Debug.DebugColor.White, 17592186044416UL);
				this._scene.Tick(0.1f);
				return false;
			}
		}
		*/
		[HarmonyPatch(typeof(Scene), "Read", new Type[] { typeof(string), typeof(SceneInitializationData) })]
		class DebugPatch22 {
			static bool Prefix(ref string sceneName) {
				//if (sceneName.Equals("Main_map")) sceneName = "test_map";
				return true;
			}
		}
		[HarmonyPatch(typeof(SettlementPositionScript), "SaveSettlementDistanceCache")] //TODO - implement this
		class DebugPatch23 {
			static bool Prefix() {
				return true;
			}
		}
		[HarmonyPatch(typeof(SettlementPositionScript), "LoadXmlFile")]
		class DebugPatch24 {
			static bool Prefix(ref string path) {
				if (path.Equals(BasePath.Name + "Modules/SandBox/ModuleData/settlements.xml")) path = BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/settlements.xml";
				return true;
			}
		}
		[HarmonyPatch(typeof(XmlDocument), "Save", new Type[] { typeof(string) } )]
		class DebugPatch25 {
			static bool Prefix(ref string filename) {
				if (filename.Equals(BasePath.Name + "Modules/SandBox/ModuleData/settlements.xml")) filename = BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/settlements.xml";
				return true;
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
