using HarmonyLib;
using SandBox;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using System.Reflection;
using System.IO;
using TaleWorlds.Core;
using System;
using TaleWorlds.ModuleManager;

namespace Bannerlord.GeneralModdingPatches {
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

			if (File.Exists(ModulePath + "GeneralModdingPatches_Config.txt")) {
				string[] lines = File.ReadAllLines(ModulePath + "GeneralModdingPatches_Config.txt");
				new_map_name = lines[0];
			} else {
				Debug.Print("There does not exist a config for GeneralModdingPatches at: " + ModulePath + "GeneralModdingPatches_Config.txt" + "\nDefaulting to modded_main_map.");
			}

			Harmony harmony = new Harmony("GeneralModdingPatches");

			Type to = AccessTools.TypeByName("psai.net.Logik");
			MethodInfo mOriginal = to.GetMethod("LoadSoundtrackFromProjectFile", BindingFlags.Instance | BindingFlags.NonPublic);

			Type tn = AccessTools.TypeByName("Bannerlord.GeneralModdingPatches.SubModule");
			MethodInfo mNew = AccessTools.Method(tn, "MusicPatch");

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
			MBMusicManager.Create(); //This potentially may cause a memory leak, ideally in the future we can get rid of this, however at the present moment this method is run after music is loaded into the game.

			harmony.PatchAll();
		}
		static IEnumerable<CodeInstruction> MusicPatch(IEnumerable<CodeInstruction> instructions) {
			foreach (CodeInstruction instruction in instructions) {
				if(instruction.opcode.Equals(OpCodes.Ldarg_1)) {
					instruction.opcode = OpCodes.Ldstr;
					instruction.operand = ModulePath + "music/soundtrack.xml";
				}
				yield return instruction;
			}
		}

		[HarmonyPatch(typeof(MapScene), "Load")]
		public static class MainScenePatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				bool flag = false;
				bool flip = true;
				int counter = 0;
				foreach (CodeInstruction instruction in instructions) {
					if (flag) counter++;
					if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("Main_map")) {
						instruction.operand = new_map_name;
					}
					if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("reading map scene")) {
						flag = false;
					}
					if (instruction.opcode == OpCodes.Ldc_I4 && instruction.OperandIs(4096)) {
						flag = true;
					}
					if(flag && counter>=10) {
						if(instruction.opcode == OpCodes.Ldc_I4_0) {
							if (flip) instruction.opcode = OpCodes.Ldc_I4_1;

							flip = !flip;
						}
					}
					yield return instruction;
				}
			}
		}
	}
}
