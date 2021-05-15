using System;
using System.Reflection;
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

		/*
		public static void patchTranspiler(string locationOriginal, string methodOriginal, string locationNew, string methodNew) {
			Type to = AccessTools.TypeByName(locationOriginal);
			MethodInfo mOriginal = AccessTools.Method(to, methodOriginal);

			Type tn = AccessTools.TypeByName(locationNew);
			MethodInfo mNew = AccessTools.Method(tn, methodNew);

			harmony.Patch(mOriginal, null, null, new HarmonyMethod(mNew));
		}
		*/
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
	}
}
