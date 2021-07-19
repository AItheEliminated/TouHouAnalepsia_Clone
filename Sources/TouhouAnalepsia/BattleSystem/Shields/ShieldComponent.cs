using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.BattleSystem.Shields
{
	public class ShieldComponent : AgentComponent
	{
		public int Durability { set; get; }

		public int MaxDurability { set; get; }

		public float LastHitTime { set; get; }

		public ShieldComponent(Agent agent) : base(agent)
		{
			Durability = 100;
			MaxDurability = 100;
		}
	}
}