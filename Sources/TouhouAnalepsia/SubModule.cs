using StoryMode.StoryModePhases;
using System.Reflection;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Abilities;
using TouhouAnalepsia.CampaignSystem.Behaviors;
using TouhouAnalepsia.Editor.FastScene;
using TouhouAnalepsia.Editor.Playground;
using Module = TaleWorlds.MountAndBlade.Module;
using TouhouAnalepsia.Patches;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase
	{
		public const string MODULE_ID = "TouhouAnalepsia";

		protected override void OnSubModuleLoad() {
			base.OnSubModuleLoad();
			PatchManager.Prepatch(); //For transpilers and whatnot, in case they need it.
			HarmonyHandler.PatchAll();

			PatchManager.DefaultPatches();

			//CampaignOptions.EnableCampaignTutorials = false;
#if !DEBUG
			if (Utilities.EditModeEnabled) {
#endif
				//TODO still not working, will look into it more in a bit.
				Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
					"Playground", new TextObject("Playground"), 1,
					() => { MBGameManager.StartNewGame(new FastSceneManager(
						() =>
						{
							PlaygroundMission.OpenPlaygroundMission("scn_playground", "");
						}
						));
					}, () => (false, TextObject.Empty)));
#if !DEBUG
			}
#endif

			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedCameraHeight, 1000);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedDistanceFromCenterPointX, 720);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedDistanceFromCenterPointY, 720);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapCenterPointX, 360);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapCenterPointY, 360);
		}
		protected override void OnBeforeInitialModuleScreenSetAsRoot() {
			// skip the shit intro video while developing. I almost get smithing-phobia for it.
#if DEBUG
			var field = typeof(Module).GetField("_splashScreenPlayed", BindingFlags.Instance | BindingFlags.NonPublic);
			field?.SetValue(Module.CurrentModule, true);
#endif
		}

		public override void OnMissionBehaviourInitialize(Mission mission) {
			mission.AddMissionBehaviour(new AbilityBattleLogic());

			base.OnMissionBehaviourInitialize(mission);
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
			base.OnGameStart(game, gameStarterObject);
			CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;

			game.ObjectManager.RegisterType<Ability>("Ability", "Abilities", 61U, true);
			game.ObjectManager.LoadXML("Abilities");
			
			//game.ObjectManager.RegisterType<Race>("Race", "Races", 62U, true);
			//game.ObjectManager.LoadXML("Races");

			if (game.GameType is Campaign) {
				FactionCulture.initializeFactionCultures();

				TouhouModelManager.initializeTouhouModels(gameStarterObject);

				LoadLocalizationData(campaignGameStarter);
   				
				//campaignGameStarter.AddBehavior(new MyCampainBehavior()); //Disabled temporarily - this sort of addition should go through a pull request first.
																			//	Plus I'm pretty sure this was moreso experimentation than a proper feature.
			}
		}
		public override void OnCampaignStart(Game game, object starterObject) {
			if (game.GameType is Campaign) {
				//CampaignOptions.EnableCampaignTutorials = false;
				TutorialPhase tutorialPhase = TutorialPhase.Instance;
				
				if(tutorialPhase != null) {
					tutorialPhase.CompleteTutorial(true);
				}

				CampaignGameStarter campaignGameStarter = (CampaignGameStarter)starterObject;

				campaignGameStarter.AddBehavior(new CompanionHandler.CompanionCampaignBehavior());
			}

			base.OnCampaignStart(game, starterObject);
		}

		private void LoadLocalizationData(CampaignGameStarter gameStarter) {
			gameStarter.LoadGameTexts(BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/module_strings.xml");
		}
	}
}
