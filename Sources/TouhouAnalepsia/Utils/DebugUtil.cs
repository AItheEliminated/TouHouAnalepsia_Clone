using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TouhouAnalepsia.Utils
{
	public static class DebugUtil
	{
		public enum MessageBoxIcon : int
		{
			None = 0,
			Warning = 2,
			Error = 4,
			Information = 128,
			Question = 256
		}

		public enum MessageBoxButton : int
		{
			Ok = 0,
			Ok_Cancel = 8,
			Retry_Cancel = 16,
			Yes_No = 32,
			Yes_No_Cancel = 64,
			Halt_Retry_Ignore = 1024, // It seems that this option will lock the icon as Error. Same below.
			Halt_Retry_Ignore_Help = 2048,
		}

		public static void ShowErrorMsg(string msg, string title = "Error")
		{
			Debug.ShowMessageBox(msg, title, (int) MessageBoxIcon.Error);
		}

		public static void ShowInfoMsg(string msg, string title = "Info")
		{
			Debug.ShowMessageBox(msg, title, (int) MessageBoxIcon.Information);
		}
	}
}
