using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Items;

namespace TouhouAnalepsia.BattleSystem.Firearms
{
	public class FirearmClip : SynchedMissionObject
	{
		public int Remaining { set; get; }

		public int Maximum { set; get; }

		public ExtendedItemInfo.FirearmTrigger Trigger { set; get; }

		public static void AddClipComponent(GameEntity weaponEntity, 
			int curAmmo, int maxAmmo, ExtendedItemInfo.FirearmTrigger trigger)
		{
			weaponEntity.CreateAndAddScriptComponent(nameof(FirearmClip));
			foreach (var component in weaponEntity.GetScriptComponents<FirearmClip>())
			{
				component.Remaining = curAmmo;
				component.Maximum = maxAmmo;
				component.Trigger = trigger;
				break;
			}
		}
	}
}