using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace TouhouAnalepsia {
	class RaceHelpers {
		
		//Is the Hero immortal?
		public static bool IsImmortal(Hero hero) {
			foreach (Race race in Race.races) { 
			}
			if(hero.Culture.GetName().ToString().Equals("")) return true;
			return false;
		}
		public static Race getRace(Hero hero) {
			return null;
		}
	}
}
