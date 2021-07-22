using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;

namespace TouhouAnalepsia.BattleSystem.Models
{
	public class TouhouAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
	{
		public override float CalculateDamage(ref AttackInformation attackInformation, ref AttackCollisionData collisionData,
			in MissionWeapon weapon, float baseDamage)
		{
			var damage = base.CalculateDamage(ref attackInformation, ref collisionData, in weapon, baseDamage);
			return damage;
		}

		private static IEnumerable<CodeInstruction> PatchDo(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var code in instructions)
			{
				if (code.opcode == OpCodes.Newobj &&
				    (code.operand as ConstructorInfo)?.DeclaringType == typeof(MultiplayerAgentApplyDamageModel))
				{
					yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(TouhouAgentApplyDamageModel)));
				}
				else
				{
					yield return code;
				}
			}
		}

		[HarmonyPatch(typeof(CustomGame))]
		static class CustomGamePatch
		{
			[HarmonyPatch("InitializeGameModels", MethodType.Normal)]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				return PatchDo(instructions);
			}
		}

		[HarmonyPatch(typeof(EditorGame))]
		static class EditorGamePatch
		{
			[HarmonyPatch("InitializeGameModels", MethodType.Normal)]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				return PatchDo(instructions);
			}
		}
	}
}