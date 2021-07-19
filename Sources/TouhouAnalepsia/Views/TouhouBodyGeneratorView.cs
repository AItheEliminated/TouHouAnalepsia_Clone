using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TouhouAnalepsia.Views.Models;

namespace TouhouAnalepsia.Views
{
	public class TouhouBodyGeneratorView : IFaceGeneratorHandler, TouhouFaceGenVM.ITouhouFaceGeneratorHandler
	{
		public static TouhouBodyGeneratorView Instance;

		public TextObject AffirmitiveText { set; get; }

		public TextObject NegativeText { set; get; }

		public BodyGenerator BodyGenerator { set; get; }

		public IFaceGeneratorCustomFilter Filter { set; get; }

		public FaceGenVM DataSource { private set; get; }

		public GauntletLayer GauntletLayer { private set; get; }

		public SceneLayer SceneLayer { private set; get; }

		public BodyGenerator BodyGen { private set; get; }

		private IInputContext DebugInput { get; } = Input.DebugInput;

		public TouhouBodyGeneratorView(
			ControlCharacterCreationStage affirmativeAction,
			TextObject affirmativeActionText,
			ControlCharacterCreationStage negativeAction,
			TextObject negativeActionText,
			BasicCharacterObject character,
			bool openedFromMultiplayer,
			IFaceGeneratorCustomFilter filter,
			ControlCharacterCreationStageReturnInt getCurrentStageIndexAction = null,
			ControlCharacterCreationStageReturnInt getTotalStageCountAction = null,
			ControlCharacterCreationStageReturnInt getFurthestIndexAction = null,
			ControlCharacterCreationStageWithInt goToIndexAction = null)
		{
		}

		public void OnTick(float dt)
		{

		}

		public void OnFinalize()
		{

		}

		public void ChangeToBodyCamera()
		{
			throw new NotImplementedException();
		}

		public void ChangeToEyeCamera()
		{
			throw new NotImplementedException();
		}

		public void ChangeToNoseCamera()
		{
			throw new NotImplementedException();
		}

		public void ChangeToMouthCamera()
		{
			throw new NotImplementedException();
		}

		public void ChangeToFaceCamera()
		{
			throw new NotImplementedException();
		}

		public void ChangeToHairCamera()
		{
			throw new NotImplementedException();
		}

		public void RefreshCharacterEntity()
		{
			throw new NotImplementedException();
		}

		public void MakeVoice(int gender, float pitch)
		{
			throw new NotImplementedException();
		}

		public void SetFacialAnimation(string faceAnimation, bool loop)
		{
			throw new NotImplementedException();
		}

		public void Done()
		{
			throw new NotImplementedException();
		}

		public void Cancel()
		{
			throw new NotImplementedException();
		}

		public void UndressCharacterEntity()
		{
			throw new NotImplementedException();
		}

		public void DressCharacterEntity()
		{
			throw new NotImplementedException();
		}

		public void OnHeightChanged(float height)
		{
			throw new NotImplementedException();
		}

		public void OnAgeChanged()
		{
			throw new NotImplementedException();
		}


	}
}
