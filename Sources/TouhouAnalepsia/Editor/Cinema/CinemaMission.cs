using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TouhouAnalepsia.Editor.Playground;
using TouhouAnalepsia.Utils.Cinema;

namespace TouhouAnalepsia.Editor.Cinema
{
	[MissionManager]
	public static class CinemaMission
	{
		[MissionMethod(UsableByEditor = true)]
		public static Mission OpenCinemaMission(string scene, string level = "")
		{
			return MissionState.OpenNew("Cinema", new MissionInitializerRecord(scene)
			{
				DoNotUseLoadingScreen = false,
				AtmosphereOnCampaign = new AtmosphereInfo()
				{
					AtmosphereName = "battlefield",
					TimeInfo = new TimeInformation()
					{
						Season = 1,
						TimeOfDay = 6f
					},
					RainInfo = new RainInformation()
					{
						Density = 1.0f
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
				TimeOfDay = 6f
			}, (Mission missionController) => new MissionBehaviour[]
			{
				new MissionOptionsComponent(),
				new BattleMissionStarterLogic(),
				new CustomBattleAgentLogic(),
				new AgentBattleAILogic(),
				new MissionBoundaryPlacer(),
				//new DeploymentHandler(true),
				new PV1Behaviour()
				//,new CinematicBehaviour()

			}, true, true);
		}

		private class PV1Behaviour : MissionLogic
		{
			private const bool SPAWN_FAKE_REINFORCEMENT = false;
			private const bool ATTACKER_MOVE_TO_SCRIPT_TARGET = false;

			private bool _playerSpawn = false;
			private List<MatrixFrame> attackSpawnLocs = new List<MatrixFrame>();
			private List<MatrixFrame> defendSpawnLocs = new List<MatrixFrame>();
			private List<MatrixFrame> defendReinforcementSpawnLocs = new List<MatrixFrame>();
			private BasicCharacterObject _attackTroopMelee;
			private BasicCharacterObject _attackTroopRanged;
			private BasicCharacterObject _attackTroopSpec;
			private BasicCharacterObject defendTroop1;
			private ItemObject _itemRanged;
			private ItemObject _itemMelee;

			private const int ATTACKER_COUNT = 32;
			private const int DEFENDER_COUNT = 32;

			private float _spawnWaveTimer = -999;
			private float _meleeCheckTimer = 0;
			private float _spawnFakeReinforcementTimer = 1;
			private MatrixFrame _fakeReinforcementSpawnPoint;
			private MatrixFrame _fakeReinforcementMovePoint;
			private MatrixFrame _trailerHackerSpawnPoint;
			private MatrixFrame _scriptMovePoint;

			public override void EarlyStart()
			{
				Debug.Print("Early Start");
				_attackTroopMelee = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("sg_kappa_1");
				_attackTroopRanged = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("sg_kappa_2");
				_attackTroopSpec = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("sg_kappa_trailer");
				defendTroop1 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("et_rabbit_1");
				_itemRanged = Game.Current.ObjectManager.GetObject<ItemObject>("et_rifle_a");
				_itemMelee = Game.Current.ObjectManager.GetObject<ItemObject>("et_rifle_a_melee");

				Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
				Mission.CombatType = Mission.MissionCombatType.Combat;

				Debug.Print("Create Teams");
				Team attackTeam = Mission.Teams.Add(BattleSideEnum.Attacker);
				Team defendTeam = Mission.Teams.Add(BattleSideEnum.Defender);

				Debug.Print("Add AI");
				var attackAi = new TeamAIGeneral(Mission, attackTeam);
				attackAi.AddTacticOption(new TacticCharge(attackTeam));
				attackTeam.AddTeamAI(attackAi);

				var defendAi = new TeamAIGeneral(Mission, defendTeam);
				defendAi.AddTacticOption(new TacticStop(defendTeam));
				defendTeam.AddTeamAI(defendAi);

				Debug.Print("Find spawn points");
				List<GameEntity> entities = new List<GameEntity>();
				Mission.Scene.GetEntities(ref entities);
				foreach (var entity in entities)
				{
					if (entity.HasTag("spawnpoint"))
					{
						if (entity.HasTag("attacker") && entity.HasTag("front"))
						{
							attackSpawnLocs.Add(entity.GetFrame());
						}
						else if (entity.HasTag("defender"))
						{
							if (entity.HasTag("reinforcement")) 
								defendReinforcementSpawnLocs.Add(entity.GetFrame());
							else
								defendSpawnLocs.Add(entity.GetFrame());
						}
					}
					else if (entity.HasTag("fakereinforcementspawn"))
					{
						_fakeReinforcementSpawnPoint = entity.GetFrame();
					}
					else if (entity.HasTag("fakereinforcementmove"))
					{
						_fakeReinforcementMovePoint = entity.GetFrame();
					}

					if (entity.HasTag("spec"))
					{
						_trailerHackerSpawnPoint = entity.GetFrame();
					}

					if (entity.HasTag("scriptmovetarget"))
					{
						_scriptMovePoint = entity.GetFrame();
					}
				}

				Mission.PlayerTeam = attackTeam;
				Mission.SetMissionMode(MissionMode.Battle, true);

				attackTeam.SetIsEnemyOf(defendTeam, true);
				defendTeam.SetIsEnemyOf(attackTeam, true);

				//SpawnTroop(Mission.AttackerTeam, _attackTroopSpec, _trailerHackerSpawnPoint);

				/*var list = new List<GameEntity>();
				var reports = new HashSet<GameEntity>();
				Mission.Scene.GetEntities(ref list);
				foreach (var entity in list)
				{
					CheckEntity(entity, reports);
				}*/

				var mat = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 1000, 1));
				Mission.SetCameraFrame(ref mat, 1.0f);
			}

			private void CheckEntity(GameEntity entity, HashSet<GameEntity> reports)
			{
				var modelNum = entity.GetComponentCount(GameEntity.ComponentType.MetaMesh);
				for (int i = 0; i < modelNum; i++)
				{
					var model = entity.GetComponentAtIndex(i, GameEntity.ComponentType.MetaMesh) as MetaMesh;
					if (model == null)
						reports.Add(entity);
					else
					{
						var meshNum = model.MeshCount;
						for (int j = 0; j < meshNum; j++)
						{
							var mesh = model.GetMeshAtIndex(j);
							if (mesh.GetFaceCount() == 0)
								reports.Add(entity);
						}
					}
				}
				foreach (var child in entity.GetChildren())
				{
					CheckEntity(child, reports);
				}
			}

			public override void OnMissionTick(float dt)
			{
				if (Mission.Time - _spawnWaveTimer > 10)
				{
					Debug.Print($"Check spawn on time {Mission.Time}");
					TrySpawnTroop();
					_spawnWaveTimer = Mission.Time;
				}

				if (Mission.Time - _meleeCheckTimer > 2)
				{
					foreach (var agent in Mission.DefenderTeam.ActiveAgents)
					{
						var enemyCount = Mission.GetNearbyEnemyAgentCount(Mission.DefenderTeam, agent.Position.AsVec2, 6);
						if (enemyCount > 0)
						{
							if (!agent.Equipment[0].IsEmpty && agent.Equipment[0].Item == _itemRanged)
							{
								var weapon = new MissionWeapon(_itemMelee, null, null);
								agent.RemoveEquippedWeapon(0);
								agent.EquipWeaponWithNewEntity(0, ref weapon);
								agent.WieldInitialWeapons(Agent.WeaponWieldActionType.Instant);
							}
						}
					}
				}

				if (SPAWN_FAKE_REINFORCEMENT && Mission.Time - _spawnFakeReinforcementTimer > 0.6f)
				{
					var agent = SpawnTroop(Mission.DefenderTeam, defendTroop1, _fakeReinforcementSpawnPoint);
					var pos = new WorldPosition(Mission.Scene, _fakeReinforcementMovePoint.origin);
					agent.SetScriptedPosition(ref pos, false, Agent.AIScriptedFrameFlags.NeverSlowDown);
					_spawnFakeReinforcementTimer = Mission.Time;
				}
			}

			private void TrySpawnTroop()
			{
				Team attackTeam = Mission.AttackerTeam;
				Team defendTeam = Mission.DefenderTeam;

				var spawnAttacker = ATTACKER_COUNT - attackTeam.ActiveAgents.Count;
				var spawnDefender = DEFENDER_COUNT - defendTeam.ActiveAgents.Count;

				if (spawnAttacker > 0)
				{
					Debug.Print($"Spawn {spawnAttacker} attackers");
					attackTeam.GetFormation(FormationClass.Ranged).BeginSpawn(spawnAttacker, false);
					for (int i = 0; i < spawnAttacker; i++)
					{
						var frame = attackSpawnLocs[MBRandom.DeterministicRandomInt(attackSpawnLocs.Count)];
						var troop = MBRandom.DeterministicRandomInt(2) == 0 ? _attackTroopMelee : _attackTroopRanged;
						var agent = SpawnTroop(attackTeam, troop, frame);
						if (ATTACKER_MOVE_TO_SCRIPT_TARGET)
						{
							var pos = new WorldPosition(Mission.Scene, _scriptMovePoint.origin);
							agent.SetScriptedPosition(ref pos, false, Agent.AIScriptedFrameFlags.NeverSlowDown);
						}
					}
					attackTeam.GetFormation(FormationClass.Ranged).EndSpawn();
				}

				if (spawnDefender > 0)
				{
					var spawnRein = Mission.Time > 20;
					var spawnLocs = spawnRein ? defendReinforcementSpawnLocs : defendSpawnLocs;
					spawnDefender = spawnRein ? spawnDefender : defendSpawnLocs.Count;
					Debug.Print($"Spawn {spawnDefender} attackers");
					defendTeam.GetFormation(FormationClass.Ranged).BeginSpawn(spawnDefender, false);
					for (int i = 0; i < spawnDefender; i++)
					{
						var frame = spawnLocs[spawnRein ? MBRandom.DeterministicRandomInt(spawnLocs.Count) : i];
						var agent = SpawnTroop(defendTeam, defendTroop1, frame);
						if (spawnRein)
						{
							var defendPos = defendSpawnLocs[MBRandom.DeterministicRandomInt(defendSpawnLocs.Count)];
							var pos = new WorldPosition(Mission.Scene, defendPos.origin);
							agent.SetScriptedPosition(ref pos, false, Agent.AIScriptedFrameFlags.RangerCanMoveForClearTarget);
						}
					}
					defendTeam.GetFormation(FormationClass.Ranged).EndSpawn();
				}

				attackTeam.ExpireAIQuerySystem();
				defendTeam.ExpireAIQuerySystem();

				attackTeam.UpdateCachedEnemyDataForFleeing();
				defendTeam.UpdateCachedEnemyDataForFleeing();

				Debug.Print($"Spawn finish");
			}

			private Agent SpawnTroop(Team team, BasicCharacterObject troopData, MatrixFrame location)
			{
				var agentBuildData = new AgentBuildData(troopData);
				agentBuildData.InitialFrame(location);
				agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(troopData));
				agentBuildData.Team(team);
				agentBuildData.Formation(team.GetFormation(FormationClass.Ranged));

				if (!_playerSpawn)
				{
					agentBuildData.Controller(Agent.ControllerType.Player);
				}
				else
				{
					agentBuildData.Controller(Agent.ControllerType.AI);
				}
				var agent = Mission.SpawnAgent(agentBuildData);
				agent.WieldInitialWeapons();
				agent.SetWatchState(Agent.WatchState.Alarmed);


				if (!_playerSpawn)
				{
					_playerSpawn = true;
					team.SetPlayerRole(true, false);
					Mission.MainAgent = agent;
					foreach (var formation in team.FormationsIncludingSpecialAndEmpty)
					{
						formation.Captain = agent;
						formation.PlayerOwner = agent;
					}
				}
				
				return agent;
			}

			/*public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
			{
				if (!affectorWeapon.IsEmpty)
					Logger.Log($"Weapon {affectorWeapon.Item.Name.ToString()} damages {damage}");
			}*/
		}
	}
}