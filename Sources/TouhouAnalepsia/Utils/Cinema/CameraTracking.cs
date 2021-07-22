using System;
using System.Numerics;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils.Cinema
{
	public class CameraTracking : AbstractCameraComponent
	{
		public float Time = 1f;

		public bool SmoothStart = false;

		public bool SmoothEnd = false;

		public bool NoRoll = true;

		public string Path = string.Empty;

		public string LookingPath = string.Empty;

		public bool SkipJourneyBeforePath = false;

		public bool SkipJourneyAfterPath = false;

		public bool UsingPathRotation = false;

		private bool _hasPrintedError = false;

		private MatrixFrame[] _points;

		private MatrixFrame[] _lookingPoints;

		private float _distanceBeforePath;

		private float _distanceAfterPath;

		private float _distanceInPath;

		private float _totalDistance;

		private float _totalLookingDistance;

		private float _progressBeforeEnterPath;

		private float _progressAfterLeavePath;

		private bool _hasInited = false;

		private Path _path;

		private Path _lookingPath = null;

		public override MatrixFrame CameraFrame
		{
			get
			{
				// those codes are a bit dirty and redundant. I didn't spend too much time optimizing those.
				if (SkipJourneyAfterPath)
				{
					Path _path2;
					if (!string.IsNullOrWhiteSpace(Path) && (_path2 = Scene.GetPathWithName(Path)) != null)
					{
						var _points = new MatrixFrame[_path2.NumberOfPoints];
						_path2.GetPoints(_points);
						var resultPoint = _points[_path2.NumberOfPoints - 1];
						if (!string.IsNullOrWhiteSpace(LookingPath) && (_path2 = Scene.GetPathWithName(LookingPath)) != null)
						{
							_points = new MatrixFrame[_path2.NumberOfPoints];
							_path2.GetPoints(_points);
							var lookingPoint = _points[_path2.NumberOfPoints - 1];
							resultPoint = CreateCameraLookAt(resultPoint.origin, lookingPoint.origin, Vec3.Up);
						}
						return resultPoint;
					}
				}
				return base.CameraFrame;
			}
		}

		public override bool Interpolate(float delta, MatrixFrame initFrame, float currentProgress, 
													out MatrixFrame result, out float newProgress)
		{
			if (!_hasInited)
			{
				_hasInited = true;

				if (string.IsNullOrWhiteSpace(Path) || (_path = Scene.GetPathWithName(Path)) == null)
				{
					if (!_hasPrintedError)
					{
						Logger.Log("CameraTracking cannot find the path: " + Path);
						_hasPrintedError = true;
					}

					newProgress = 0;
					result = initFrame * rotFrame;
					return false;
				}

				if (!string.IsNullOrWhiteSpace(LookingPath))
				{
					_lookingPath = Scene.GetPathWithName(LookingPath);
					if (_lookingPath == null)
					{
						if (!_hasPrintedError)
						{
							Logger.Log("CameraTracking cannot find the looking path: " + LookingPath);
							_hasPrintedError = true;
						}
					}
					else if (_lookingPath.NumberOfPoints == 0)
					{
						if (!_hasPrintedError)
						{
							Logger.Log("No point in looking path: " + LookingPath);
							_hasPrintedError = true;
						}

						_lookingPath = null;
					}
				}
			}

			var gameEntityFrame = GameEntity.GetGlobalFrame();

			if (_path.NumberOfPoints == 0)
			{
				Logger.Log("No point in path " + Path + ". It will be skipped.");
				newProgress = 1f;
				result = gameEntityFrame * rotFrame;
				return true;
			}

			var _time = Math.Max(Time, MathF.Epsilon);
			var currentTime = _time * currentProgress;
			currentTime += delta;
			newProgress = Math.Min(currentTime / _time, 1f);

			var actulProgress = newProgress;
			if ((SmoothStart && newProgress < 0.5f) || (SmoothEnd && newProgress > 0.5f))
			{
				actulProgress = MBMath.SmoothStep(0, 1, newProgress);
			}

			if (_points == null)
			{
				_points = new MatrixFrame[_path.NumberOfPoints];
				_path.GetPoints(_points);
				if (SkipJourneyBeforePath)
				{
					_distanceBeforePath = 0;
				}
				else
				{
					_distanceBeforePath = (_points[0].origin - initFrame.origin).Length;
				}

				if (SkipJourneyAfterPath)
				{
					_distanceAfterPath = 0f;
				}
				else
				{
					_distanceAfterPath = (gameEntityFrame.origin - _points[_path.NumberOfPoints - 1].origin).Length;
				}
				
				_distanceInPath = GetPathLength(_path);
				_totalDistance = _distanceBeforePath + _distanceInPath + _distanceAfterPath;
				_totalDistance = Math.Max(_totalDistance, MathF.Epsilon);
				_progressBeforeEnterPath = _distanceBeforePath / _totalDistance;
				_progressAfterLeavePath = (_distanceBeforePath + _distanceInPath) / _totalDistance;

				_lookingPoints = null;
				_totalLookingDistance = 0f;
				if (_lookingPath != null)
				{
					_lookingPoints = new MatrixFrame[_lookingPath.NumberOfPoints];
					_lookingPath.GetPoints(_lookingPoints);
					_totalLookingDistance = GetPathLength(_lookingPath);
				}
			}

			if (1 - newProgress < MathF.Epsilon)
			{
				newProgress = 1f;
				result = CameraFrame;
				if (!SkipJourneyAfterPath)
					result = result * rotFrame;
				return true;
			}

			float progress;
			Vec3 posFrom, posTo, posCur;
			Mat3 rotFrom, rotTo, rotCur;
			if (!SkipJourneyBeforePath && actulProgress < _progressBeforeEnterPath)
			{
				progress = actulProgress / _progressBeforeEnterPath;
				progress = MathF.Clamp(progress, 0, 1);
				var targetFrame = _points[0];
				posFrom = initFrame.origin;
				posTo = targetFrame.origin;
				posCur = Vec3.Lerp(posFrom, posTo, progress);
				rotFrom = initFrame.rotation.TransformToParent(rotMat3);
				if (_lookingPath != null)
				{
					var lookFrame = CreateCameraLookAt(posTo, _lookingPoints[0].origin, Vec3.Up);
					rotCur = NoRoll ? LerpWithoutRoll(rotFrom, lookFrame.rotation, progress) :
									Mat3.Lerp(rotFrom, lookFrame.rotation, progress);
				}
				else
				{
					rotTo = UsingPathRotation ? targetFrame.rotation : gameEntityFrame.rotation;
					rotTo = rotTo.TransformToParent(rotMat3);
					if (NoRoll)
					{
						rotCur = UsingPathRotation ?
							LerpWithoutRoll(rotFrom, rotTo, progress) :
							LerpWithoutRoll(rotFrom, rotTo, actulProgress);
					}
					else
					{
						rotCur = UsingPathRotation ?
							Mat3.Lerp(rotFrom, rotTo, progress) :
							Mat3.Lerp(rotFrom, rotTo, actulProgress);
					}
				}
				result = new MatrixFrame(rotCur, posCur);
				//Logger.Log("Before: " + progress);
				return false;
			}
			else if (actulProgress < _progressAfterLeavePath)
			{
				progress = (actulProgress - _progressBeforeEnterPath) / (_progressAfterLeavePath - _progressBeforeEnterPath);
				progress = MathF.Clamp(progress, 0, 1);
				result = _path.GetFrameForDistance(_distanceInPath * progress) * rotFrame;
				if (_lookingPath != null)
				{
					var pos = result.origin;
					//var target = _lookingPath.GetFrameForDistance(0).origin;
					var target = _lookingPath.GetFrameForDistance(_totalLookingDistance * progress).origin;
					result = CreateCameraLookAt(pos, target, Vec3.Up);
				}
				else if (!UsingPathRotation)
				{
					rotFrom = initFrame.rotation;
					rotTo = gameEntityFrame.rotation;
					result = new MatrixFrame(
							NoRoll ? LerpWithoutRoll(rotFrom, rotTo, actulProgress) :
							Mat3.Lerp(rotFrom, rotTo, actulProgress), 
						result.origin) * rotFrame;
				}
				//Logger.Log("In: " + progress);
				return false;
			}
			else
			{
				progress = (actulProgress - _progressAfterLeavePath) / (1f - _progressAfterLeavePath);
				// dirty hack for SkipJourneyAfterPath
				// sometime, progress could be NaN because divide by 0 (while _progressAfterLeavePath is 1)
				if (float.IsNaN(progress) || float.IsPositiveInfinity(progress))
					progress = 1f;
				progress = MathF.Clamp(progress, 0, 1);
				var fromFrame = _points[_path.NumberOfPoints - 1];
				posFrom = fromFrame.origin;
				posTo = (SkipJourneyAfterPath ? _points[_path.NumberOfPoints - 1] : gameEntityFrame).origin;
				posCur = Vec3.Lerp(posFrom, posTo, progress);
				rotTo = gameEntityFrame.rotation.TransformToParent(rotMat3);
				if (_lookingPath != null)
				{
					var lookFrame = CreateCameraLookAt(posFrom, 
						_lookingPoints[_lookingPath.NumberOfPoints - 1].origin, Vec3.Up);
					rotCur = NoRoll ? LerpWithoutRoll(lookFrame.rotation, rotTo, progress) : 
									Mat3.Lerp(lookFrame.rotation, rotTo, progress);
				}
				else
				{
					rotFrom = fromFrame.rotation.TransformToParent(rotMat3);
					if (NoRoll)
					{
						rotCur = UsingPathRotation ?
							LerpWithoutRoll(rotFrom, rotTo, progress) :
							LerpWithoutRoll(rotFrom, rotTo, actulProgress);
					}
					else
					{
						rotCur = UsingPathRotation ?
							Mat3.Lerp(rotFrom, rotTo, progress) :
							Mat3.Lerp(rotFrom, rotTo, actulProgress);
					}
				}
				result = new MatrixFrame(rotCur, posCur);
				//Logger.Log("After: " + progress);
				return false;
			}
		}

		private static float GetPathLength(Path path)
		{
			var result = path.GetTotalLength();
			if (result == 0)
			{
				var arcs = path.NumberOfPoints - 1;
				for (int i = 0; i < arcs; i++)
				{
					result += path.GetArcLength(i);
				}
			}

			return result;
		}
	}
}