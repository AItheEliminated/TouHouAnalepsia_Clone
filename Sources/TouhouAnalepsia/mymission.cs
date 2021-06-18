using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Engine;
using System.Reflection;
using Module = TaleWorlds.MountAndBlade.Module;
using TouhouAnalepsia.Editor.FastScene;
using TouhouAnalepsia.Editor.Playground;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using StoryMode.Behaviors;
using SandBox;
using System.Xml;
using TaleWorlds.ModuleManager;
using TouhouAnalepsia.Abilities;
using TaleWorlds.CampaignSystem.SandBox;
using StoryMode.StoryModeObjects;
using Helpers;



class MyCampainBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, SettlementEnteredEvent);
    }

    // Event triggered when any party enters a settlement
    private void SettlementEnteredEvent(MobileParty arg1, Settlement arg2, Hero arg3)
    {
        if (arg3 != Hero.MainHero)
            return;

        // Do not repeat this quest. (Currently don't know how to check completed quests)
        if (Campaign.Current.QuestManager.Quests.Any(q => q.GetType() == typeof(MyCampainQuest)))
            return;

        Hero quest_giver = Hero.MainHero;

        // Find nearest village around the player that is not the current settlement
        IEnumerable<Settlement> settlements = Settlement.FindSettlementsAroundPosition(quest_giver.GetPosition().AsVec2, 200.0f)
            .OrderBy(s => s.GetTrackDistanceToMainAgent())
            .Where(s => s.IsVillage && s != arg2);

        // Start the quest in that settlement
        Settlement settlement = settlements.First();
        if (settlement != null)
            new MyCampainQuest(quest_giver, settlement).StartQuest();
    }

    public override void SyncData(IDataStore dataStore)
    {
        // Not sure what this does. Multiplayer maybe?
    }

    public class MyCampainQuestBehaviorTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
    {
        public MyCampainQuestBehaviorTypeDefiner()
            // This number is the SaveID, supposed to be a unique identifier
            : base(846_000_000)
        {
        }

        protected override void DefineClassTypes()
        {
            // Your quest goes here, second argument is the SaveID
            AddClassDefinition(typeof(MyCampainBehavior.MyCampainQuest), 1);
        }
    }

    internal class MyCampainQuest : QuestBase
{
    // [SaveableField(1)]
    private bool _metAntiImperialMentor;
    // [SaveableField(2)]
    protected float _TotalDistanceTraveled;

    // Save JournalLog so we can update it or just keep track of it
    protected JournalLog _Task1;
    protected readonly int _DistanceToTravel = 100;
    protected Vec2 _PreviousPos;


    private TextObject _StartQuestLog
    {
        get
        {
            TextObject parent = new TextObject("Find and meet {HERO.LINK} and tell him he is your dad. He is currently in {SETTLEMENT}.");
            StringHelpers.SetCharacterProperties("HERO", StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother.CharacterObject, null);
            parent.SetTextVariable("SETTLEMENT", StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother.CurrentSettlement.EncyclopediaLinkWithName);
            
            return parent;
        }
    }

    private TextObject _EndQuestLog
    {
        get
        {
            TextObject parent = new TextObject("You talked with {HERO.LINK}.");
            StringHelpers.SetCharacterProperties("HERO", StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother.CharacterObject, null);
            return parent;
        }
    }

    private TextObject _YouShouldRunLog
    {
        get
        {
            TextObject parent = new TextObject("Your \"dad\" wants you to run {DISTANCE} units.");
            parent.SetTextVariable("DISTANCE", _DistanceToTravel);
            return parent;
        }
    }

    private TextObject _YouShouldRunText
    {
        get
        {
            TextObject parent = new TextObject("How dare you? How about you run {DISTANCE} units, just for fun.");
            parent.SetTextVariable("DISTANCE", _DistanceToTravel);
            return parent;
        }
    }

    public override TextObject Title
    {
        get
        {
            TextObject parent = new TextObject("Meet with your \"dad\" {HERO.NAME}");
            StringHelpers.SetCharacterProperties("HERO", StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother.CharacterObject, (TextObject) null);
            return parent;
        }
    }

    public override bool IsRemainingTimeHidden
    {
        get
        {
            return false;
        }
    }

    public MyCampainQuest(Hero questGiver, Settlement settlement)
        : base("my_campain_story_mode_quest", questGiver, duration: CampaignTime.DaysFromNow(20), rewardGold: -100 )
    {
        _metAntiImperialMentor    = false;
        SetDialogs();
        HeroHelper.SpawnHeroForTheFirstTime(StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother, settlement);
        AddTrackedObject(settlement);

        AddLog(_StartQuestLog);

        AddTwoWayContinuousLog(new TextObject("This is a two way continuous Log. We are not going to use it."), new TextObject("Task 2"), 0, 10);
    }

