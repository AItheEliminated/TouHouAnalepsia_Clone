using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TouhouAnalepsia.Utils;
using TouhouAnalepsia.Visuals;

namespace TouhouAnalepsia.Editor.Playground
{
	[MissionManager]
	public static class PlaygroundMission
	{
		[MissionMethod(UsableByEditor = true)]
		public static Mission OpenPlaygroundMission(string scene, string level = "")
		{
			return MissionState.OpenNew("Playground", new MissionInitializerRecord(scene)
			{
				DoNotUseLoadingScreen = false,
				AtmosphereOnCampaign = new AtmosphereInfo()
				{
					AtmosphereName = "default_summer",
					TimeInfo = new TimeInformation()
					{
						Season = 0,
						TimeOfDay = 12f
					}
				},
				DamageToFriendsMultiplier = 1f,
				DamageToPlayerMultiplier = 0f,
				DisableDynamicPointlightShadows = false,
				EnableSceneRecording = false,
				NeedsRandomTerrain = false,
				PlayingInCampaignMode = false,
				RandomTerrainSeed = 0,
				SceneLevels = level,
				TimeOfDay = 12f
			}, (Mission missionController) => new MissionBehaviour[]
			{
				new MissionOptionsComponent(),
				new AgentTownAILogic(),
				new MissionHardBorderPlacer(),
				new HighlightsController(),
				new BattleHighlightsController(),
				new PlayerSpawnController()
			}, true, true);
		}

		private class PlayerSpawnController : MissionLogic
		{
			private Game _game;

			public PlayerSpawnController()
			{
				_game = Game.Current;
			}

			public override void AfterStart()
			{
				BasicCharacterObject player = this._game.ObjectManager.GetObject<BasicCharacterObject>("et_rabbit_1");
				MatrixFrame identity = GetPlayerSideSpawnFrameInSettlement(true);
				AgentBuildData agentBuildData = new AgentBuildData(player);
				agentBuildData.InitialFrame(identity).Controller(Agent.ControllerType.Player);
				Mission.SpawnAgent(agentBuildData, false, 0).WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);

				BasicCharacterObject npc = this._game.ObjectManager.GetObject<BasicCharacterObject>("testguy");
				agentBuildData = new AgentBuildData(npc);
				agentBuildData.InitialFrame(identity).Controller(Agent.ControllerType.AI);
				Mission.SpawnAgent(agentBuildData, false, 0).WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);
				Mission.SpawnAgent(agentBuildData, false, 0).WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);

				Mission.CombatType = Mission.MissionCombatType.Combat;
			}

			public override bool MissionEnded(ref MissionResult missionResult)
			{
				return Mission.InputManager.IsGameKeyPressed(4);
			}

			private MatrixFrame GetPlayerSideSpawnFrameInSettlement(bool spawnWithHorse)
			{
				GameEntity gameEntity;
				if ((gameEntity = base.Mission.Scene.FindEntityWithTag("spawnpoint_player")) == null)
				{
					gameEntity = (base.Mission.Scene.FindEntitiesWithTag("sp_player_conversation").FirstOrDefault<GameEntity>() ?? base.Mission.Scene.FindEntityWithTag("spawnpoint_player_outside"));
				}
				GameEntity gameEntity2 = gameEntity;
				MatrixFrame result = (gameEntity2 != null) ? gameEntity2.GetFrame() : MatrixFrame.Identity;
				result.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				return result;
			}
		}
	}
}