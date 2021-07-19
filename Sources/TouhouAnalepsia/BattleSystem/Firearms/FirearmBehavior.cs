using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Items;

namespace TouhouAnalepsia.BattleSystem.Firearms
{
	public class FirearmBehavior : MissionLogic
	{
		public static readonly Vec3 DEFAULT_MUZZLE_POSITION = new Vec3(0, 0, 1);

		public const string DEFAULT_GUNSHOT_FLASH = "ps_gunfire";

		public const string DEFAULT_GUNSHOT_SOUND = "tha/combat/rifle_fire_a";

		private static int defaultGunshotSoundCode = -1;

		private List<Tuple<Agent, EquipmentIndex, ItemObject>> _delayRearmAgents =
			new List<Tuple<Agent, EquipmentIndex, ItemObject>>();

		public override void OnCreated()
		{
			if (defaultGunshotSoundCode < 0)
				defaultGunshotSoundCode = SoundEvent.GetEventIdFromString(DEFAULT_GUNSHOT_SOUND);
		}

		public override void OnAgentBuild(Agent agent, Banner banner)
		{
			if (!agent.IsHuman)
				return;

			for (EquipmentIndex index = EquipmentIndex.WeaponItemBeginSlot;
				index < EquipmentIndex.NumAllWeaponSlots;
				index++)
			{
				var weapon = agent.Equipment[index];
				if (!weapon.IsEmpty && weapon.Item.PrimaryWeapon.WeaponClass.IsFirearm())
				{
					if (ExtendedItemInfo.TryGet(weapon.Item, out var itemInfo))
					{
						var entity = agent.GetWeaponEntityFromEquipmentSlot(index);
						if (entity != null)
						{
							var maximum = itemInfo.ClipCapacity;
							agent.Equipment.GetAmmoCountAndIndexOfType(weapon.Item.ItemType,
								out var ammo, out _);
							var totalAmmo = weapon.Ammo + ammo;
							var remaining = Math.Min(totalAmmo, maximum);
							var trigger = itemInfo.WeaponTrigger;
							FirearmClip.AddClipComponent(entity, remaining, maximum, trigger);
						}
					}
				}
			}
		}

		/*public override void OnItemPickup(Agent agent, SpawnedItemEntity item)
		{
			foreach (var component in 
				item.GameEntityWithWorldPosition.GameEntity.GetScriptComponents<FirearmClip>())
			{
			}
		}*/

		public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation,
			bool hasRigidBody, int forcedMissileIndex)
		{
			if (shooterAgent.Equipment[weaponIndex].Item.PrimaryWeapon.WeaponClass.IsFirearm())
			{
				var weaponEntity = shooterAgent.GetWeaponEntityFromEquipmentSlot(weaponIndex);
				var itemInfo = ExtendedItemInfo.Get(shooterAgent.Equipment[weaponIndex].Item);
				var agentFrame = shooterAgent.AgentVisuals.GetGlobalFrame();
				var weaponFrame = shooterAgent.AgentVisuals.GetSkeleton().GetBoneEntitialFrame(27);
				weaponFrame = agentFrame.TransformToParent(weaponFrame);
				if (weaponEntity != null && itemInfo.GunshotFlash != null)
				{
					var frame = new MatrixFrame(Mat3.Identity, itemInfo.MuzzlePosition);
					frame = weaponFrame.TransformToParent(frame);
					/*var rot = Mat3.Identity;
					rot.RotateAboutForward(MBMath.HalfPI);
					frame.rotation = rot;*/
					Mission.AddParticleSystemBurstByName(itemInfo.GunshotFlash, frame, true);
					weaponFrame = frame;
				}

				var soundCode = itemInfo.GunshotSoundCode;
				if (soundCode >= 0)
				{
					var se = SoundEvent.CreateEvent(soundCode > 0 ? soundCode : defaultGunshotSoundCode, Mission.Scene);
					if (shooterAgent.IsMainAgent)
						Mission.MakeSound(soundCode, weaponFrame.origin, false, true, shooterAgent.Index, -1 );
					else
						Mission.MakeSound(soundCode, weaponFrame.origin, true, false, -1, -1);
				}

				_delayRearmAgents.Add(Tuple.Create(
					shooterAgent,
					weaponIndex,
					shooterAgent.Equipment[weaponIndex].Item
					));
			}
		}

		public override void OnMissionTick(float dt)
		{
			if (_delayRearmAgents.Count > 0)
			{
				foreach (var tuple in _delayRearmAgents)
				{
					var agent = tuple.Item1;
					var weaponIndex = tuple.Item2;
					var item = tuple.Item3;
					// make sure agent doesn't drop or switch weapon before tick
					if (!agent.Equipment[weaponIndex].IsEmpty &&
					    agent.Equipment[weaponIndex].Item.Id == item.Id)
					{
						/*agent.RemoveEquippedWeapon(weaponIndex);
						var newWeapon =
							new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>("et_rifle_a_melee"),
								null, null);
						agent.EquipWeaponWithNewEntity(weaponIndex, ref newWeapon);
						agent.TryToWieldWeaponInSlot(weaponIndex, Agent.WeaponWieldActionType.Instant, false);
						return;*/

						var weaponEntity = agent.GetWeaponEntityFromEquipmentSlot(weaponIndex);

						if (weaponEntity == null)
							continue;
						foreach (var component in weaponEntity.GetScriptComponents<FirearmClip>())
						{
							component.Remaining = component.Remaining - 1;
							switch (component.Trigger)
							{
								case ExtendedItemInfo.FirearmTrigger.Single:
									component.Remaining = component.Maximum;
									break;
								case ExtendedItemInfo.FirearmTrigger.Bolt:
									if (component.Remaining > 0)
										agent.SetWeaponReloadPhaseAsClient(weaponIndex, 1);
									else
										component.Remaining = component.Maximum;
									break;
								case ExtendedItemInfo.FirearmTrigger.Semi:
									if (component.Remaining > 0)
										agent.SetWeaponReloadPhaseAsClient(weaponIndex, 2);
									else
										component.Remaining = component.Maximum;
									break;
							}
							continue;
						}
					}
				}

				_delayRearmAgents.Clear();
			}
		}

		public override void OnRemoveBehaviour()
		{
			_delayRearmAgents.Clear();
		}
	}
}