using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TouhouAnalepsia {
	class Logger {
		public static void Log(string s) { //How to use Debug.Print?
			InformationManager.DisplayMessage(new InformationMessage(s));
			Debug.Print(s);
		}
	}
}