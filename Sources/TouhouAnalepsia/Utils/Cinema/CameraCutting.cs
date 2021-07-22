using System;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils.Cinema
{
	public class CameraCutting : AbstractCameraComponent
	{
		public override bool Interpolate(float delta, MatrixFrame initFrame, float currentProgress, 
													out MatrixFrame result, out float newProgress)
		{
			result = GameEntity.GetGlobalFrame() * rotFrame;
			newProgress = 1;
			return true;
		}
	}
}