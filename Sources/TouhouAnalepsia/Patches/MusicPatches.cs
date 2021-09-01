using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Patches {
	class MusicPatches {
		public static void Patch() {
			HarmonyHandler.patchTranspiler("psai.net.Logik", "LoadSoundtrackFromProjectFile", "TouhouAnalepsia.Patches.MusicPatches", "MusicPatch");
			MBMusicManager.Create(); //This potentially may cause a memory leak, ideally in the future we can get rid of this, however at the present moment this method is run after music is loaded into the game.
		}
		static IEnumerable<CodeInstruction> MusicPatch(IEnumerable<CodeInstruction> instructions) {
			foreach (CodeInstruction instruction in instructions) {
				if (instruction.opcode.Equals(OpCodes.Ldarg_1)) {
					instruction.opcode = OpCodes.Ldstr;
					instruction.operand = ModuleHelper.GetModuleFullPath("TouhouAnalepsia") + "music/soundtrack.xml";
				}
				yield return instruction;
			}
		}
	}
}
