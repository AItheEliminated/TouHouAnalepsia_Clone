using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TouhouAnalepsia.Utils;
using TouhouAnalepsia.Utils.Inits;

namespace TouhouAnalepsia.Items.Scripts
{
	public abstract class AbstractItemScript : SynchedMissionObject
	{
		private static readonly Dictionary<string, ItemScriptParam> inQueueScripts = 
							new Dictionary<string, ItemScriptParam>();

		protected bool IsInited { private set; get; } = false;

		protected sealed override void OnInit()
		{
			base.OnInit();
			var guid = GameEntity.GetGuid();
			if (inQueueScripts.TryGetValue(guid, out var param))
			{
				inQueueScripts.Remove(guid);
				if (param.OwnerAgent != null && param.OwnerAgent.TryGetTarget(out var agent))
				{
					IsInited = true;
					OnInit(agent, param);
				}
				else if (param.OwnerAgentVisuals != null && 
				         param.OwnerAgentVisuals.TryGetTarget(out var agentVisuals))
				{
					IsInited = true;
					OnInit(agentVisuals, param);
				}
				else
				{
					Logger.Log($"Bad param with guid '{guid}' for entity {GameEntity.Name}");
				}
			}
			else
			{
				Logger.Log($"Cannot find param with guid '{guid}' for entity {GameEntity.Name}");
			}
		}

		public virtual void OnInit(Agent agent, ItemScriptParam param)
		{
		}

		public virtual void OnInit(AgentVisuals agentVisuals, ItemScriptParam param)
		{
		}

		public static void AddParamAndScript(Agent agent, string scriptName, ItemScriptParam param)
		{
			param.OwnerAgent = new WeakReference<Agent>(agent);
			param.OwnerAgentVisuals = null;
			var guid = agent.AgentVisuals.GetEntity().GetGuid();
			if (inQueueScripts.ContainsKey(guid))
			{
				Logger.Log($"Duplicated item script param guid '{guid}'");
			}
			inQueueScripts[guid] = param;
			agent.AgentVisuals.GetEntity().CreateAndAddScriptComponent(scriptName);
		}

		public static void AddParamAndScript(AgentVisuals agentVisuals, string scriptName, ItemScriptParam param)
		{
			param.OwnerAgent = null;
			param.OwnerAgentVisuals = new WeakReference<AgentVisuals>(agentVisuals);
			var guid = agentVisuals.GetEntity().GetGuid();
			if (inQueueScripts.ContainsKey(guid))
			{
				Logger.Log($"Duplicated item script param guid '{guid}'");
			}
			inQueueScripts[guid] = param;
			agentVisuals.GetEntity().CreateAndAddScriptComponent(scriptName);
		}

		[InitOnGameCreate]
		public static void Clear()
		{
			inQueueScripts.Clear();
		}

		public class ItemScriptParam
		{
			private static readonly string[] emptyParams = new string[0];

			[CanBeNull]
			public WeakReference<Agent> OwnerAgent { set; get; }

			[CanBeNull]
			public WeakReference<AgentVisuals> OwnerAgentVisuals { set; get; }

			[NotNull]
			public string[] UserParams { set; get; } = emptyParams;

			public void SplitAndSetParam(string param)
			{
				if (!string.IsNullOrWhiteSpace(param))
				{
					UserParams = param.Split(';');
				}
			}
		}
	}
}