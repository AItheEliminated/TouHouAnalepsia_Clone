using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia {
	[Obsolete]
	class AbilityOld {
		public double hpStatic { get; private set; }
		public double hpScaled { get; private set; }
		public int hpHealedPerMinute { get; private set; }
		public double damageStatic { get; private set; } //Non-working
		public double damageScaled { get; private set; } //Non-working
		public double movementSpeed { get; private set; }

		public AbilityOld() {
			hpStatic = 0.0;
			hpScaled = 1.0;
			hpHealedPerMinute = 0;

			damageStatic = 0.0;
			damageScaled = 1.0;

			movementSpeed = 1.0;
		}
		public AbilityOld(double hpStatic, double hpScaled, int hpHealedPerMinute, double damageStatic, double damageScaled, double movementSpeed) {
			this.hpStatic = hpStatic;
			this.hpScaled = hpScaled;
			this.hpHealedPerMinute = hpHealedPerMinute;

			this.damageStatic = damageStatic;
			this.damageScaled = damageScaled;

			this.movementSpeed = movementSpeed;
		}

		//Only use for simple additions, else use AbilitySet
		public AbilityOld add(AbilityOld modAbility) {
			AbilityOld newAbility = this; //new Ability(hpStatic, hpScaled, hpHealedPerMinute, damageStatic, damageScaled, movementSpeed);

			newAbility.hpStatic += modAbility.hpStatic;
			newAbility.hpScaled *= modAbility.hpScaled;
			newAbility.hpHealedPerMinute += modAbility.hpHealedPerMinute;

			newAbility.damageStatic += modAbility.damageStatic;
			newAbility.damageScaled *= modAbility.damageScaled;

			newAbility.movementSpeed *= modAbility.movementSpeed;
			return newAbility;
		}

		public static AbilityOld calculateAbility(Agent agent) {
			AbilityOld newAbility = new AbilityOld();

			if (!agent.IsHuman) return newAbility; // Insures agent is Human, that is, not a horse. Support for non-human creatures is something to be done for the future... Maybe.

			if (agent.Origin==null) {
				Logger.Log("Error at calculateAbility! Origin is Null!");
				return newAbility;
			}
			Hero hero = null;
			if (agent?.Character is CharacterObject charObj) hero = charObj.HeroObject;

			if (hero != null) { //To allow for more potential interactions with lords and such, access to their Hero object should be useful
				newAbility = newAbility.add(abilityFromLevel(hero.Level))
									   .add(abilityFromCulture(hero.Culture));

				if(hero==Hero.MainHero) { //Special Bonuses for the Main Hero
					newAbility.add(new AbilityOld() { 
						movementSpeed = 2.0,
						hpScaled = 2.0
					});
				}
			} else {
				newAbility = newAbility.add(abilityFromLevel(agent.Origin.Troop.Level))
									   .add(abilityFromCulture(agent.Origin.Troop.Culture));
			}

			return newAbility;
		}
		private static AbilityOld abilityFromLevel(int level) {
			AbilityOld newAbility = new AbilityOld();
			newAbility.hpScaled *= Math.Max(1.0, (Math.Pow(1.07, level) - Math.Sqrt(level)/10));
			return newAbility;
		}
		private static AbilityOld abilityFromCulture(BasicCultureObject culture) {
			AbilityOld newAbility = new AbilityOld();
			
			if (culture.StringId.Equals("fairy")) newAbility.hpHealedPerMinute += 10;

			if (culture.StringId.Equals("youkai")) newAbility.hpScaled *= 2;

			//if (culture.IsBandit) newAbility.movementSpeed += 5; //Currently movement speed increase seems impossible... More work is to be done on this.
			//newAbility.movementSpeed += 5;
			return newAbility;
		}
	}
}
