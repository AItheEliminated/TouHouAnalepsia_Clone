using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.BattleSystem.Shields
{
	public class ShieldBehavior : MissionLogic
	{
		/// <summary>
		/// </summary>
		/// <returns> true if shield worked and this hit had been stopped.</returns>
		public static bool CheckShieldHit(Mission mission, Agent attacker, Agent defender, Mission.Missile missile,
			Vec3 missilePosition, Vec3 missileAngularVelocity, Vec3 movementVelocity, ref Blow blow)
		{
			var shield = defender.GetComponent<ShieldComponent>();
			if (shield != null)
			{
				shield.LastHitTime = mission.Time;
				if (shield.Durability > 0)
				{
					var damage = blow.InflictedDamage;
					var effectPos = missilePosition;
					var velNor = movementVelocity.NormalizedCopy();
					var agentPos = defender.GetChestGlobalPosition();
					agentPos.z = effectPos.z;
					var delta = effectPos - agentPos;
					var length = delta.Length;
					if (Vec3.DotProduct(delta, velNor) > 0)
					{
						effectPos = effectPos - length * velNor * 2;
					}

					effectPos = effectPos + (length - 0.5f) * velNor;

					if (shield.Durability > damage)
					{
						shield.Durability = shield.Durability - damage;
						mission.AddParticleSystemBurstByName(
							"ps_shield_fairy", new MatrixFrame(Mat3.Identity, effectPos), true);
						return true;
					}
					else
					{
						var subDamage = damage - shield.Durability;
						shield.Durability = 0;
						blow.InflictedDamage = subDamage;
						mission.AddParticleSystemBurstByName(
							"ps_shield_break", new MatrixFrame(Mat3.Identity, effectPos), true);
					}
				}
			}

			return false;
		}

		public override void OnAgentCreated(Agent agent)
		{
			if (agent.IsHuman && agent.Character.Name.Contains("Kappa"))
			{
				agent.AddComponent(new ShieldComponent(agent) { Durability = 200, MaxDurability = 200 });
			}
		}

		/*public override void OnMissionTick(float dt)
		{
			foreach (var agent in Mission.Agents)
			{
				if (agent.IsHuman && agent.IsActive())
				{
					var shield = agent.GetComponent<ShieldComponent>();
					if (shield != null)
					{
						if (shield.Durability < shield.MaxDurability &&
						    (Mission.Time - shield.LastHitTime) > 10)
						{
							shield.Durability = Math.Min(shield.Durability + 1, shield.MaxDurability);
						}
					}
				}
			}
		}*/

		/*public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent,
			int damage, in MissionWeapon affectorWeapon)
		{
			var shield = affectedAgent.GetComponent<ShieldComponent>();
			if (shield != null)
			{
				shield.LastHitTime = Mission.Time;
			}
		}*/
	}
}