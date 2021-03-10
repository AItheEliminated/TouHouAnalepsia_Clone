using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using Helpers;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace TouhouAnalepsia {
	public class TouhouModelManager {
		public static void initializeTouhouModels(IGameStarter gameStarterObject) {
			gameStarterObject.AddModel(new TouhouMarriageModel());

			gameStarterObject.AddModel(new TouhouAgeModel());
			gameStarterObject.AddModel(new TouhouHeroDeathProbabilityCalculationModel());

			gameStarterObject.AddModel(new TouhouDiplomacyModel());

			gameStarterObject.AddModel(new TouhouKingdomCreationModel());
		}
	}
    class TouhouMarriageModel : DefaultMarriageModel {
		public override int MinimumMarriageAgeMale {
			get {
				return 16;
			}
		}
		public override int MinimumMarriageAgeFemale {
			get {
				return 16;
			}
		}
		public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero) {
			Clan clan = firstHero.Clan;
			if (((clan != null) ? clan.Leader : null) == firstHero) {
				Clan clan2 = secondHero.Clan;
				if (((clan2 != null) ? clan2.Leader : null) == secondHero) {
					return false;
				}
			}
			//Yuri procreation, of course, and incest is *probably* not too big of a deal. TODO: Perhaps some unique mechanic for procreation?
			//if (firstHero.IsFemale != secondHero.IsFemale && !this.DiscoverAncestors(firstHero, 3).Intersect(this.DiscoverAncestors(secondHero, 3)).Any<Hero>()) {

			//Reimplemented the incest check for human cultures.
			if ((!CultureHandler.isHuman(firstHero.Culture) && !CultureHandler.isHuman(secondHero.Culture)) || !this.DiscoverAncestors(firstHero, 3).Intersect(this.DiscoverAncestors(secondHero, 3)).Any<Hero>()) {
				return this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
			}
			//}
			return false;
		}
		private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n) { // Vanilla Settings
			if (hero != null) {
				yield return hero;
				if (n > 0) {
					foreach (Hero hero2 in this.DiscoverAncestors(hero.Mother, n - 1)) {
						yield return hero2;
					}
					foreach (Hero hero3 in this.DiscoverAncestors(hero.Father, n - 1)) {
						yield return hero3;
					}
				}
			}
		}
	}

	public class TouhouAgeModel : AgeModel {
		public override int BecomeInfantAge {
			get {
				return 3;
			}
		}
		public override int BecomeChildAge {
			get {
				return 6;
			}
		}
		public override int BecomeTeenagerAge {
			get {
				return 14;
			}
		}

		public override int HeroComesOfAge {
			get {
				return 16;
			}
		}
		public override int BecomeOldAge { //See MaxAge comment bellow.
			get {
				return 5000;
			}
		}
		public override int MaxAge { //Or something similar, further testing on game effects of this is needed. Doesn't *seem* to effect much except visual stuff in dnSpy, but probably something I'm missing.
			get {
				return 10000;
			}
		}
	}
	public class TouhouHeroDeathProbabilityCalculationModel : DefaultHeroDeathProbabilityCalculationModel { //Vanilla Settings - I'll work on this a bit more later on, so true immortals don't die.
		
	}

	public class TouhouDiplomacyModel : DefaultDiplomacyModel {
		public override float GetScoreOfClanToJoinKingdom(Clan clan, Kingdom kingdom) {
			if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan) {
				return -100000000f;
			}
			int relationBetweenClans = FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan);
			float num = (float)Math.Min(2.0, Math.Max(0.5, 1.0 + Math.Sqrt((double)Math.Abs(relationBetweenClans)) * (double)((relationBetweenClans < 0) ? -0.06f : 0.04f)));
			float num2 = 1f + (CultureHandler.isCultureAccepted(clan.Culture,FactionCulture.getFactionCultureByID(kingdom.Culture.StringId)) ? 0.15f : ((kingdom.Leader == Hero.MainHero) ? 0f : -0.15f));
			float num3 = clan.CalculateSettlementValue(null);
			float num4 = clan.CalculateSettlementValue(kingdom);
			int commanderLimit = clan.CommanderLimit;
			float num5 = 0f;
			float num6 = 0f;
			if (!clan.IsMinorFaction) {
				float num7 = 0f;
				foreach (Town town in kingdom.Fiefs) {
					num7 += town.Settlement.GetSettlementValueForFaction(kingdom);
				}
				int num8 = 0;
				foreach (Clan clan2 in kingdom.Clans) {
					if (!clan2.IsUnderMercenaryService) {
						num8 += clan2.CommanderLimit;
					}
				}
				num5 = num7 / (float)(num8 + commanderLimit);
				num6 = -((float)(num8 * num8) * 50f);
			}
			float num9 = (clan.Leader != null && clan.Leader.Gold < 100000) ? (0.33f + 0.67f * (1f - (float)clan.Leader.Gold / 100000f) * (1f - (float)clan.Leader.Gold / 100000f)) : 0.33f;
			return num5 * (float)Math.Sqrt((double)commanderLimit) * num9 * 0.2f * (num * num2) + (clan.MapFaction.IsAtWarWith(kingdom) ? (num4 - num3) : 0f) + num6;
		}

		public override float GetScoreOfClanToLeaveKingdom(Clan clan, Kingdom kingdom) {
			int relationBetweenClans = FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan);
			float num = (float)Math.Min(2.0, Math.Max(0.5, 1.0 + Math.Sqrt((double)Math.Abs(relationBetweenClans)) * (double)((relationBetweenClans < 0) ? -0.06f : 0.04f)));
			float num2 = 1f + (CultureHandler.isCultureAccepted(clan.Culture, FactionCulture.getFactionCultureByID(kingdom.Culture.StringId)) ? 0.15f : ((kingdom.Leader == Hero.MainHero) ? 0f : -0.15f));
			float num3 = clan.CalculateSettlementValue(null);
			float num4 = clan.CalculateSettlementValue(kingdom);
			int commanderLimit = clan.CommanderLimit;
			float num5 = 0f;
			if (!clan.IsMinorFaction) {
				float num6 = 0f;
				foreach (Town town in kingdom.Fiefs) {
					num6 += town.Settlement.GetSettlementValueForFaction(kingdom);
				}
				int num7 = 0;
				foreach (Clan clan2 in kingdom.Clans) {
					if (!clan2.IsUnderMercenaryService) {
						num7 += clan2.CommanderLimit;
					}
				}
				num5 = num6 / (float)(num7 + commanderLimit);
			}
			float num8 = HeroHelper.CalculateReliabilityConstant(clan.Leader, 1f);
			double toDays = (CampaignTime.Now - clan.LastFactionChangeTime).ToDays;
			float num9 = 4000f * (float)(15.0 - Math.Sqrt(Math.Min(225.0, toDays)));
			float num10 = 60000f;// + (float)((clan.Fortifications != null) ? clan.Fortifications.Count<Town>() : 0) * 10000f; //TODO reimplement this
			float num11 = (clan.Leader.Gold < 100000) ? (0.33f + 0.67f * (1f - (float)clan.Leader.Gold / 100000f) * (1f - (float)clan.Leader.Gold / 100000f)) : 0.33f;
			num10 /= num11;
			return (-num5 * (float)Math.Sqrt((double)commanderLimit) * num11 * 0.2f - num10 * num8 + -num9) * (num * num2) + (num3 - num4) + (float)((kingdom.Ruler == Hero.MainHero) ? -80000 : 0);
		}
		public override float GetScoreOfKingdomToGetClan(Kingdom kingdom, Clan clan) {
			float num = Math.Min(2f, Math.Max(0.33f, 1f + 0.02f * (float)FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan)));
			float num2 = 1f + (CultureHandler.isCultureAccepted(clan.Culture, FactionCulture.getFactionCultureByID(kingdom.Culture.StringId)) ? 1f : 0f);
			int commanderLimit = clan.CommanderLimit;
			float num3 = (clan.TotalStrength + 150f * (float)commanderLimit) * 20f;
			float powerRatioToEnemies = FactionHelper.GetPowerRatioToEnemies(kingdom);
			float num4 = HeroHelper.CalculateReliabilityConstant(clan.Leader, 1f);
			float num5 = 1f / Math.Max(0.4f, Math.Min(2.5f, (float)Math.Sqrt((double)powerRatioToEnemies)));
			num3 *= num5;
			return (clan.CalculateSettlementValue(kingdom) * 0.1f + num3) * num * num2 * num4;
		}
		public override float GetScoreOfKingdomToSackClan(Kingdom kingdom, Clan clan) {
			float num = Math.Min(2f, Math.Max(0.33f, 1f + 0.02f * (float)FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan)));
			float num2 = 1f + (CultureHandler.isCultureAccepted(clan.Culture, FactionCulture.getFactionCultureByID(kingdom.Culture.StringId)) ? 1f : 0.5f);
			int commanderLimit = clan.CommanderLimit;
			float num3 = (clan.TotalStrength + 150f * (float)commanderLimit) * 20f;
			float num4 = clan.CalculateSettlementValue(kingdom);
			return 10f - 1f * num3 * num2 * num - num4;
		}
	}

	public class TouhouKingdomCreationModel : DefaultKingdomCreationModel {

		public override IEnumerable<CultureObject> GetAvailablePlayerKingdomCultures() {
			List<CultureObject> cultures = new List<CultureObject>();
			cultures.Add(Clan.PlayerClan.Culture);
			foreach (Settlement settlement in from t in Clan.PlayerClan.Settlements
											  where t.IsTown || t.IsCastle
											  select t) {
				if (!cultures.Contains(settlement.Culture)) {
					cultures.Add(settlement.Culture);
				}
			}

			List<FactionCulture> factionCultures = new List<FactionCulture>();
			foreach (FactionCulture factionCulture in FactionCulture.factionCultures) {
				if (factionCultures.Contains(factionCulture)) continue;
				foreach (CultureObject cultureObject in cultures) {
					if (factionCulture.acceptedCultures.Contains(cultureObject) && !factionCultures.Contains(factionCulture)) {
						factionCultures.Add(factionCulture);
						break;
					}
				}
			}
			foreach (FactionCulture selectedFactionCulture in factionCultures) yield return selectedFactionCulture.culture;
		}
	}
}
