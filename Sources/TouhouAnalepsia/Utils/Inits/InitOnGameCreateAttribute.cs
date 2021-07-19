using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace TouhouAnalepsia.Utils.Inits
{
	[AttributeUsage(AttributeTargets.Method)]
	public class InitOnGameCreateAttribute : AbstractInitAttribute
	{
		[HarmonyPatch(typeof(TaleWorlds.Core.Game))]
		static class GamePatch
		{
			[HarmonyPatch("CreateGame", MethodType.Normal)]
			[HarmonyPostfix]
			static void PatchCreateGame()
			{
				InitManager.OnGameCreate(false);
			}

			[HarmonyPatch("LoadSaveGame", MethodType.Normal)]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> PatchLoadSaveGame(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					yield return code;
					if (code.opcode == OpCodes.Call && (code.operand as MethodInfo)?.Name == "RegisterTypes")
					{
						yield return new CodeInstruction(OpCodes.Ldc_I4_1);
						yield return new CodeInstruction(OpCodes.Call, 
							AccessTools.Method(typeof(InitManager), nameof(InitManager.OnGameCreate)));
					}
				}
			}
		}
	}
}