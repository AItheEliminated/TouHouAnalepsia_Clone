using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.ObjectSystem;

namespace TouhouAnalepsia {
	class Race : MBObjectBase {
		public string name { get; private set; }
		public bool isImmortal { get; private set; }
		public Ability ability { get; private set; }

		//Initialization stuff

		public override void Deserialize(MBObjectManager objectManager, XmlNode node) {
			base.Deserialize(objectManager, node);
			this.name = node.Attributes["name"].Value;
			this.isImmortal = (node.Attributes["is_immortal"] != null && Convert.ToBoolean(node.Attributes["is_immortal"].Value));

			races.Add(this);
		}

		//Initialization stuff end

		public Race(string name, bool isImmortal, Ability ability) {
			this.name = name;
			this.isImmortal = isImmortal;
			this.ability = ability;
			if (this.ability == null) this.ability = new Ability();


			if (!isValid()) {
				//TODO error handling for if it is not valid.
			} else races.Add(this);
		}

		public void modifyRace(Ability modAbility) {
			if (ability != null) {
				ability = ability.add(modAbility);
			} else {
				ability = modAbility;
			}

			if (ability == null) ability = new Ability();
		}

		public bool isValid() {
			bool result = false;
			foreach (Race race in races) {
				if (race != this) result |= race.name.Equals(this.name);
			}
			//result |= ;
			return !result;
		}

		public static List<Race> races { get; } = new List<Race>();
	}
}
