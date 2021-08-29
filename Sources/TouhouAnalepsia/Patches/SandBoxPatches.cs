using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using static TouhouAnalepsia.HarmonyHandler;

namespace TouhouAnalepsia.Patches {
	class SandBoxPatches {
		public static void Patch() {

		}
		[HarmonyPatch(typeof(SandBoxManager), "Initialize", new Type[] { typeof(CampaignGameStarter) })]
		class DebugPatch29 {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				bool skip=false;
				foreach (CodeInstruction instruction in instructions) {
					if (instruction.opcode.Equals(OpCodes.Newobj)) {
						skip = false;
						if (((ConstructorInfo)instruction.operand).DeclaringType.Equals(typeof(BackstoryCampaignBehavior))) {
							skip = true;
							continue;
						}
					}
					if (skip && instruction.opcode.Equals(OpCodes.Ret)) {
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ret);
						yield break;
					}
					if (!skip) yield return instruction;
				}
			}
		}
	}
}
