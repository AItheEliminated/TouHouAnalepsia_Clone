using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia {
	class Ability {
		public double hpStatic { get; private set; }
		public double hpScaled { get; private set; }
		public int hpHealedPerMinute { get; private set; }
		public double damageStatic { get; private set; } //Non-working
		public double damageScaled { get; private set; } //Non-working
		public double movementSpeed { get; private set; } //Non-working

		public Ability() {
			hpStatic = 0.0;
			hpScaled = 1.0;
			hpHealedPerMinute = 0;

			damageStatic = 0.0;
			damageScaled = 1.0;

			movementSpeed = 1.0;
		}
		public Ability(double hpStatic, double hpScaled, int hpHealedPerMinute, double damageStatic, double damageScaled, double movementSpeed) {
			this.hpStatic = hpStatic;
			this.hpScaled = hpScaled;
			this.hpHealedPerMinute = hpHealedPerMinute;

			this.damageStatic = damageStatic;
			this.damageScaled = damageScaled;

			this.movementSpeed = movementSpeed;
		}
		public static Ability getAbilityFromNode(XmlNode node) {
			if (node.Name != "ability") return null;//TODO Erronous input handle
			//node.Attributes["    "].Value;
			return null;
		}

		//Only use for simple additions, else use AbilitySet
		public Ability add(Ability modAbility) {
			Ability newAbility = this; //new Ability(hpStatic, hpScaled, hpHealedPerMinute, damageStatic, damageScaled, movementSpeed);

			newAbility.flagEnable.AddRange(modAbility.flagEnable);
			newAbility.flagDisable.AddRange(modAbility.flagDisable);

			newAbility.hpStatic += modAbility.hpStatic;
			newAbility.hpScaled *= modAbility.hpScaled;
			newAbility.hpHealedPerMinute += modAbility.hpHealedPerMinute;

			newAbility.damageStatic += modAbility.damageStatic;
			newAbility.damageScaled *= modAbility.damageScaled;

			newAbility.movementSpeed *= modAbility.movementSpeed;
			return newAbility;
		}

		private readonly List<List<string>> flagEnable = new List<List<string>>();
		private readonly List<List<string>> flagDisable = new List<List<string>>();
		public bool IsEnabled(List<string> effects) {
			bool result = false;
			if (!flagEnable.IsEmpty()) { 
				foreach (List<string> flagset in flagEnable) {
					if (effects.Intersect(flagset).Count() == flagset.Count()) result = true; //Checks if at least one flagEnable flagset is true
				}
			} else {
				result = true; // Default to true if no enable flags
			}
			if(!flagDisable.IsEmpty()) {
				foreach (List<string> flagset in flagDisable) {
					if (effects.Intersect(flagset).Count() == flagset.Count()) result = false; //checks that no flagDisable flagset is true
				}
			}
			return result;
		}

		public static Ability calculateAbility(Agent agent) {
			Ability newAbility = new Ability();

			if (!agent.IsHuman) return newAbility; // Insures agent is Human, that is, not a horse. Support for non-human creatures is something to be done for the future... Maybe.

			if (agent.Origin==null) {
				InformationManager.DisplayMessage(new InformationMessage("Error at calculateAbility! Origin is Null!"));
				return newAbility;
			} //else InformationManager.DisplayMessage(new InformationMessage("Sucess at calculateAbility! Origin is not Null!"));

			newAbility = newAbility.add(abilityFromLevel(agent.Origin.Troop.Level))
								   .add(abilityFromCulture(agent.Origin.Troop.Culture));
			return newAbility;
		}
		private static Ability abilityFromLevel(int level) {
			Ability newAbility = new Ability();
			newAbility.hpScaled *= Math.Max(1.0, (Math.Pow(1.07, level) - Math.Sqrt(level)/10));
			//newAbility.hpScaled *= 2;
			return newAbility;
		}
		private static Ability abilityFromCulture(BasicCultureObject culture) {
			Ability newAbility = new Ability();
			newAbility.hpHealedPerMinute += 10;

			//if (culture.IsBandit) newAbility.movementSpeed += 5; //Currently movement speed increase seems impossible... More work is to be done on this.
			//newAbility.movementSpeed += 5;
			return newAbility;
		}
	}
}
