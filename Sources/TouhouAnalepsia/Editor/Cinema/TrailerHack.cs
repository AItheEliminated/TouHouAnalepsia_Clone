using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Editor.Cinema
{
	// hack hack hack all the day
	// hack hack hack while i sing this song
	public class TrailerHack : MissionLogic
	{
		public override void OnAgentCreated(Agent agent)
		{
			/*if (!agent.SpawnEquipment[EquipmentIndex.Body].IsEmpty)
			{
				if (agent.SpawnEquipment[EquipmentIndex.Body].Item.StringId == "rabbit_infantry_armor_a")
				{
					if (MBRandom.DeterministicRandomInt(3) == 0)
					{
						agent.SpawnEquipment[EquipmentIndex.Body] = new EquipmentElement(
							MBObjectManager.Instance.GetObject<ItemObject>("rabbit_infantry_armor_b")
						);
					}
				}
			}
			if (!agent.SpawnEquipment[EquipmentIndex.Head].IsEmpty)
			{
				if (agent.SpawnEquipment[EquipmentIndex.Head].Item.StringId == "kappa_helmet_a")
				{
					if (MBRandom.DeterministicRandomInt(2) == 0)
					{
						agent.SpawnEquipment[EquipmentIndex.Head] = new EquipmentElement(
							MBObjectManager.Instance.GetObject<ItemObject>("kappa_helmet_b")
						);
					}
				}
			}*/
		}

		public override void OnAgentBuild(Agent agent, Banner banner)
		{
			MetaMesh frontHair = null, backHair = null;
			uint hairColor = 0;
			var agentVisuals = agent.AgentVisuals;
			if (agentVisuals == null || agent.Character == null || 
			    false == (agent.Character.GetName()?.ToString() != null))
			{
				return;
			}
			if (agent.Character.GetName().ToString().ToLower().Contains("rabbit"))
			{
				agentVisuals.GetHeadMetaMeshOfVisuals(out var head,
					out _, out _, out _, true);
				var num = head.MeshCount;
				for (int i = 0; i < num; i++)
				{
					var mesh = head.GetMeshAtIndex(i);
					if (mesh.HasTag("eye"))
					{
						mesh.Color = 0xffa12c24;
					}
				}

				switch (MBRandom.DeterministicRandomInt(5))
				{
					case 0:
						frontHair = MetaMesh.GetCopy("tha_hair_front_b");
						backHair = MetaMesh.GetCopy("tha_hair_back_b");
						break;
					case 1:
						frontHair = MetaMesh.GetCopy("tha_hair_front_d");
						backHair = MetaMesh.GetCopy("tha_hair_back_d");
						break;
					default:
						frontHair = MetaMesh.GetCopy("tha_hair_front_e");
						backHair = MetaMesh.GetCopy("tha_hair_back_d");
						break;
				}

				switch (MBRandom.DeterministicRandomInt(10))
				{
					default:
						hairColor = 0xff131313;
						break;
					case 2:
					case 3:
						hairColor = 0xff1b1b1b;
						break;
					case 4:
					case 5:
						hairColor = 0xff21201a;
						break;
					case 6:
						hairColor = 0xff391f21;
						break;
					case 7:
						hairColor = 0xff5d2c28;
						break;
					case 8:
						hairColor = 0xffb4b4b4;
						break;
					case 9:
						hairColor = 0xff272727;
						break;
				}
				frontHair.SetFactor1Linear(hairColor);
				backHair.SetFactor1Linear(hairColor);
				agentVisuals.AddMultiMesh(frontHair, BodyMeshTypes.Head);
				agentVisuals.AddMultiMesh(backHair, BodyMeshTypes.Head);
				frontHair.ManualInvalidate();
				backHair.ManualInvalidate();

				MetaMesh earMesh = null;
				switch (MBRandom.DeterministicRandomInt(3))
				{
					case 0:
						earMesh = MetaMesh.GetCopy("tha_ear_rabbit_b");
						break;
					case 1:
						earMesh = MetaMesh.GetCopy("tha_ear_rabbit_c");
						break;
					default:
						earMesh = MetaMesh.GetCopy("tha_ear_rabbit_a");
						break;
				}

				/*if (MBRandom.DeterministicRandomInt(20) == 0)
				{
					earMesh.ManualInvalidate();
					earMesh = MetaMesh.GetCopy("tha_ear_rabbit_d");
				}*/
				agentVisuals.AddMultiMesh(earMesh, BodyMeshTypes.Head);
				earMesh.ManualInvalidate();
			}
			else if (agent.Character.GetName().ToString().ToLower().Contains("kappa"))
			{
				agentVisuals.GetHeadMetaMeshOfVisuals(out var head,
					out _, out _, out _, true);
				var num = head.MeshCount;
				for (int i = 0; i < num; i++)
				{
					var mesh = head.GetMeshAtIndex(i);
					if (mesh.HasTag("eye"))
					{
						switch (MBRandom.DeterministicRandomInt(3))
						{
							case 0:
								mesh.Color = 0xff3c49b1;
								break;
							case 1:
								mesh.Color = 0xfffbc921;
								break;
							default:
								mesh.Color = 0xff00999c;
								break;
						}
					}
				}

				switch (MBRandom.DeterministicRandomInt(4))
				{
					case 0:
						frontHair = MetaMesh.GetCopy("tha_hair_front_a");
						break;
					case 1:
						frontHair = MetaMesh.GetCopy("tha_hair_front_b");
						break;
					case 2:
						frontHair = MetaMesh.GetCopy("tha_hair_front_c");
						break;
					default:
						frontHair = MetaMesh.GetCopy("tha_hair_front_d");
						break;
				}

				switch (MBRandom.DeterministicRandomInt(3))
				{
					case 0:
						backHair = MetaMesh.GetCopy("tha_hair_back_a");
						break;
					case 1:
						backHair = MetaMesh.GetCopy("tha_hair_back_b");
						break;
					default:
						backHair = MetaMesh.GetCopy("tha_hair_back_c");
						break;
				}

				switch (MBRandom.DeterministicRandomInt(10))
				{
					default:
						hairColor = 0xffb2e7ff;
						break;
					case 2:
						hairColor = 0xff03193f; // purple
						break;
					case 3:
						hairColor = 0xffafd1ed;
						break;
					case 4:
						hairColor = 0xff7a09fa; // purple
						break;
					case 5:
						hairColor = 0xff657392;
						break;
					case 6:
						hairColor = 0xff6f86e1;
						break;
					case 7:
						hairColor = 0xff1781cd;
						break;
					case 8:
						hairColor = 0xff0098dc;
						break;
					case 9:
						hairColor = 0xff3003d9;
						break;
				}
				frontHair.SetFactor1Linear(hairColor);
				backHair.SetFactor1Linear(hairColor);
				agentVisuals.AddMultiMesh(frontHair, BodyMeshTypes.Head);
				agentVisuals.AddMultiMesh(backHair, BodyMeshTypes.Head);
				frontHair.ManualInvalidate();
				backHair.ManualInvalidate();
			}
		}
	}
}