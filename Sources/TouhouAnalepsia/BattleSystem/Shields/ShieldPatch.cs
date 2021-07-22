using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace TouhouAnalepsia.BattleSystem.Shields
{
	public static class ShieldPatch
	{
		private static MethodInfo methodCheckShieldHit = AccessTools.Method(
			typeof(ShieldBehavior), nameof(ShieldBehavior.CheckShieldHit));

		[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.Mission))]
		[HarmonyPatch("MissileHitCallback")]
		static class MissileHitCallbackPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				var labelIfShield = generator.DefineLabel();
				var labelIfNoShield = generator.DefineLabel();
				var isColliderAgentHitCount = 0;
				var missileHasPhysicsHitCount = 0;
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Call &&
					    (code.operand as MethodInfo)?.Name.Contains("IsColliderAgent") == true &&
					    (++isColliderAgentHitCount) == 2)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldarg_0); // mission
						yield return new CodeInstruction(OpCodes.Ldarg_S, 10); // attacker
						yield return new CodeInstruction(OpCodes.Ldarg_S, 11); // defender
						yield return new CodeInstruction(OpCodes.Ldloc_0); // missile
						yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // missilePosition
						yield return new CodeInstruction(OpCodes.Ldarg_S, 5); // missileAngularVelocity
						yield return new CodeInstruction(OpCodes.Ldarg_S, 6); // movementVelocity
						yield return new CodeInstruction(OpCodes.Ldloca_S, 24); // blow
						yield return new CodeInstruction(OpCodes.Call, methodCheckShieldHit);
						yield return new CodeInstruction(OpCodes.Brfalse_S, labelIfNoShield);
						yield return new CodeInstruction(OpCodes.Ldc_I4_3); // missileCollisionReaction = BecomeInvisible
						yield return new CodeInstruction(OpCodes.Stloc_S, 6);
						yield return new CodeInstruction(OpCodes.Ldarg_1); // hitParticleIndex = 0
						yield return new CodeInstruction(OpCodes.Ldc_I4_0);
						yield return new CodeInstruction(OpCodes.Stind_I4);
						yield return new CodeInstruction(OpCodes.Br_S, labelIfShield); // jump out
						yield return Patches.CreateCodeInstructionWithLabel(OpCodes.Ldarg_2, null, labelIfNoShield);
						yield return code;
					}
					else if (code.opcode == OpCodes.Call &&
					         (code.operand as MethodInfo)?.Name.Contains("MissileHasPhysics") == true &&
					         (++missileHasPhysicsHitCount) == 2)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return Patches.CreateCodeInstructionWithLabel(OpCodes.Ldarg_2, null, labelIfShield);
						yield return code;
					}
					else
						yield return code;
				}
			}
		}
	}
}