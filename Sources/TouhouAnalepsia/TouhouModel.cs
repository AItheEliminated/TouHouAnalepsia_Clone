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

namespace TouhouAnalepsia {
    class TouhouMarriageModel : MarriageModel {
		public override int MinimumMarriageAgeMale { //Vanilla Settings
			get {
				return 18;
			}
		}
		public override int MinimumMarriageAgeFemale { //Vanilla Settings
			get {
				return 18;
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
			return this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
			//}
			//return false;
		}
		public override List<Hero> GetChildrenSuitableForMarriage(Hero hero) { //Vanilla Settings
			List<Hero> list = new List<Hero>();
			foreach (Hero hero2 in hero.Children) {
				if (this.IsSuitableForMarriage(hero2)) {
					list.Add(hero2);
				}
			}
			return list;
		}
		private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n) { //Vanilla Settings
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
			yield break;
		}
		public override int GetEffectiveRelationIncrease(Hero firstHero, Hero secondHero) { //Vanilla Settings
			ExplainedNumber explainedNumber = new ExplainedNumber(20f, null);
			SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Charm, DefaultSkillEffects.CharmRelationBonus, firstHero.IsFemale ? secondHero.CharacterObject : firstHero.CharacterObject, ref explainedNumber, -1, true, 0);
			return MathF.Round(explainedNumber.ResultNumber);
		}
		public override bool IsSuitableForMarriage(Hero maidenOrSuitor) { //Vanilla Settings
			if (!maidenOrSuitor.IsAlive || maidenOrSuitor.Spouse != null || !maidenOrSuitor.IsNoble || maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate) {
				return false;
			}
			if (maidenOrSuitor.IsFemale) {
				return maidenOrSuitor.CharacterObject.Age >= (float)this.MinimumMarriageAgeFemale;
			}
			return maidenOrSuitor.CharacterObject.Age >= (float)this.MinimumMarriageAgeMale;
		}
		public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero) { //Vanilla Settings
			if (firstHero.IsHumanPlayerCharacter) {
				return firstHero.Clan;
			}
			if (secondHero.IsHumanPlayerCharacter) {
				return secondHero.Clan;
			}
			if (firstHero.Clan.Leader == firstHero) {
				return firstHero.Clan;
			}
			if (secondHero.Clan.Leader == secondHero) {
				return secondHero.Clan;
			}
			if (!firstHero.IsFemale) {
				return firstHero.Clan;
			}
			return secondHero.Clan;
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
	public class TouhouHeroDeathProbabilityCalculationModel : HeroDeathProbabilityCalculationModel { //Vanilla Settings - I'll work on this a bit more later on, so true immortals don't die.
		public override float CalculateHeroDeathProbability(Hero hero, StatExplainer explanation = null) {
			return this.CalculateHeroDeathProbabilityInternal(hero, explanation);
		}
		private float CalculateHeroDeathProbabilityInternal(Hero hero, StatExplainer explanation = null) {
			TextObject textObject = new TextObject("{=fPJKSmnq}Daily Death Probability", null);
			TextObject textObject2 = new TextObject("{=uaGEbQyJ}Daily Death Probability with Health Advise Perk", null);
			ExplainedNumber explainedNumber = new ExplainedNumber(0f, explanation, null);
			int becomeOldAge = Campaign.Current.Models.AgeModel.BecomeOldAge;
			int maxAge = Campaign.Current.Models.AgeModel.MaxAge;
			if (hero.Age > (float)becomeOldAge) {
				if (hero.Age < (float)maxAge && !hero.IsSpecial) {
					float num = 0.8f * ((hero.Age - (float)becomeOldAge) / (float)(Campaign.Current.Models.AgeModel.MaxAge - becomeOldAge));
					ExplainedNumber explainedNumber2 = new ExplainedNumber(1f, null);
					PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.HealthAdvise, hero.CharacterObject, true, ref explainedNumber2);
					float num2 = (float)(1.0 - Math.Pow((double)(1f - num), 0.011904762126505375));
					num2 *= explainedNumber2.ResultNumber;
					explainedNumber.Add(num2, (explainedNumber2.ResultNumber < 1f) ? textObject2 : textObject, null);
				} else if (hero.Age >= (float)maxAge) {
					explainedNumber.Add(1f, null, null);
				}
			}
			return explainedNumber.ResultNumber;
		}
	}
}
