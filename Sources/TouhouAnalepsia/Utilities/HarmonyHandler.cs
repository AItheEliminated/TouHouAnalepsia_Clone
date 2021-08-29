using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
namespace TouhouAnalepsia {
	class HarmonyHandler {
		protected static Harmony harmony { get; private set; } = new Harmony("org.hinageshi.touhouanalepsia");

		/* Sample Harmony program
		
		HarmonyHandler.patchPrefix("TaleWorlds.MountAndBlade.MissionBehaviour","OnAgentBuild","TouhouAnalepsia.HarmonyHandler","MyPrefix");
		
		public static bool MyPrefix() {
			Logger.Log("Bah!");
			return true;
		}
		 */

		public static void patchPrefix(string locationOriginal, string methodOriginal, string locationNew, string methodNew) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			Type tn = AccessTools.TypeByName(locationNew);
			MethodInfo mNew = AccessTools.Method(tn, methodNew);

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}
			if (mNew == null) {
				Logger.Log(tn + "." + mNew + " null!");
				return;
			}

			harmony.Patch(mOriginal, new HarmonyMethod(mNew)) ;
		}
		public static void patchPostfix(string locationOriginal, string methodOriginal, string locationNew, string methodNew) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			Type tn = AccessTools.TypeByName(locationNew);
			MethodInfo mNew = AccessTools.Method(tn, methodNew);

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}
			if (mNew == null) {
				Logger.Log(tn + "." + mNew + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, new HarmonyMethod(mNew));
		}
		public static void PatchAll() {
			harmony.PatchAll();
		}

		public static void patchTranspiler(string locationOriginal, string methodOriginal, string locationNew, string methodNew) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			Type tn = AccessTools.TypeByName(locationNew);
			MethodInfo mNew = AccessTools.Method(tn, methodNew);

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}
			if (mNew == null) {
				Logger.Log(tn + "." + mNew + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		public static void patchFinalizer(string locationOriginal, string methodOriginal, string locationNew, string methodNew) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			Type tn = AccessTools.TypeByName(locationNew);
			MethodInfo mNew = AccessTools.Method(tn, methodNew);

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}
			if (mNew == null) {
				Logger.Log(tn + "." + mNew + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, null, new HarmonyMethod(mNew));
		}

		//Default Methods
		public static void patchReturn(string locationOriginal, string methodOriginal) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);
			
			MethodInfo mNew = AccessTools.Method(typeof(HarmonyHandler), "Return");

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		static IEnumerable<CodeInstruction> Return(IEnumerable<CodeInstruction> instructions) {
			yield return new CodeInstruction(OpCodes.Ret);
		}
		public static void patchReturnNull(string locationOriginal, string methodOriginal) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			MethodInfo mNew = AccessTools.Method(typeof(HarmonyHandler), "ReturnNull");

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		static IEnumerable<CodeInstruction> ReturnNull(IEnumerable<CodeInstruction> instructions) {
			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Ret);
		}
		public static void patchReturnTrue(string locationOriginal, string methodOriginal) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			MethodInfo mNew = AccessTools.Method(typeof(HarmonyHandler), "ReturnTrue");

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		static IEnumerable<CodeInstruction> ReturnTrue(IEnumerable<CodeInstruction> instructions) {
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Ret);
		}
		public static void patchReturnFalse(string locationOriginal, string methodOriginal) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			MethodInfo mNew = AccessTools.Method(typeof(HarmonyHandler), "ReturnFalse");

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		static IEnumerable<CodeInstruction> ReturnFalse(IEnumerable<CodeInstruction> instructions) {
			yield return new CodeInstruction(OpCodes.Ldc_I4_0);
			yield return new CodeInstruction(OpCodes.Ret);
		}
		private static string str_patchReturnString;
		public static void patchReturnString(string locationOriginal, string methodOriginal, string value) {
			str_patchReturnString = value;

			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			MethodInfo mNew = AccessTools.Method(typeof(HarmonyHandler), "ReturnString");

			if (mOriginal == null) {
				Logger.Log(to + "." + mOriginal + " null!");
				return;
			}

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		static IEnumerable<CodeInstruction> ReturnString(IEnumerable<CodeInstruction> instructions) {
			yield return new CodeInstruction(OpCodes.Ldstr, str_patchReturnString);
			yield return new CodeInstruction(OpCodes.Ret);
		}
	}
}
