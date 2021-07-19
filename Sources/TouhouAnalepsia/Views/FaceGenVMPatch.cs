using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace TouhouAnalepsia.Views
{
	public class FaceGenVMPatch : FaceGenVM
	{
		private FaceGenPropertyVM _chestSlider;

		private Dictionary<FaceGenVM.FaceGenTabs, MBBindingList<FaceGenPropertyVM>> _tabs;

		public FaceGenVMPatch(BodyGenerator bodyGenerator, IFaceGeneratorHandler faceGeneratorScreen,
			Action<float> onHeightChanged, Action onAgeChanged, TextObject affirmitiveText, TextObject negativeText,
			int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex, bool canChangeGender,
			bool openedFromMultiplayer, IFaceGeneratorCustomFilter filter) : base(bodyGenerator, faceGeneratorScreen,
			onHeightChanged, onAgeChanged, affirmitiveText, negativeText, currentStageIndex, totalStagesCount,
			furthestIndex, goToIndex, canChangeGender, openedFromMultiplayer, filter)
		{
			_tabs = AccessTools.Field(typeof(FaceGenVM), "_tabProperties").GetValue(this) as Dictionary<FaceGenVM.FaceGenTabs, MBBindingList<FaceGenPropertyVM>>;
		}

		public void RefreshExt()
		{
			//_chestSlider = new FaceGenPropertyVM(20, 0f, 1f, new TextObject("Chest:", null), keyTimePoint, 0, (double)((this._heightSlider == null) ? this._faceGenerationParams._heightMultiplier : this._heightSlider.Value), new Action<int, float, bool, bool>(this.UpdateFace), new Action(this.AddCommand), new Action(this.ResetSliderPrevValues), true, false);
			_tabs[FaceGenVM.FaceGenTabs.Body].Add(_chestSlider);
		}

		/*[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.GauntletUI.BodyGeneratorView))]
		[HarmonyPatch(MethodType.Constructor,
			typeof(ControlCharacterCreationStage),
			typeof(TextObject),
			typeof(ControlCharacterCreationStage),
			typeof(TextObject),
			typeof(BasicCharacterObject),
			typeof(bool),
			typeof(IFaceGeneratorCustomFilter),
			typeof(ControlCharacterCreationStageReturnInt),
			typeof(ControlCharacterCreationStageReturnInt),
			typeof(ControlCharacterCreationStageReturnInt),
			typeof(ControlCharacterCreationStageWithInt))]*/
		static class BodyGeneratorViewPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Newobj &&
					    (code.operand as ConstructorInfo)?.DeclaringType == typeof(FaceGenVM))
					{
						yield return new CodeInstruction(OpCodes.Newobj, AccessTools.FirstConstructor(typeof(FaceGenVMPatch), info => true));
					}
					else
					{
						yield return code;
					}
				}
			}
		}

		//[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.ViewModelCollection.FaceGenVM))]
		//[HarmonyPatch("Refresh")]
		static class RefreshPatch
		{
			// insert a ((FaceGenVMPatch) this).RefreshExt() before this.UpdateGenderBasedResources()
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					if (code.opcode == OpCodes.Call &&
						(code.operand as MethodInfo)?.FullDescription().Contains("UpdateGenderBasedResources") == true)
					{
						yield return new CodeInstruction(OpCodes.Castclass, typeof(FaceGenVMPatch));
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FaceGenVMPatch), nameof(RefreshExt)));
						yield return new CodeInstruction(OpCodes.Ldarg_0);
					}
					yield return code;
				}
			}
		}

		public enum NewFaceGenTabs
		{
			None = -1,
			Body = 0,
			Face = 1,
			Eyes = 2,
			Hair = 3,
			Hands = 4,
			Underwears = 5,
			Parts = 6,
			NumOfFaceGenTabs = 7
		}
	}
}
