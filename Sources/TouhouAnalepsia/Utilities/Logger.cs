using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TouhouAnalepsia {
	class Logger {
		public static void Log(string s) { //How to use Debug.Print?
			InformationManager.DisplayMessage(new InformationMessage(s));
			Debug.Print(s);
		}

        public static void WriteToFile(string text, string fileName)
        {
            string filePath =
                Path.Combine(BasePath.Name, "Modules", SubModule.ModNamePath, fileName);

            try
            {
                StreamWriter sw = new StreamWriter(filePath, true); // Open in append mode
                sw.WriteLine(TADateTime.GetLocalTime() + " : " + text);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Logging failed for " + filePath + " file");
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        public static void DisplayInGameConsole(string text)
        {
            InformationManager.DisplayMessage(
                new InformationMessage(text));
        }
        public static void DisplayInGameConsole(string text, Color color)
        {
            InformationManager.DisplayMessage(
                new InformationMessage(text, color));
        }
        public static void LogException(string text, Exception e)
        {
            string outText =
                $"({SubModule.ModName}) > {text} with exception: " +
                ((e == null) ? "null" : e.ToString());

            DisplayInGameConsole(outText);
            WriteToFile(outText, "Exception.txt");
        }
        public static void LogException(string text)
        {
            string outText = $"({SubModule.ModName}) > {text}";

            DisplayInGameConsole(outText);
            WriteToFile(outText, "Exception.txt");
        }
    }
}