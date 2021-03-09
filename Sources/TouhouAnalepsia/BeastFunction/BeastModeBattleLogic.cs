using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.BeastFunction {
	class BeastModeBattleLogic : MissionLogic {
		private SiegeWeapon beast_unit = null;
		private Agent sel_agent;

		public override void OnAgentBuild(Agent agent, Banner banner) {
			base.OnAgentCreated(agent);

			if (sel_agent==null) sel_agent = agent;
			if (agent == Agent.Main) sel_agent = agent;
		}
		public override void OnMissionTick(float dt) {
			base.OnMissionTick(dt);
			//Logger.Log("Beast Unit: " + beast_unit + ", Selected Agent: " + sel_agent?.Name);
			if (beast_unit != null && sel_agent != null) {
				sel_agent.TeleportToPosition(beast_unit.Entity().GetGlobalFrame().origin);

				//beast_unit.GameEntity.

				/*
				Vec3 pos = sel_agent.Position;

				pos += 3 * Vec3.Up;

				beast_unit.GameEntity.SetLocalPosition(pos);

				MatrixFrame frame = beast_unit.GameEntity.GetGlobalFrame();

				frame.origin = pos;

				beast_unit.GameEntity.SetGlobalFrame(frame);
				*/
			}
		}

		public override void AfterStart() {
			base.AfterStart();
			beast_unit = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<SiegeWeapon>().FirstOrDefault()?.GetFirstScriptOfType<SiegeWeapon>();
			beast_unit.GameEntity.SetMobility(TaleWorlds.Engine.GameEntity.Mobility.dynamic_forced);
		}
	}
}
