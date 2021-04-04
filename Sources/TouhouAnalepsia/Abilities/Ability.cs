using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TouhouAnalepsia.CultureAndRace;

namespace TouhouAnalepsia.Abilities {
	class Ability : MBObjectBase {
		public double hpStatic { get; private set; }
		public double hpScaled { get; private set; }
		public int hpHealedPerMinute { get; private set; }
		public double damageStatic { get; private set; } //Non-working
		public double damageScaled { get; private set; } //Non-working
		public double movementSpeed { get; private set; }

		public static Dictionary<string, List<Tuple<string, Delegate>>> conditions = new Dictionary<string, List<Tuple<string, Delegate>>>();

		public Ability() : base(Guid.NewGuid().ToString()) {
			Initialize();
		}

		public static Ability combine(params Ability[] abilities) {
			Ability combined_ability = new Ability();
			foreach(Ability ability in abilities) {
				combined_ability.hpStatic += ability.hpStatic;
				combined_ability.hpScaled *= ability.hpScaled;
				combined_ability.hpHealedPerMinute += ability.hpHealedPerMinute;
				combined_ability.damageStatic += ability.damageStatic;
				combined_ability.damageScaled *= ability.damageScaled;
				combined_ability.movementSpeed *= ability.movementSpeed;
			}

			return combined_ability;
		}

		private static IEnumerable<Ability> inner_calculateAbility(Agent agent) {
			if (!agent.IsHuman) yield break; // Insures agent is Human, that is, not a horse. Support for non-human creatures is something to be done for the future... Maybe.

			
			if (agent.Origin == null) {
				Logger.Log("Error at calculateAbility! Origin is Null!");
				yield break;
			}

			Hero hero = null;
			if (agent?.Character is CharacterObject charObj) hero = charObj.HeroObject;

			BasicCultureObject culture = hero?.Culture;
			if (culture == null) culture = agent.Origin.Troop.Culture; //TODO agent.Character.Level

			RaceObject race = null;

			if (hero != null) { //To allow for more potential interactions with lords and such, access to their Hero object should be useful
				yield return abilityFromLevel(hero.Level);

				if (hero == Hero.MainHero) { //Special Bonuses for the Main Hero
					yield return new Ability() {
						movementSpeed = 2.0,
						hpScaled = 2.0
					};
				}
			} else {
				yield return abilityFromLevel(agent.Origin.Troop.Level); //TODO agent.Character.Level;
			}

			List<Ability> all_abilities = new List<Ability>();
			Game.Current.ObjectManager.GetAllInstancesOfObjectType(ref all_abilities);

			foreach (Ability ability in all_abilities) {
				List<Tuple<string,Delegate>> requirements = null;
				if(conditions.ContainsKey(ability.StringId)) requirements = conditions[ability.StringId];
				//Logger.Log("Checking if ability is valid...");

				if (requirements == null || requirements.IsEmpty()) {
					//yield return ability;
					//Logger.Log("Added Ability effect!");
					continue;
				}

				foreach (Tuple<string, Delegate> @delegate in requirements) {
					if (@delegate.Item1.ToLower().Contains("hero") && hero == null) continue;
					if (@delegate.Item1.ToLower().Contains("agent") && agent == null) continue;
					if (@delegate.Item1.ToLower().Contains("culture") && culture == null) continue;
					if (@delegate.Item1.ToLower().Contains("race") && race == null) continue;

					if ((bool)@delegate.Item2.DynamicInvoke(hero, agent, culture, race)) {
						yield return ability;
						//Logger.Log("Added Ability effect!");
						break;
					}
				}
			}
		}
		private static Ability abilityFromLevel(int level) {
			Ability newAbility = new Ability();
			newAbility.hpScaled *= Math.Max(1.0, (Math.Pow(1.07, level) - Math.Sqrt(level) / 10));
			return newAbility;
		}

		public static Ability calculateAbility(Agent agent) {
			return combine(inner_calculateAbility(agent).ToArray());
		}

		public override void Deserialize(MBObjectManager objectManager, XmlNode node) {
			base.Deserialize(objectManager, node);

			if (node.Attributes["hpStatic"]?.Value != null) hpStatic = double.Parse(node.Attributes["hpStatic"].Value);
			if (node.Attributes["hpScaled"]?.Value != null) hpScaled = double.Parse(node.Attributes["hpScaled"].Value);
			if (node.Attributes["hpHealedPerMinute"]?.Value != null) hpHealedPerMinute = (int)double.Parse(node.Attributes["hpHealedPerMinute"].Value);
			if (node.Attributes["damageStatic"]?.Value != null) damageStatic = double.Parse(node.Attributes["damageStatic"].Value);
			if (node.Attributes["damageScaled"]?.Value != null) damageScaled = double.Parse(node.Attributes["damageScaled"].Value);
			if (node.Attributes["movementSpeed"]?.Value != null) movementSpeed = double.Parse(node.Attributes["movementSpeed"].Value);

			foreach (XmlNode child in node.ChildNodes) {
				if (child.NodeType.Equals(XmlNodeType.Comment)) continue;
				switch (child.Name) {
					case "Condition":
						if (child.Attributes["condition"]?.Value == null) break;

						ParameterExpression[] symbols = new[] {
							Expression.Parameter(typeof(Hero), "Hero"),
							Expression.Parameter(typeof(Agent), "Agent"),
							Expression.Parameter(typeof(BasicCultureObject), "Culture"),
							Expression.Parameter(typeof(RaceObject), "Race")
						};

						Expression body = new ExpressionParser(symbols, child.Attributes["condition"].Value, symbols, new ParsingConfig()).Parse(typeof(bool));

						Delegate compiled = Expression.Lambda(body, symbols).Compile();

						if (!conditions.ContainsKey(StringId)) conditions[StringId] = new List<Tuple<string, Delegate>>();
						conditions[StringId].Add(new Tuple<string, Delegate>(child.Attributes["uses"]?.Value, compiled));

						break;
				}
			}
		}

		public override void Initialize() {
			base.Initialize();
			hpStatic = 0.0;
			hpScaled = 1.0;
			hpHealedPerMinute = 0;

			damageStatic = 0.0;
			damageScaled = 1.0;

			movementSpeed = 1.0;
		}
	}
}
