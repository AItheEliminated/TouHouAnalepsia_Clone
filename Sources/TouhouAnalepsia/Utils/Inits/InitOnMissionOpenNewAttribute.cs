using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace TouhouAnalepsia.Utils.Inits
{
	[AttributeUsage(AttributeTargets.Method)]
	public class InitOnMissionOpenNewAttribute : AbstractInitAttribute
	{
		[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.MissionState))]
		static class MissionStatePatch
		{
			[HarmonyPatch("HandleOpenNew", MethodType.Normal)]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> PatchHandleOpenNew(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Callvirt && 
					    (code.operand as MethodInfo)?.Name == "GetEnumerator")
					{
						yield return new CodeInstruction(OpCodes.Call,
							AccessTools.Method(typeof(InitManager), nameof(InitManager.OnMissionOpenNew)));
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Stloc_0);
					}
					yield return code;
				}
			}
		}
	}
}