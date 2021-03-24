using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TouhouAnalepsia
{
    public static class XmlHelper
    {
		public static T GetSettings<T>(string fileName)
		{
			string filePth =
				Path.Combine(
					new string[] {
						BasePath.Name,
						"Modules",
						SubModule.ModNamePath,
						"settings",
						fileName + ".xml"
					});

			T result;

			try
			{
				using (XmlReader xmlReader = XmlReader.Create(filePth))
				{
					XmlRootAttribute xmlRootAttribute = new XmlRootAttribute();
					xmlRootAttribute.ElementName = SubModule.ModNamePath;
					xmlRootAttribute.IsNullable = true;
					if (xmlReader.MoveToContent() != XmlNodeType.Element)
					{
						result = default(T);
					}
					else if (xmlReader.Name != xmlRootAttribute.ElementName)
					{
						result = default(T);
					}
					else
					{
						result = (T)((object)new XmlSerializer(typeof(T), xmlRootAttribute).Deserialize(xmlReader));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogException("Failed to load settings", ex);
				result = default(T);
			}
			return result;
		}
	}
}
