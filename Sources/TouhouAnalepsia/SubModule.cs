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
using TaleWorlds.ModuleManager;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase {

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			HarmonyHandler.PatchAll();

			Patches.DefaultPatches();

			CampaignOptions.EnableCampaignTutorials = false;

			if (Utilities.EditModeEnabled)
			{
				/* //TODO fix this, temporarily disabled completely to stop crash.
				Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
					"Playground", new TextObject("Playground"), 1,
					() => { MBGameManager.StartNewGame(new FastSceneManager(
						() =>
						{
							PlaygroundMission.OpenPlaygroundMission("scn_playground", "");
						}
						)); }, false));
				*/
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
