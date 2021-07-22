using System.Reflection;

using TaleWorlds.CampaignSystem;

using TaleWorlds.Core;

using TaleWorlds.Engine;

using TaleWorlds.Library;

using TaleWorlds.Localization;

using TaleWorlds.MountAndBlade;

using TouhouAnalepsia.Abilities;

using TouhouAnalepsia.BattleSystem.Firearms;

using TouhouAnalepsia.BattleSystem.Shields;

using TouhouAnalepsia.CampaignSystem.Behaviors;

using TouhouAnalepsia.Editor.Cinema;

using TouhouAnalepsia.Editor.FastScene;

using TouhouAnalepsia.Editor.Playground;

using TouhouAnalepsia.Utils.Inits;

using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Abilities;
using TouhouAnalepsia.BattleSystem.Firearms;
using TouhouAnalepsia.BattleSystem.Shields;
using TouhouAnalepsia.CampaignSystem.Behaviors;
using TouhouAnalepsia.Editor.Cinema;
using TouhouAnalepsia.Editor.FastScene;
using TouhouAnalepsia.Editor.Playground;
using TouhouAnalepsia.Utils.Inits;
using TouhouAnalepsia.Visuals;
using Module = TaleWorlds.MountAndBlade.Module;

namespace TouhouAnalepsia
{
	public class SubModule : MBSubModuleBase
	{
		public const string MODULE_ID = "TouhouAnalepsia";

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			Patches.Prepatch(); //For transpilers and whatnot, in case they need it.
			HarmonyHandler.PatchAll();

			Patches.DefaultPatches();

			InitManager.OnSubModuleLoad();

			//CampaignOptions.EnableCampaignTutorials = false; // 1.5.9
#if !DEBUG
			if (Utilities.EditModeEnabled) {
#endif
			//TODO still not working, will look into it more in a bit.
			Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
					"Playground", new TextObject("Playground"), 1,
					() => {
						MBGameManager.StartNewGame(new FastSceneManager(
					() =>
					{
						PlaygroundMission.OpenPlaygroundMission("scn_playground", "");
					}
					));
					}, () => (false, TextObject.Empty)));

			//TODO: wait for updating scene and its models
			/*Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
				"Cinema", new TextObject("Cinema"), 2,
				() => {
					MBGameManager.StartNewGame(new FastSceneManager(
						() =>
						{
							CinemaMission.OpenCinemaMission("scn_Trench Warfare-Valley Town-hairball", "");
						}
					));
				}, () => (false, TextObject.Empty)));*/

#if !DEBUG
			}
#endif

			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedCameraHeight, 1000);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedDistanceFromCenterPointX, 720);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapMaxAllowedDistanceFromCenterPointY, 720);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapCenterPointX, 360);
			//TaleWorlds.Core.ManagedParameters.SetParameter(TaleWorlds.Core.ManagedParametersEnum.CampaignMapCenterPointY, 360);
		}
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			// skip the shit intro video while developing. I almost get smithing-phobia for it.
#if DEBUG
			var field = typeof(Module).GetField("_splashScreenPlayed", BindingFlags.Instance | BindingFlags.NonPublic);
			field?.SetValue(Module.CurrentModule, true);
#endif
			InitManager.OnBeforeInitialModuleScreenSetAsRoot();
		}

		public override void OnMissionBehaviourInitialize(Mission mission)
		{
			//mission.AddMissionBehaviour(new AbilityBattleLogic());
			mission.AddMissionBehaviour(new AgentVisualsAuxBehavior());
			mission.AddMissionBehaviour(new FirearmBehavior());
			mission.AddMissionBehaviour(new ShieldBehavior());
			mission.AddMissionBehaviour(new TrailerHack());

			base.OnMissionBehaviourInitialize(mission);
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			base.OnGameStart(game, gameStarterObject);

			BodyMeshCache.Init();

			game.ObjectManager.RegisterType<Ability>("Ability", "Abilities", 61U, true);
			game.ObjectManager.LoadXML("Abilities");

			if (game.GameType is Campaign)
			{
				FactionCulture.initializeFactionCultures();

				TouhouModelManager.initializeTouhouModels(gameStarterObject);

				loadLocalizationData((CampaignGameStarter)gameStarterObject);
				//game.ObjectManager.RegisterType<Race>("Race", "Races", 50U, true);
				CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
				campaignGameStarter.AddBehavior(new MyCampainBehavior());
			}
		}

		public override void OnGameEnd(Game game)
		{
			BodyMeshCache.DeInit();
		}

		public override void OnCampaignStart(Game game, object starterObject)
		{
			if (game.GameType is Campaign)
			{
				//CampaignOptions.EnableCampaignTutorials = false; // 1.5.9

				CampaignGameStarter campaignGameStarter = (CampaignGameStarter)starterObject;

				campaignGameStarter.AddBehavior(new CompanionHandler.CompanionCampaignBehavior());
			}

			base.OnCampaignStart(game, starterObject);
		}

		private void loadLocalizationData(CampaignGameStarter gameStarter)
		{
			gameStarter.LoadGameTexts(BasePath.Name + "Modules/TouhouAnalepsia/ModuleData/module_strings.xml");
		}

		// Used for deterministic mode.
		/*[HarmonyPatch(typeof(MBInitialScreenBase))]
		[HarmonyPatch("OnFrameTick")]
		public class DeterministicPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldstr && code.operand.Equals("CustomBattle"))
					{
						yield return new CodeInstruction(OpCodes.Ldstr, "Cinema");
					}
					else
					{
						yield return code;
					}
				}
			}
		}*/
	}
}