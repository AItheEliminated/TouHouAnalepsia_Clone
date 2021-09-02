using HarmonyLib;
using SandBox;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using System.Reflection;
using System.IO;
using TaleWorlds.Core;

namespace Aurelian.MapFix {
	public class SubModule : MBSubModuleBase {
		protected static string ModulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8)) + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;

		private static string new_map_name = "modded_main_map";

		public override void OnGameInitializationFinished(Game game) {
			base.OnGameInitializationFinished(game);
			if (File.Exists(ModulePath + "ModuleData" + Path.DirectorySeparatorChar + "managed_modded_parameters.xml")) {
				ManagedParameters.Instance.Initialize(ModulePath + "ModuleData" + Path.DirectorySeparatorChar + "managed_modded_parameters.xml");
			}
		}

		protected override void OnSubModuleLoad() {
			base.OnSubModuleLoad();

			if (File.Exists(ModulePath + "MapFix.txt")) {
				string[] lines = File.ReadAllLines(ModulePath + "MapFix.txt");
				new_map_name = lines[0];
			} else {
				Debug.Print("There does not exist a config for Aurelian's Map Fix at: " + ModulePath + "MapFix.txt" + "\nDefaulting to modded_main_map.");
			}

			new Harmony("Aurelian.MapFix").PatchAll();
		}

		[HarmonyPatch(typeof(MapScene), "Load")]
		public static class MainScenePatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				int truthOccurance = -1;
				bool truthFlag = false;
				foreach (CodeInstruction instruction in instructions) {
					if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("Main_map")) {
						instruction.operand = new_map_name;
					} else if (instruction.opcode == OpCodes.Ldloca_S) {
						truthOccurance++;
						truthFlag = true;
					} else if (instruction.opcode == OpCodes.Stfld) {
						truthFlag = false;
					} else if (instruction.opcode == OpCodes.Ldc_I4_0 && truthFlag && (truthOccurance == 1 || truthOccurance == 3)) {
						instruction.opcode = OpCodes.Ldc_I4_1;
					}
					yield return instruction;
				}
			}
		}
	}
}
