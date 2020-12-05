using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace TouhouAnalepsia {
	class CompanionHandler {
		public static bool AddStaticCompanion(Hero companion) {
			if (companion == null) return false;

			companion.HasMet = true;

			companion.ChangeState(Hero.CharacterStates.Active);

			AddCompanionAction.Apply(Clan.PlayerClan, companion);
			AddHeroToPartyAction.Apply(companion, MobileParty.MainParty, true);

			return true;
		}
		public static bool AddStaticCompanion(string id) {
			return AddStaticCompanion(Hero.FindFirst((Hero x) => x.StringId.Equals(id)));
		}

		public class CompanionCampaignBehavior : CampaignBehaviorBase { //Temporary, to ease the companion's aquisition.
			public override void RegisterEvents() {
				CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.SettlementEntered));
			}

			public override void SyncData(IDataStore dataStore) {
			}

			int tmp=-3;

			private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero) {
				if (party == MobileParty.MainParty) {
					switch (tmp) {
						case 0:
							AddStaticCompanion("kokoro");
							break;
						case 1:
							AddStaticCompanion("sumireko");
							break;
						case 2:
							AddStaticCompanion("joon");
							break;
						case 3:
							AddStaticCompanion("shion");
							break;
						case 4:
							AddStaticCompanion("joutouguu");
							break;
						case 5:
							AddStaticCompanion("keiki");
							break;
						case 6:
							AddStaticCompanion("clownpiece");
							break;
						case 7:
							AddStaticCompanion("junko");
							break;
					}
					tmp++;
				}
			}
		}
	}
}
