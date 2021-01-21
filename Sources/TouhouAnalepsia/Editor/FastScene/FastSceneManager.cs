using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Editor.FastScene
{
	public class FastSceneManager : MBGameManager
	{
		private Action _afterLoad;

		public FastSceneManager(/*[JetBrains.Annotations.NotNull]*/ Action afterLoad)  //For compatibility reasons I'm commenting out [NotNull] - seems to be causing some issues for others.
		{
			_afterLoad = afterLoad;
		}

		protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
		{
			nextStep = GameManagerLoadingSteps.None;
			switch (gameManagerLoadingStep)
			{
				case GameManagerLoadingSteps.PreInitializeZerothStep:
					MBGameManager.LoadModuleData(false);
					MBGlobals.InitializeReferences();
					Game.CreateGame(new FastSceneGame(), this).DoLoading();
					nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
					return;
				case GameManagerLoadingSteps.FirstInitializeFirstStep:
				{
					bool flag = true;
					foreach (MBSubModuleBase mbsubModuleBase in Module.CurrentModule.SubModules)
					{
						flag = (flag && mbsubModuleBase.DoLoading(Game.Current));
					}
					nextStep = (flag ? GameManagerLoadingSteps.WaitSecondStep : GameManagerLoadingSteps.FirstInitializeFirstStep);
					return;
				}
				case GameManagerLoadingSteps.WaitSecondStep:
					MBGameManager.StartNewGame();
					nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
					return;
				case GameManagerLoadingSteps.SecondInitializeThirdState:
					nextStep = (Game.Current.DoLoading() ? GameManagerLoadingSteps.PostInitializeFourthState : GameManagerLoadingSteps.SecondInitializeThirdState);
					return;
				case GameManagerLoadingSteps.PostInitializeFourthState:
					nextStep = GameManagerLoadingSteps.FinishLoadingFifthStep;
					return;
				case GameManagerLoadingSteps.FinishLoadingFifthStep:
					nextStep = GameManagerLoadingSteps.None;
					return;
				default:
					return;
			}
		}

		public override void OnLoadFinished()
		{
			base.OnLoadFinished();
			GameStateManager.Current.PushState(new FastSceneState());
			_afterLoad.Invoke();
		}
	}
}