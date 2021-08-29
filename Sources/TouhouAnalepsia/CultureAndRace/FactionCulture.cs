using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace TouhouAnalepsia {
	class FactionCulture {
		//TODO Handle LordDefectionCampaignBehavior and MakeOutlawFactionsEnemyToKingdomFactions

		public CultureObject culture { get; private set; }
		public readonly List<CultureObject> acceptedCultures = new List<CultureObject>();

		public static FactionCulture getFactionCultureByID(string id) {
			foreach (FactionCulture factionCulture in factionCultures) {
				if (id.Equals(factionCulture.culture.StringId)) return factionCulture;
			}
			if (id.Equals("fc_human")) {
				List<CultureObject> allCultures = new List<CultureObject>();
				Game.Current.ObjectManager.GetAllInstancesOfObjectType<CultureObject>(ref allCultures);
				foreach (FactionCulture factionCulture in factionCultures) {
					if (factionCulture.culture == allCultures.First()) return factionCulture;
				}

				return new FactionCulture(allCultures.First(), allCultures);
			}
			return getFactionCultureByID("fc_human");
		}
		public static bool IsFactionCulture(string id) {
			return id.StartsWith("fc_");
		}

		public bool IsAcceptedCulture(CultureObject culture) {
			return acceptedCultures.Contains(culture);
		}

		public FactionCulture(CultureObject factionCultureObject, List<CultureObject> acceptedCultures) {
			this.culture = factionCultureObject;
			this.acceptedCultures.AddRange(acceptedCultures);

			factionCultures.Add(this);
		}

		public static void initializeFactionCultures() {
			List<CultureObject> allCultures = new List<CultureObject>();
			Game.Current.ObjectManager.GetAllInstancesOfObjectType<CultureObject>(ref allCultures);
			IEnumerable<CultureObject> filteredCultures = from x in allCultures where x.StringId.Split('_')[0].Equals("fc") select x;

			foreach (CultureObject cultureObject in filteredCultures) initializeFactionCultureByCulture(cultureObject);
		}

		private static FactionCulture initializeFactionCultureByCulture(CultureObject culture) {
			List<CultureObject> acceptedCultures = new List<CultureObject>();
			switch (culture.StringId) {
				default:
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_hakurei_shrine":
					acceptedCultures.Add(CultureHandler.getCultureWithID("human"));
					acceptedCultures.Add(CultureHandler.getCultureWithID("fairy"));
					break;
				case "fc_human_city":
					acceptedCultures.Add(CultureHandler.getCultureWithID("human"));
					break;
				case "fc_eientei":
					acceptedCultures.Add(CultureHandler.getCultureWithID("human"));
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_scarlet_empire":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					acceptedCultures.Add(CultureHandler.getCultureWithID("fairy"));
					break;
				case "fc_misty_empire":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					acceptedCultures.Add(CultureHandler.getCultureWithID("fairy"));
					break;
				case "fc_anokuni":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_kyuutaki":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_underworld":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_netherworld":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_shogunate":
					acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
				case "fc_player_culture":
					if (Hero.MainHero != null) acceptedCultures.Add(Hero.MainHero.Culture);
					else acceptedCultures.Add(CultureHandler.getCultureWithID("youkai"));
					break;
			}
			return new FactionCulture(culture, acceptedCultures);
		}

		/* //TODO implement saving system for dynamic accepted cultures
		public class FactionBehavior : CampaignBehaviorBase {
			public override void RegisterEvents() {
			}

			public override void SyncData(IDataStore dataStore) {
				foreach (FactionCulture factionCulture in factionCultures) {
					for (int i = 0; i < factionCulture.acceptedCultures.Count(); i++) {
						string accepted_culture = factionCulture.acceptedCultures.ElementAt<CultureObject>(i).StringId;
						dataStore.SyncData<string>(factionCulture.culture.StringId + ".acceptedCulture" + i, ref accepted_culture);
					}
				}
			}
		}
		*/

		public static List<FactionCulture> factionCultures { private set; get; } = new List<FactionCulture>();
	}
}
