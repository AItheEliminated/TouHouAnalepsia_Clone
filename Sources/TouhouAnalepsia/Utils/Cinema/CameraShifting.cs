using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils.Cinema
{
	public class CameraShifting : AbstractCameraComponent
	{
		private const float RayCastLength = 2000f;

		public float Time = 1f;

		public bool SmoothStart = false;

		public bool SmoothEnd = false;

		public bool NoRoll = true;

		//public bool KeepSightlineDistance = false;

		public bool Slerp = false;

		public override bool Interpolate(float delta, MatrixFrame initFrame, float currentProgress, 
													out MatrixFrame result, out float newProgress)
		{
			var _time = Math.Max(Time, MathF.Epsilon);
			var currentTime = _time * currentProgress;
			currentTime += delta;
			newProgress = Math.Min(currentTime / _time, 1f);

			var actulProgress = newProgress;
			if ((SmoothStart && newProgress < 0.5f) || (SmoothEnd && newProgress > 0.5f))
			{
				actulProgress = MBMath.SmoothStep(0, 1, newProgress);
			}
			

			var targetFrame = GameEntity.GetGlobalFrame();
			var posFrom = initFrame.origin;
			var posTo = targetFrame.origin;
			var rotFrom = initFrame.rotation;
			var rotTo = targetFrame.rotation;
			var posCur = Vec3.Lerp(posFrom, posTo, actulProgress);
			var rotCur = NoRoll ? LerpWithoutRoll(rotFrom, rotTo, actulProgress) : Mat3.Lerp(rotFrom, rotTo, actulProgress);
			if (Slerp)
			{
				var success = true;
				var targetFrom = new Vec3(0, RayCastLength);
				targetFrom = rotFrom.TransformToParent(targetFrom);
				targetFrom = targetFrom + posFrom;
				success &= Scene.RayCastForClosestEntityOrTerrain(posFrom, targetFrom, out var disFrom, out Vec3 closestFrom);
				var targetTo = new Vec3(0, RayCastLength);
				targetTo = rotTo.TransformToParent(targetTo);
				targetTo = targetTo + posTo;
				success &= Scene.RayCastForClosestEntityOrTerrain(posTo, targetTo, out var disTo, out Vec3 closestTo);
				if (success)
				{
					var dis = MathF.Lerp(disFrom, disTo, actulProgress);
					var targetCur = Vec3.Lerp(closestFrom, closestTo, actulProgress);
					posCur = targetCur - rotCur.TransformToParent(new Vec3(0, dis));
				}
				else
				{
					Logger.Log("Failed to perform slerp for camera: " + GameEntity.Name);
				}
			}
			result = new MatrixFrame(rotCur, posCur) * rotFrame;
			return 1 - newProgress < MathF.Epsilon;
		}
	}
}