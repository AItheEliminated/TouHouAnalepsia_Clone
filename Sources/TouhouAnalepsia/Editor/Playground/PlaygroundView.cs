using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace TouhouAnalepsia.Editor.Playground
{
	[ViewCreatorModule]
	public static class PlaygroundView
	{
		[ViewMethod("Playground")]
		public static MissionView[] OpenPlaygroundMission(Mission mission)
		{
			return new List<MissionView>
			{
				ViewCreator.CreateMissionSingleplayerEscapeMenu(),
				ViewCreator.CreateOptionsUIHandler(),
				ViewCreator.CreateMissionAgentStatusUIHandler(mission),
				ViewCreator.CreateMissionMainAgentEquipmentController(mission),
				ViewCreator.CreateMissionMainAgentCheerControllerView(mission),
				ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
			}.ToArray();
		}
	}
}