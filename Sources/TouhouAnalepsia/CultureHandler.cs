using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace TouhouAnalepsia {
	class CultureHandler {
		public static bool isCultureAccepted(string idCultureObject, string idFactionCulture) {
			foreach(FactionCulture factionCulture in FactionCulture.factionCultures) {
				if (factionCulture.culture.StringId.Equals(idFactionCulture)) return isCultureAccepted(idCultureObject,factionCulture);
			}

			return false;
		}
		public static bool isCultureAccepted(string idCultureObject, FactionCulture factionCulture) {
			foreach(CultureObject cultureObject in factionCulture.acceptedCultures) {
				if (cultureObject.StringId.Equals(idCultureObject)) return isCultureAccepted(cultureObject, factionCulture);
			}

			return false;
		}
		public static bool isCultureAccepted(CultureObject cultureObject, string idFactionCulture) {
			foreach (FactionCulture factionCulture in FactionCulture.factionCultures) {
				if (factionCulture.culture.StringId.Equals(idFactionCulture)) return isCultureAccepted(cultureObject, factionCulture);
			}

			return false;
		}
		public static bool isCultureAccepted(CultureObject cultureObject, FactionCulture factionCulture) {
			return factionCulture.acceptedCultures.Contains(cultureObject);
		}

		public static CultureObject getCultureWithID(string id) {
			List<CultureObject> allCultures = new List<CultureObject>();
			Game.Current.ObjectManager.GetAllInstancesOfObjectType<CultureObject>(ref allCultures);

			return (from x in allCultures
					where x.StringId.Equals(id)
					select x).First() ;
		}

		public static bool isHuman(CultureObject culture) {
			return isHuman(culture.StringId);
		}
		public static bool isHuman(string cultureID) {
			if (cultureID.Equals("human")) return true; 
			return false;
		}
		public static bool isYoukai(CultureObject culture) {
			return isYoukai(culture.StringId);
		}
		public static bool isYoukai(string cultureID) {
			if (cultureID.Equals("youkai")) return true;
			if (cultureID.Equals("fairy")) return true;
			return false;
		}
	}
}
