using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Utils.Cinema
{
	public abstract class AbstractCameraComponent : ScriptComponentBehaviour
	{
		private const string CameraModelEntityName = "_cameraModel";

		public const float FovMin = 5;

		public const float FovMax = 90;

		public float Fov = 65;

		public bool Preview = false;

		public string Next = string.Empty;

		public float Delay = 0f;

		public string Focus = string.Empty;

		private GameEntity _entity;

		private Camera _camera;

		private SceneView _sceneView;

		private object _focusingObject;

		private bool _hasPrintedError = false;

		private bool _jumpToCamera = false;

		private static WeakReference<AbstractCameraComponent> previewOwner =
			new WeakReference<AbstractCameraComponent>(null);

		private static readonly MetaMesh cameraMesh = MetaMesh.GetCopy("icon_camera");

		protected static readonly Mat3 rotMat3;

		protected static readonly MatrixFrame rotFrame;

		static AbstractCameraComponent()
		{
			rotMat3 = Mat3.Identity;
			rotMat3.RotateAboutSide((float)Math.PI / 2);
			rotFrame = MatrixFrame.Identity;
			rotFrame.Rotate((float)Math.PI / 2, Vec3.Side);
		}

		public virtual MatrixFrame CameraFrame
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(Focus))
				{
					if (_focusingObject == null)
					{
						var focusingObj = Scene.GetFirstEntityWithName(Focus);
						if (focusingObj == null && !_hasPrintedError)
						{
							Logger.Log($"Camera {GameEntity.Name} cannot find focus target: {Focus}");
							_hasPrintedError = true;
						}
					}

					if (_focusingObject is GameEntity focusingEntity)
					{
						return CreateCameraLookAt(GameEntity.GetGlobalFrame().origin,
							focusingEntity.GetGlobalFrame().origin, Vec3.Up);
					}
				}
				return GameEntity.GetGlobalFrame();
			}
		}

		protected override void OnInit()
		{
		}

		protected override void OnEditorInit()
		{
		}

		protected override void OnRemoved(int removeReason)
		{
			if (previewOwner.TryGetTarget(out var owner) && owner == this)
			{
				MBEditor.GetEditorSceneView().SetEnable(true);
			}
			if (_sceneView != null)
			{
				_sceneView.SetEnable(false);
				_sceneView.ManualInvalidate();
				_camera.ReleaseCamera();
				_camera.ManualInvalidate();
				_sceneView = null;
				_camera = null;
			}
		}

		protected override TickRequirement GetTickRequirement()
		{
			return TickRequirement.Tick;
		}

		protected override void OnEditorTick(float dt)
		{
			Fov = MathF.Clamp(Fov, FovMin, FovMax);
			if (Preview && MBEditor.IsEntitySelected(GameEntity))
			{
				if (_sceneView == null)
				{
					_sceneView = SceneView.CreateSceneView();
					_camera = Camera.CreateCamera();
					_sceneView.SetScene(GameEntity.Scene);
					_sceneView.SetPostfxFromConfig();
					_sceneView.SetRenderOption(View.ViewRenderOptions.ClearColor, true);
					_sceneView.SetRenderOption(View.ViewRenderOptions.ClearDepth, true);
					_sceneView.SetCamera(_camera);
				}
				// fix a bug which can crash the game
				/*if (!_jumpToCamera)
				{
					//Scene.EnsurePostfxSystem();
					MBEditor.ZoomToPosition(GameEntity.GetGlobalFrame().origin);
					_jumpToCamera = true;
					return;
				}*/
				var realScreenResolution = Screen.RealScreenResolution;
				var aspectRatioXY = realScreenResolution.x / realScreenResolution.y;
				var globalFrame = MatrixFrame.Identity;
				globalFrame.Rotate((float) Math.PI / 2, Vec3.Side);
				globalFrame = GameEntity.GetGlobalFrame() * globalFrame;
				_camera.Frame = globalFrame;
				_camera.SetFovHorizontal(Fov / 65f * aspectRatioXY, aspectRatioXY, 0.2f, 1500f);
				_sceneView.SetCamera(_camera);
				if (!previewOwner.TryGetTarget(out var oldOwner) || oldOwner != this)
				{
					previewOwner.SetTarget(this);
					MBEditor.GetEditorSceneView().SetEnable(false);
				}
				_sceneView.SetEnable(true);
				return;
			}
			_jumpToCamera = false;
			_sceneView?.SetEnable(false);
			if (previewOwner.TryGetTarget(out var owner) && owner == this)
			{
				previewOwner.SetTarget(null);
				MBEditor.GetEditorSceneView().SetEnable(true);
			}
			MBEditor.RenderEditorMesh(cameraMesh, GameEntity.GetGlobalFrame());
		}

		public abstract bool Interpolate(float delta, MatrixFrame initFrame, float currentProgress, 
													out MatrixFrame result, out float newProgress);

		protected static MatrixFrame CreateCameraLookAt(Vec3 position, Vec3 target, Vec3 up)
		{
			var heightDelta = target.Z - position.Z;
			var delta = target - position;
			var length = delta.Length;
			if (length < MathF.Epsilon)
			{
				return Camera.ConstructCameraFromPositionElevationBearing(position, 0, 0);
			}
			else
			{
				var elevation = (float)Math.Asin(heightDelta / length);
				var bearing = Vec3.AngleBetweenTwoVectors(Vec3.Forward, new Vec3(delta.X, delta.Y));
				if (delta.X < 0)
					bearing = MathF.TwoPI - bearing;
				bearing = -bearing;
				return Camera.ConstructCameraFromPositionElevationBearing(position, elevation, bearing);
			}
		}

		protected static Mat3 LerpWithoutRoll(Mat3 a, Mat3 b, float alpha)
		{
			var eulers = Mat3.Lerp(a, b, alpha).GetEulerAngles();
			eulers.y = 0;
			var result = Mat3.Identity;
			result.ApplyEulerAngles(eulers);
			return result;
		}
	}
}