using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouAnalepsia {

	//To be worked on in the future, for now leave it as it is.
	//Purpose is to allow more complex enable and disable conditions for abilities. i.e. vampire during night, but weaker then normal during day

	class AbilitySet {
		private readonly List<AbilityOld> abilities = new List<AbilityOld>();

		public void addAbility(AbilityOld ability) {
			abilities.Add(ability);
		}
		public bool removeAbility(AbilityOld ability) {
			return abilities.Remove(ability);
		}
		public List<AbilityOld> getEnabledAbilities(List<string> effects) {
			List<AbilityOld> enabledAbilities = new List<AbilityOld>();

			foreach (AbilityOld ability in abilities) {
				if (ability.IsEnabled(effects)) enabledAbilities.Add(ability);
			}

			return enabledAbilities;
		}
	}
}
