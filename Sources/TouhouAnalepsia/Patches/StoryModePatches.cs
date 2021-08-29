using HarmonyLib;
using StoryMode.Behaviors;
using TaleWorlds.CampaignSystem;
using static TouhouAnalepsia.HarmonyHandler;

namespace TouhouAnalepsia.Patches {
	class StoryModePatches {
		public static void Patch() {
			patchReturn("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnNewGameCreated");
			patchReturn("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnMainStoryLineSideChosen");
			patchReturn("StoryMode.Behaviors.FirstPhaseCampaignBehavior", "OnStoryModeTutorialEnded");

			patchReturn("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "InitializeTutorial");
			patchReturn("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "OnStoryModeTutorialEnded");
			patchReturn("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "OnGameLoaded");
			patchReturn("StoryMode.Behaviors.TutorialPhaseCampaignBehavior", "RegisterEvents");

			patchReturnFalse("StoryMode.MainStoryLine", "get_IsPlayerInteractionRestricted");
		}

		[HarmonyPatch(typeof(StoryMode.StoryModeSubModule), "AddBehaviors")]
		class DebugPatch19 {
			static bool Prefix(CampaignGameStarter campaignGameStarter) {
				//campaignGameStarter.AddBehavior(new LordConversationsStoryModeBehavior());
				//campaignGameStarter.AddBehavior(new TrainingFieldCampaignBehavior());
				campaignGameStarter.AddBehavior(new TutorialPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new FirstPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new SecondPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new ThirdPhaseCampaignBehavior());
				//campaignGameStarter.AddBehavior(new StoryModeTutorialBoxCampaignBehavior());
				//campaignGameStarter.AddBehavior(new TravelToVillageTutorialQuestBehavior());
				/*
				campaignGameStarter.AddBehavior(new TalkToTheHeadmanTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new PurchaseGrainTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new RecruitTroopsTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new LocateAndRescueTravellerTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new FindHideoutTutorialQuestBehavior());
				campaignGameStarter.AddBehavior(new BannerInvestigationQuestBehavior());
				campaignGameStarter.AddBehavior(new MeetWithIstianaQuestBehavior());
				campaignGameStarter.AddBehavior(new MeetWithArzagosQuestBehavior());
				campaignGameStarter.AddBehavior(new IstianasBannerPieceQuestBehavior());
				campaignGameStarter.AddBehavior(new ArzagosBannerPieceQuestBehavior());
				campaignGameStarter.AddBehavior(new WeakenEmpireQuestBehavior());
				campaignGameStarter.AddBehavior(new AssembleEmpireQuestBehavior());
				campaignGameStarter.AddBehavior(new SupportKingdomQuestBehavior());
				campaignGameStarter.AddBehavior(new CreateKingdomQuestBehavior());
				campaignGameStarter.AddBehavior(new ConspiracyProgressQuestBehavior());
				campaignGameStarter.AddBehavior(new DefeatTheConspiracyQuestBehavior());
				campaignGameStarter.AddBehavior(new RebuildPlayerClanQuestBehavior());
				*/
				//campaignGameStarter.AddBehavior(new RescueFamilyQuestBehavior());
				return false;
			}
		}
	}
}
