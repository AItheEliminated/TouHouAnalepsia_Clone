using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia {
	class AbilityBattleLogic : MissionLogic {
		public override void OnAgentBuild(Agent agent, Banner banner) {
			base.OnAgentCreated(agent);
			if (!initializedAgents.Contains(agent)) initializeAgent(agent);
		}
		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow) {
			base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
			initializedAgents.Remove(affectedAgent);
		}
		public override void OnMissionTick(float dt) {
			base.OnMissionTick(dt);
			tickCounter += 1;
			if (initializedAgents.Count < Mission.Agents.Count) { //Simple check to initialize agents, probably a problem if too many agents are on the field. Further testing, however, is needed.
				int nullAgents=0;
				foreach (Agent agent in Mission.Agents) {
					if (!initializedAgents.Contains(agent)) initializeAgent(agent);
					if (agent != null) {
						nullAgents++;
						continue;
					}
					if (agent.IsHero) {
						Logger.Log("Hero was here!");
					}
				}
				//if(nullAgents!=0) Logger.Log(nullAgents.ToString().Add(" Null Agents present! (Ignore if you're in the tutorial area)")); //DEBUG information, issue has mostly been solved.
			}
			if (tickCounter%tickRate==0) {//don't run all the time, only once in a while. (~once a second)
				foreach (Agent agent in initializedAgents) {
					if (DoesAgentNeedTickEvent(agent)) {
						Ability agentAbility;
						abilities.TryGetValue(agent, out agentAbility);
						//Handle HP regen
						int hpRegen = agentAbility.hpHealedPerMinute;
						int extraHP = Math.Max(0,hpRegen-60);
						//InformationManager.DisplayMessage(new InformationMessage("Healthy?")); //DEBUG
						if (hpRegen>=0) {
							//InformationManager.DisplayMessage(new InformationMessage("Healthy!")); //DEBUG
							if (tickCounter % (tickRate * (61 - Math.Min(60,hpRegen))) == 0) {
								int hpToAdd = 1;

								if (tickCounter % (tickRate * 5) == 0) hpToAdd+=(int)Math.Ceiling((double)(extraHP/12));

								agent.Health += hpToAdd;
								if (agent.Health > agent.HealthLimit) agent.Health = agent.HealthLimit; //Ensures health isn't past max.
							}
						}
					}
				}
			}
		}
		public override void AfterStart() {
			base.OnBehaviourInitialize();
			tickCounter = 0;
			initializedAgents = new List<Agent>();
			abilities = new Dictionary<Agent, Ability>();
		}
		public override void OnClearScene() {
			base.OnClearScene();
			tickCounter = 0;
			initializedAgents = new List<Agent>();
			abilities = new Dictionary<Agent, Ability>();
		}

		private static void initializeAgent(Agent agent) {
			if (agent == null) return;
			if (initializedAgents.Contains(agent)) return;
			initializedAgents.Add(agent);
			//agent.HealthLimit *= 2;
			//agent.Health *= 2;

			Ability ability = Ability.calculateAbility(agent);

			agent.HealthLimit *= (float)ability.hpScaled;
			agent.Health = agent.HealthLimit;

			//TODO implement speed adjustment

			abilities.Add(agent, ability);
			if (agent.Controller==Agent.ControllerType.Player) {
				//Logger.Log("Health and Stuff Adjustment for Player!"); //DEBUG information, currently the issue has been solved.
			}
		}

		private static bool DoesAgentNeedTickEvent(Agent agent) {
			Ability ability;
			if (!abilities.TryGetValue(agent, out ability)) {
				InformationManager.DisplayMessage(new InformationMessage("Error in DoesAgentNeedTickEvent!"));
				return false;
			}
			//InformationManager.DisplayMessage(new InformationMessage(ability.hpHealedPerMinute.ToString() + ", "+ ability.hpScaled.ToString())); //DEBUG
			return ability.hpHealedPerMinute > 0;
		}

		private static int tickCounter=0;
		private static readonly int tickRate = 60;

		//Initialized once per mission, more work may need to go into this in the future, to ensure no bugs.
		public static Dictionary<Agent, Ability> abilities = new Dictionary<Agent, Ability>();
		public static List<Agent> initializedAgents = new List<Agent>();
	}
}