    protected override void RegisterEvents()
    {
        CampaignEvents.TickEvent.AddNonSerializedListener(this, Tick);
    }

    protected void Tick(float dt)
    {
        if (_Task1 != null && !_Task1.HasBeenCompleted())
        {
            // Compute total distance traveled on the map.
            Vec2 current_pos        = Hero.MainHero.GetPosition().AsVec2;
            _TotalDistanceTraveled    += current_pos.Distance(_PreviousPos);

            _Task1.UpdateCurrentProgress((int)Math.Floor(_TotalDistanceTraveled));

            _PreviousPos = current_pos;
        }
    }

    protected override void InitializeQuestOnGameLoad()
    {
        SetDialogs();
    }

    protected override void SetDialogs()
    {
        // Dialog to offer the quest?
        OfferDialogFlow = DialogFlow.CreateDialogFlow(QuestManager.QuestOfferToken, 100)
            .NpcLine(new TextObject("{=!}You shouldn't see this."))
            .Condition(() => Hero.OneToOneConversationHero == QuestGiver)
            .CloseDialog();

        // Dialog to discuss the quest?
        DiscussDialogFlow = DialogFlow.CreateDialogFlow(QuestManager.QuestDiscussToken, 100)
            .NpcLine(new TextObject("{=!}You shouldn't see this."))
            .Condition(() => Hero.OneToOneConversationHero == QuestGiver)
            .CloseDialog();

        // Dialog when starting a conversation with the AntiImperialMentor if we have NOT met him
        DialogFlow unmet_dialog = DialogFlow.CreateDialogFlow(QuestManager.NpcLordStartToken, 110)
            .NpcLine(new TextObject("淬樱骑开发的怎么样了？"))
            // This whole dialog will only start if the following conditions are met
            .Condition(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother && !_metAntiImperialMentor)

            .PlayerLine(new TextObject("是的，大家都在摸鱼"))
            .NpcLine(new TextObject("Is that true? Well, that is interesting."))
            // Player option: Starts with BeginPlayerOptions() and ends with EndPlayerOptions()
            .BeginPlayerOptions()
                // PlayerOption will trigger the following dialog flow up to the next CloseDialog() when selected
                .PlayerOption(new TextObject("Yes, it's really me dad."), null)
                    .NpcLine(new TextObject("Good. I don't have anything to say."))
                    .Consequence(new ConversationSentence.OnConsequenceDelegate(GoodBoy))
                .CloseDialog()
                .PlayerOption(new TextObject("Actually I was just joking."), null)
                    .Consequence(new ConversationSentence.OnConsequenceDelegate(BadBoy))
                    .NpcLine(_YouShouldRunText)
                    .NpcLine(new TextObject("Now, GO!"))
                .CloseDialog()
            .EndPlayerOptions()
            .CloseDialog();

        // Dialog when starting a conversation with the AntiImperialMentor if we have met him
        DialogFlow met_dialog = DialogFlow.CreateDialogFlow(QuestManager.NpcLordStartToken, 110)
            .NpcLine(new TextObject("So, did you run like I told you to?"))
            // This whole dialog will only start if the following conditions are met
            .Condition(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryMode.StoryModeObjects.StoryModeHeroes.LittleBrother && _metAntiImperialMentor)

            .BeginPlayerOptions()
                .PlayerOption(new TextObject("Yes I did."), null)
                .Condition(() => _Task1.HasBeenCompleted())
                    .NpcLine(new TextObject("Ok Great. Now leave"))
                    .Consequence(() => Campaign.Current.ConversationManager.ConversationEndOneShot += new Action(CompleteQuestWithSuccess))
                .CloseDialog()
                .PlayerOption(new TextObject("Actuall no, I didn't."), null)
                    .NpcLine(new TextObject("Don't come back until you did."))
                .CloseDialog()
            .EndPlayerOptions()
            .CloseDialog();

        Campaign.Current.ConversationManager.AddDialogFlow(unmet_dialog, this);
        Campaign.Current.ConversationManager.AddDialogFlow(met_dialog, this);
    }

    private void GoodBoy()
    {
        AddLog(new TextObject("You were a good boy"));
        Campaign.Current.ConversationManager.ConversationEndOneShot += new Action(CompleteQuestWithSuccess);
        _metAntiImperialMentor = true;
    }

    private void BadBoy()
    {
        AddLog(new TextObject("You were a bad boy"));
        _Task1 = AddDiscreteLog(_YouShouldRunLog, new TextObject("Units ran."), currentProgress: 0, targetProgress: _DistanceToTravel);
        _PreviousPos            = Hero.MainHero.GetPosition().AsVec2;
        _TotalDistanceTraveled    = 0.0f;
        _metAntiImperialMentor    = true;
    }

    // Called after the quest is finished
    protected override void OnFinalize()
    {
        base.OnFinalize();
        AddLog(_EndQuestLog);
    }
}
}