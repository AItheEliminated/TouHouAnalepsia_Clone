using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils.Cinema
{
	public class StageDirector : AbstractCameraComponent
	{
		public string SlateText = string.Empty;

		public override bool Interpolate(float delta, MatrixFrame initFrame, float currentProgress, out MatrixFrame result, out float newProgress)
		{
			throw new System.NotImplementedException();
		}
	}
}