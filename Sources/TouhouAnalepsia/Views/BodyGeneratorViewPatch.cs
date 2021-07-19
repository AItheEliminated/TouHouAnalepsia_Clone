using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace TouhouAnalepsia.Views
{
	public static class BodyGeneratorViewPatch
	{
		private static FieldInfo theInstance = AccessTools.Field(typeof(TouhouBodyGeneratorView), nameof(TouhouBodyGeneratorView.Instance));

		private static ConstructorInfo theConstructor = AccessTools.FirstConstructor(typeof(TouhouBodyGeneratorView), info => true);

		private static MethodInfo onTickMethod = AccessTools.Method(typeof(TouhouBodyGeneratorView), nameof(TouhouBodyGeneratorView.OnTick));

		private static MethodInfo onFinalizeMethod = AccessTools.Method(typeof(TouhouBodyGeneratorView), nameof(TouhouBodyGeneratorView.OnFinalize));

		private static MethodInfo getSceneLayer = AccessTools.PropertyGetter(typeof(TouhouBodyGeneratorView), nameof(TouhouBodyGeneratorView.SceneLayer));

		private static MethodInfo getGauntletLayer = AccessTools.PropertyGetter(typeof(TouhouBodyGeneratorView), nameof(TouhouBodyGeneratorView.GauntletLayer));

		//[HarmonyPatch(typeof(SandBox.GauntletUI.GauntletBarberScreen))]
		static class GauntletBarberScreenPatch
		{
			/*[HarmonyPatch("Handler", MethodType.Getter)]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchHandler(IEnumerable<CodeInstruction> instructions)
			{
				// replace "return this._facegenLayer" with "return TouhouBodyGeneratorView.Instance"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldfld)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldsfld, theInstance);
					}
					else
					{
						yield return code;
					}
				}
			}

			/*[HarmonyPatch(MethodType.Constructor, typeof(BarberState))]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchConstructor(IEnumerable<CodeInstruction> instructions)
			{
				// replace "this._facegenLayer = new BodyGeneratorView()" with "TouhouBodyGeneratorView.Instance = new TouhouBodyGeneratorView()"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Newobj && 
					    (code.operand as ConstructorInfo)?.DeclaringType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Newobj, theConstructor);
					}
					else if (code.opcode == OpCodes.Stfld &&
					         (code.operand as FieldInfo)?.FieldType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Stsfld, theInstance);
						yield return new CodeInstruction(OpCodes.Pop);
					}
					else
					{
						yield return code;
					}
				}
			}

			/*[HarmonyPatch("OnFrameTick")]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchOnFrameTick(IEnumerable<CodeInstruction> instructions)
			{
				// replace "this._facegenLayer.OnTick()" with "TouhouBodyGeneratorView.Instance.OnTick()"
				// replace "this._facegenLayer.SceneLayer" with "TouhouBodyGeneratorView.Instance.SceneLayer"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldfld &&
					    (code.operand as FieldInfo)?.FieldType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldsfld, theInstance);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "OnTick")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, onTickMethod);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "get_SceneLayer")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, getSceneLayer);
					}
					else
					{
						yield return code;
					}
				}
			}

			/*[HarmonyPatch("OnExit")]
			[HarmonyPostfix]*/
			static void PatchOnExit()
			{
				TouhouBodyGeneratorView.Instance = null;
			}

			/*[HarmonyPatch("OnInitialize")]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchOnInitialize(IEnumerable<CodeInstruction> instructions)
			{
				// replace "this._facegenLayer.GauntletLayer" with "TouhouBodyGeneratorView.Instance.GauntletLayer"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldfld &&
					    (code.operand as FieldInfo)?.FieldType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldsfld, theInstance);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "get_GauntletLayer")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, getGauntletLayer);
					}
					else
					{
						yield return code;
					}
				}
			}

			/*[HarmonyPatch("OnActivate")]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchOnActivate(IEnumerable<CodeInstruction> instructions)
			{
				// replace "this._facegenLayer.SceneLayer" with "TouhouBodyGeneratorView.Instance.SceneLayer"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldfld &&
					    (code.operand as FieldInfo)?.FieldType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldsfld, theInstance);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "get_SceneLayer")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, getSceneLayer);
					}
					else
					{
						yield return code;
					}
				}
			}

			/*[HarmonyPatch("OnDeactivate")]
			[HarmonyTranspiler]*/
			static IEnumerable<CodeInstruction> PatchOnDeactivate(IEnumerable<CodeInstruction> instructions)
			{
				// replace "this._facegenLayer.SceneLayer" with "TouhouBodyGeneratorView.Instance.SceneLayer"
				// replace "this._facegenLayer.OnFinalize()" with "TouhouBodyGeneratorView.Instance.OnFinalize()"
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Ldfld &&
					    (code.operand as FieldInfo)?.FieldType == typeof(BodyGeneratorView))
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldsfld, theInstance);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "get_SceneLayer")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, getSceneLayer);
					}
					else if (code.opcode == OpCodes.Callvirt &&
					         (code.operand as MethodInfo)?.Name == "OnFinalize")
					{
						yield return new CodeInstruction(OpCodes.Callvirt, onFinalizeMethod);
					}
					else
					{
						yield return code;
					}
				}
			}
		}
	}
}
