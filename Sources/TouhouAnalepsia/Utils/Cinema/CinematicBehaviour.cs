using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TouhouAnalepsia.Utils.Cinema
{
	public class CinematicBehaviour : MissionLogic
	{
		private StageDirector _director;

		private AbstractCameraComponent _prevCamera;

		private AbstractCameraComponent _currentCamera;

		private float _timer;

		private float _progress = 0f;

		private Camera benchmarkCamera;

		private float _lastShowTime = 0;

		public override void OnBehaviourInitialize()
		{
			var directorEntity = Mission.Scene.GetFirstEntityWithScriptComponent<StageDirector>();
			if (directorEntity == null)
			{
				Logger.Log("Cannot find any entity with StageDirector script");
				_timer = 0;
			}
			else
			{
				_director = directorEntity.GetFirstScriptOfType<StageDirector>();
				if (!string.IsNullOrEmpty(_director.SlateText))
					Logger.Log(_director.SlateText);
				_prevCamera = null;
				_currentCamera = _director;
				_timer = _director.Delay;
			}
		}

		/*public override void OnMissionTick(float dt)
		{
			base.OnMissionTick(dt);
		}*/

		public override void OnPreDisplayMissionTick(float dt)
		{
			/*_lastShowTime += dt;
			if (_lastShowTime > 1)
			{
				_lastShowTime -= 1;
				Logger.Log("" + (int) Math.Round(Mission.Time));
			}*/

			_timer -= dt;
			if (_timer > 0)
			{
				return;
			}
			dt = -_timer;
			_timer = 0f;

			if (_currentCamera != null)
			{
				if (_director == _currentCamera)
				{
					InformationManager.ClearAllMessages();
					MessageManager.EraseMessageLines();
				}

				if (_prevCamera == null) // find the next camera
				{
					_progress = 0f;

					if (!string.IsNullOrWhiteSpace(_currentCamera.Next))
					{
						var nextEntity = Mission.Scene.FindEntityWithName(_currentCamera.Next);
						if (nextEntity == null)
						{
							Logger.Log("Cannot find entity: " + _currentCamera.Next);
							_currentCamera = null;
						}
						else
						{
							var cameraComponent = nextEntity.GetFirstScriptOfType<AbstractCameraComponent>();
							if (cameraComponent == null)
							{
								Logger.Log("Entity " + _currentCamera.Next + " isn't a camera");
								_currentCamera = null;
							}
							else
							{
								_prevCamera = _currentCamera;
								_currentCamera = cameraComponent;
							}
						}
					}
					else
					{
						_currentCamera = null;
					}
				}

				if (_prevCamera != null && _currentCamera != null) // tick the camera
				{
					var start = _prevCamera.CameraFrame;
					var isFinished =
						_currentCamera.Interpolate(dt, start, _progress, out var result, out var newProgress);
					//Mission.SetCameraFrame(result * rotateFrame, 1f);
					MissionScreen missionScreen;
					if ((missionScreen = (ScreenManager.TopScreen as MissionScreen)) != null)
					{
						Camera combatCamera = missionScreen.CombatCamera;
						/*if (benchmarkCamera == null)
						{
							benchmarkCamera = Camera.CreateCamera();
						}*/
						var fov = MathF.Lerp(_prevCamera.Fov, _currentCamera.Fov, newProgress);
						fov = MathF.Clamp(fov, AbstractCameraComponent.FovMin, AbstractCameraComponent.FovMax);
						missionScreen.UpdateFreeCamera(result);
						missionScreen.CustomCamera = missionScreen.CombatCamera;
						missionScreen.CustomCamera.SetFovHorizontal(fov / 65f * combatCamera.GetAspectRatio(),
							combatCamera.GetAspectRatio(),
							combatCamera.Near, combatCamera.Far);
					}
					_progress = newProgress;
					if (isFinished)
					{
						_prevCamera = null;
						_timer = _currentCamera.Delay;
					}
				}
			}
		}

		public override bool MissionEnded(ref MissionResult missionResult)
		{
			return Mission.InputManager.IsGameKeyPressed(4);
		}
	}
}