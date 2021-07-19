using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Visuals
{
	public class AgentVisualsAuxBehavior : MissionLogic
	{
		private List<Agent> _delayInitAgents = new List<Agent>();

		public override void OnAgentBuild(Agent agent, Banner banner)
		{
			if (agent.IsHuman)
			{
				_delayInitAgents.Add(agent);
			}
		}

		public override void OnMissionTick(float dt)
		{
			if (_delayInitAgents.Count > 0)
			{
				foreach (var agent in _delayInitAgents)
				{
					if (!agent.IsActive())
						continue;
					agent.AgentVisuals.GetHeadMetaMeshOfVisuals(out var head,
						out _, out _, out _, true);
					if (head == null)
						continue;
					BodyMeshCache.FixupHeadMesh(head, agent.BodyPropertiesValue);
				}

				_delayInitAgents.Clear();
			}

			if (true)
				return;
			foreach (var agent in Mission.Agents)
			{
				if (agent.IsHuman && agent.IsActive())
				{
					var skeleton = agent.AgentVisuals.GetSkeleton();
					var boneNames = new string[skeleton.GetBoneCount()];
					var localRestFrames = new MatrixFrame[skeleton.GetBoneCount()];
					var EntitialRestFrames = new MatrixFrame[skeleton.GetBoneCount()];
					var EntitialFrames = new MatrixFrame[skeleton.GetBoneCount()];
					for (int i = 0, j = skeleton.GetBoneCount(); i < j; i++)
					{
						boneNames[i] = skeleton.GetBoneName((byte)i);
						localRestFrames[i] = skeleton.GetBoneLocalRestFrame((sbyte)i);
						EntitialRestFrames[i] = skeleton.GetBoneEntitialRestFrame((sbyte)i);
						EntitialFrames[i] = skeleton.GetBoneEntitialFrame((sbyte)i);
					}

					//28
					for (int i = 28, j = skeleton.GetBoneCount(); i < j; i++)
					{
						var rotMat = Mat3.Identity;
						/*rotMat.RotateAboutForward(0.78f);
						rotMat.RotateAboutSide(-0.78f);
						rotMat.RotateAboutUp(0.78f);*/
						var mat = new MatrixFrame(rotMat, Vec3.Zero);
						skeleton.SetBoneLocalFrame(i, mat);
					}
					skeleton.SetBoneLocalFrame(0, EntitialFrames[0]);
					skeleton.UpdateEntitialFramesFromLocalFrames();

					for (int i = 0, j = skeleton.GetBoneCount(); i < j; i++)
					{
						boneNames[i] = skeleton.GetBoneName((byte)i);
						localRestFrames[i] = skeleton.GetBoneLocalRestFrame((sbyte)i);
						EntitialRestFrames[i] = skeleton.GetBoneEntitialRestFrame((sbyte)i);
						EntitialFrames[i] = skeleton.GetBoneEntitialFrame((sbyte)i);
					}

					//Logger.Log(EntitialFrames[0].origin.ToString());
				}
			}
		}
	}
}