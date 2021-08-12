using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace TouhouAnalepsia.Editor.Cinema
{
	[ViewCreatorModule]
	public static class CinemaView
	{
		[ViewMethod("Cinema")]
		public static MissionView[] OpenCinemaMission(Mission mission)
		{
			return new List<MissionView>
			{
				ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
				ViewCreator.CreateOptionsUIHandler()
				,ViewCreator.CreateMissionAgentStatusUIHandler(mission),
				ViewCreator.CreateMissionMainAgentEquipmentController(mission),
				ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
				ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
				ViewCreator.CreateMissionAgentLockVisualizerView(mission),
				ViewCreator.CreatePhotoModeView(),
				ViewCreator.CreateMissionOrderUIHandler(mission),
				new OrderTroopPlacer(),
			}.ToArray();
		}
	}
}