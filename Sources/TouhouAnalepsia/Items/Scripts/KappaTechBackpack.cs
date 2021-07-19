using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Items.Scripts
{
	public class KappaTechBackpack : AbstractItemScript
	{
		private static Random random = new Random();

		private Agent _agent;
		private GameEntity _entity;
		private bool _disable = false;

		public override void OnInit(Agent agent, ItemScriptParam param)
		{
			if (IsInited)
			{
				_agent = agent;
				var gameEntity = GameEntity.CreateEmpty(Scene, false);
				gameEntity.Name = nameof(KappaTechBackpack);
				var skeleton = Skeleton.CreateFromModel("kappa_tech_backpack_skeleton");
				gameEntity.Skeleton = skeleton;
				var mesh = "kappa_tech_backpack_a";
				if (param.UserParams.Length > 0 && !string.IsNullOrWhiteSpace(param.UserParams[0]))
					mesh = param.UserParams[0];
				var metaMesh = MetaMesh.GetCopy(mesh);
				gameEntity.AddMultiMeshToSkeleton(metaMesh);
				skeleton.SetAnimationAtChannel("kappa_backpack_idle", 0, 1f, -1f, (float) random.NextDouble());

				var matrix = MatrixFrame.Identity;
				matrix.origin.x = 0.422f;
				ParticleSystem.CreateParticleSystemAttachedToBone("outdoor_smoke_tiny", skeleton, 1, ref matrix);
				/*if (agent.IsPlayerControlled)
				{
					for (int i = 0; i < metaMesh.MeshCount; i++)
					{
						var mesh = metaMesh.GetMeshAtIndex(i);
						var mat = mesh.GetMaterial();
						mat = mat.CreateCopy();
						mat.AddMaterialShaderFlag("dont_write_motion_vectors", true);
						mesh.SetMaterial(mat);
						mat.ManualInvalidate();
						mesh.ManualInvalidate();
					}
				}*/

				_entity = gameEntity;
				OnTick(0);
				_entity.SetPreviousFrameInvalid();

				metaMesh.ManualInvalidate();
				skeleton.ManualInvalidate();
			}
		}

		public override void OnInit(AgentVisuals agentVisuals, ItemScriptParam param)
		{
			if (IsInited)
			{
				// TODO:
			}
		}

		protected override TickRequirement GetTickRequirement()
		{
			return TickRequirement.Tick;
		}

		protected override void OnTick(float dt)
		{
			if (_agent != null)
			{
				if (_agent.State != AgentState.Deleted)
				{
					var skeleton = _agent.AgentVisuals.GetSkeleton();
					if (skeleton != null)
					{
						MatrixFrame frame = skeleton.GetBoneEntitialFrame(11);
						frame.Rotate(MathF.HalfPI, new Vec3(0, 1, 0));
						frame = _agent.AgentVisuals.GetGlobalFrame().TransformToParent(frame);
						_entity.SetFrame(ref frame);
					}
				}
				else if (_disable)
				{
					_entity.Remove(0);
				}

				if (!_disable && (_agent.State == AgentState.Killed || 
				                  _agent.State == AgentState.Unconscious ||
				                  _agent.State == AgentState.Deleted))
				{
					_disable = true;
					_entity.PauseSkeletonAnimation();
				}
			}
		}

		protected override void OnRemoved(int removeReason)
		{
			base.OnRemoved(removeReason);
		}

		protected override bool MovesEntity()
		{
			return base.MovesEntity();
		}
	}
}