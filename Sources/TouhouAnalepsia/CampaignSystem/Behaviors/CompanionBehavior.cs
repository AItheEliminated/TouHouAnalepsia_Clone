using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace TouhouAnalepsia.CampaignSystem.Behaviors
{
	public class CompanionBehavior : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
		{
			// force spawning the special companions (kokoro, sumireko etc.)
			// since 1.6.1 the game will try to assign a random equipment set
			// to any hero who didn't spawn when the game created
			foreach (var hero in Hero.AllAliveHeroes)
			{
				if (hero.IsWanderer && hero.IsNotSpawned)
				{
					hero.ChangeState(Hero.CharacterStates.Active);
					TeleportHeroAction.ApplyForCharacter(hero, hero.HomeSettlement);
				}
			}
		}
	}
}