using System.Collections.Generic;
using SandBox;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace TouhouAnalepsia.Editor.FastScene
{
	public class FastSceneGame : GameType
	{
		public static FastSceneGame Current => Game.Current.GameType as FastSceneGame;

		protected override void OnInitialize()
		{
			Game currentGame = base.CurrentGame;
			GameTextManager gameTextManager = currentGame.GameTextManager;
			this.InitializeGameTexts(gameTextManager);
			IGameStarter gameStarter = new BasicGameStarter();
			this.InitializeGameModels(gameStarter);
			base.GameManager.InitializeGameStarter(currentGame, gameStarter);
			base.GameManager.OnGameStart(base.CurrentGame, gameStarter);
			MBObjectManager objectManager = currentGame.ObjectManager;
			currentGame.SetBasicModels(gameStarter.Models);
			currentGame.CreateGameManager();
			base.GameManager.BeginGameStart(base.CurrentGame);
			base.CurrentGame.SetRandomGenerators();
			currentGame.InitializeDefaultGameObjects();
			currentGame.LoadBasicFiles();
			this.LoadCustomGameXmls();
			objectManager.UnregisterNonReadyObjects();
			currentGame.SetDefaultEquipments(new Dictionary<string, Equipment>());
			objectManager.UnregisterNonReadyObjects();
			base.GameManager.OnNewCampaignStart(base.CurrentGame, null);
			base.GameManager.OnAfterCampaignStart(base.CurrentGame);
			base.GameManager.OnGameInitializationFinished(base.CurrentGame);
		}

		private void InitializeGameModels(IGameStarter basicGameStarter)
		{
			basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
			basicGameStarter.AddModel(new CustomBattleAgentStatCalculateModel());
			basicGameStarter.AddModel(new DefaultMissionDifficultyModel());
			basicGameStarter.AddModel(new CustomBattleApplyWeatherEffectsModel());
			basicGameStarter.AddModel(new CustomBattleAutoBlockModel());
			basicGameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
			basicGameStarter.AddModel(new DefaultRidingModel());
			basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
			basicGameStarter.AddModel(new CustomBattleMoraleModel());
			basicGameStarter.AddModel(new CustomBattleInitializationModel());
		}

		private void InitializeGameTexts(GameTextManager gameTextManager)
		{
			gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
			gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
			gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
			gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
		}

		public override bool IsDevelopment {
			get {
				return true;
			}
		}

		private void LoadCustomGameXmls() {
			//ObjectManager.LoadXML("Monsters");
			//ObjectManager.LoadXML("SkeletonScales");
			//ObjectManager.LoadXML("ItemModifiers");
			//ObjectManager.LoadXML("ItemModifierGroups");
			//ObjectManager.LoadXML("CraftingPieces");
			//ObjectManager.LoadXML("CraftingTemplates");
			ObjectManager.LoadXML("Items");
			ObjectManager.LoadXML("EquipmentRosters");
			ObjectManager.LoadXML("NPCCharacters");
			ObjectManager.LoadXML("SPCultures");
		}

		public override void OnDestroy()
		{
		}

		public override void OnStateChanged(GameState oldState)
		{
		}

		protected override void BeforeRegisterTypes(MBObjectManager objectManager)
		{
		}

		protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
		{
			nextState = GameTypeLoadingStates.None;
			switch (gameTypeLoadingState)
			{
				case GameTypeLoadingStates.InitializeFirstStep:
					base.CurrentGame.Initialize();
					nextState = GameTypeLoadingStates.WaitSecondStep;
					return;
				case GameTypeLoadingStates.WaitSecondStep:
					nextState = GameTypeLoadingStates.LoadVisualsThirdState;
					return;
				case GameTypeLoadingStates.LoadVisualsThirdState:
					nextState = GameTypeLoadingStates.PostInitializeFourthState;
					break;
				case GameTypeLoadingStates.PostInitializeFourthState:
					break;
				default:
					return;
			}
		}

		protected override void OnRegisterTypes(MBObjectManager objectManager)
		{
			objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "NPCCharacters", 43U, true);
			objectManager.RegisterType<BasicCultureObject>("Culture", "SPCultures", 17U, true);
		}
	}
}