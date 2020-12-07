using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase {
		protected override void OnSubModuleLoad() {
			base.OnSubModuleLoad();
		}
		public override void OnMissionBehaviourInitialize(Mission mission) {
			mission.AddMissionBehaviour(new AbilityBattleLogic());

			base.OnMissionBehaviourInitialize(mission);
		}
		protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
			FactionCulture.initializeFactionCultures();

			TouhouModelManager.initializeTouhouModels(gameStarterObject);

			loadLocalizationData((CampaignGameStarter)gameStarterObject);
			//game.ObjectManager.RegisterType<Race>("Race", "Races", 50U, true);

			base.OnGameStart(game, gameStarterObject);
		}
		public override void OnCampaignStart(Game game, object starterObject) {
			CampaignGameStarter campaignGameStarter = (CampaignGameStarter)starterObject;

			campaignGameStarter.AddBehavior(new CompanionHandler.CompanionCampaignBehavior());

			base.OnCampaignStart(game, starterObject);
		}

		private void loadLocalizationData(CampaignGameStarter gameStarter) {
			gameStarter.LoadGameTexts(BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/module_strings.xml");
		}
	}
}
