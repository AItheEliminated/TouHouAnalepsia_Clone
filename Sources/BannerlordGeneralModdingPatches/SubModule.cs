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

		private static bool music_patch = false;

		public override void OnGameInitializationFinished(Game game) {
			base.OnGameInitializationFinished(game);
			if (File.Exists(ModulePath + "ModuleData" + Path.DirectorySeparatorChar + "managed_modded_parameters.xml")) {
				ManagedParameters.Instance.Initialize(ModulePath + "ModuleData" + Path.DirectorySeparatorChar + "managed_modded_parameters.xml");
			}
		}

		protected override void OnSubModuleLoad() {
			base.OnSubModuleLoad();
			
			if (File.Exists(ModulePath + "GeneralModdingPatches_Config.txt")) {
				//Debug.Print(Path.GetFullPath(MainMapFixConfig + "MainMapFix_Config.txt"));
				string[] lines = File.ReadAllLines(ModulePath + "GeneralModdingPatches_Config.txt");
				new_map_name = lines[0];
			} else {
				Debug.Print("There does not exist a config for GeneralModdingPatches at: " + ModulePath + "GeneralModdingPatches_Config.txt" + "\nDefaulting to modded_main_map.");
			}


			if(File.Exists(ModulePath + "music" + Path.DirectorySeparatorChar + "soundtrack.xml")) {
				music_patch = true;
			}

			Harmony.DEBUG = true;
			Harmony harmony = new Harmony("GeneralModdingPatches");
			
			Type to = AccessTools.TypeByName("psai.net.Logik");
			MethodInfo mOriginal = to.GetMethod("LoadSoundtrackFromProjectFile", BindingFlags.Instance | BindingFlags.NonPublic);
			
			Type tn = AccessTools.TypeByName("Bannerlord.GeneralModdingPatches.SubModule");
			MethodInfo mNew = AccessTools.Method(tn, "MusicPatch");

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
			Harmony.GetPatchInfo(mOriginal);
			
			harmony.PatchAll();
		}

		/*
		static bool MusicPatch(ref string pathToProjectFile) {
			pathToProjectFile = BasePath.Name + "Modules/TouhouAnalepsia/" + "music" + Path.DirectorySeparatorChar + "soundtrack.xml";
			Debug.Print("ABCDEFG");
			return true;
		}
		*/
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

		/*
		[HarmonyPatch(typeof(MBMusicManager), MethodType.Constructor)]
		public static class MusicPatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				bool flag = false;
				foreach (CodeInstruction instruction in instructions) {
					if (!music_patch) yield return instruction;

					if(flag) {
						yield return new CodeInstruction(OpCodes.Ldstr, ModulePath + "music" + Path.DirectorySeparatorChar + "soundtrack.xml");
						flag = false;
						continue;
					}

					if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("music/soundtrack.xml")) {
						flag = true;
					}
					yield return instruction;
				}
			}
		}

		[HarmonyPatch(typeof(MBMusicManagerOld), "Initialize")]
		public static class MusicPatchOld {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				bool flag = false;
				foreach (CodeInstruction instruction in instructions) {
					if (!music_patch) yield return instruction;

					if (flag) {
						yield return new CodeInstruction(OpCodes.Ldstr, ModulePath + "music" + Path.DirectorySeparatorChar + "soundtrack.xml");
						flag = false;
						continue;
					}

					if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs("music/soundtrack.xml")) {
						flag = true;
					}
					yield return instruction;
				}
			}
		}
		*/
	}
}
