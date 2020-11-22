using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase {
		protected override void OnSubModuleLoad() {
			base.OnSubModuleLoad();
		}
		public override void OnMissionBehaviourInitialize(Mission mission) {
			base.OnMissionBehaviourInitialize(mission);
			mission.AddMissionBehaviour(new AbilityBattleLogic());
		}
		protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
			gameStarterObject.AddModel(new TouhouMarriageModel());

			game.ObjectManager.RegisterType<Race>("Race", "Races", 50U, true);
		}
	}
}
