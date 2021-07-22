using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Schema;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;
using TouhouAnalepsia.Utils.Inits;

namespace TouhouAnalepsia.Utils
{
	public static class ObjectManagerPatch
	{
		public static void AddCustomXsd(XmlSchemaSet schema, string vanillaXsdPath)
		{
			string xsdName = null;
			if (vanillaXsdPath.Contains("Items"))
			{
				xsdName = "Items";
			}

			if (!string.IsNullOrWhiteSpace(xsdName))
			{
				var path = ModuleHelper.GetModuleFullPath(SubModule.MODULE_ID) +
				           "/XmlSchemas/" + xsdName + ".xsd";
				try
				{
					using (var xmlTextReader = new XmlTextReader(path))
					{
						schema.Add(null, xmlTextReader);
					}
				}
				catch (Exception)
				{
					// TODO: use DebugUtil
					Debug.Print($"Cannot find custom xsd file: {path}", 0, Debug.DebugColor.Red, 17592186044416UL);
				}
			}
		}

		[HarmonyPatch(typeof(MBObjectManager))]
		static class MBObjectManagerPatch
		{
			[HarmonyPatch("GetMergedXmlForNative", MethodType.Normal)]
			[HarmonyPostfix]
			static void PatchGetMergedXmlForNative(ref XmlDocument __result)
			{
				XmlNode root = __result;

				if (root.FirstChild?.Name == "base")
					root = root.FirstChild;

				XmlElement firstNode = null;
				foreach (XmlNode childNode in root.ChildNodes)
				{
					var childEle = childNode as XmlElement;
					if (childEle != null)
					{
						if (firstNode == null)
						{
							firstNode = childEle;
							continue;
						}

						if (childEle.Name == firstNode.Name)
						{
							foreach (XmlNode eleChildNode in childEle.ChildNodes)
							{
								firstNode.AppendChild(eleChildNode);
							}

							childEle.RemoveAll();
						}
					}
				}

				if (firstNode.Name == "skins")
				{
					var nodes = firstNode.SelectNodes("//skin[@name=\'woman\']");
					foreach (XmlNode node in nodes)
					{
						/*node.SelectSingleNode("");
						XmlAttribute attribute = node as XmlAttribute;
						if (attribute != null)
						{
							var color = Color.ConvertStringToColor(attribute.Value);
							var parent = attribute.OwnerElement;
							parent.RemoveAttributeNode(attribute);
							parent.SetAttribute("point", string.Format(CultureInfo.InvariantCulture,
								"{0:0.000}, {1:0.000}, {2:0.000}", color.Red, color.Green, color.Blue));
						}*/
					}
				}
			}

			[HarmonyPatch("LoadXmlWithValidation", MethodType.Normal)]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> PatchLoadXmlWithValidation(
				IEnumerable<CodeInstruction> instructions)
			{
				foreach (var code in instructions)
				{
					yield return code;
					if (code.opcode == OpCodes.Callvirt && 
					    (code.operand as MethodInfo)?.Name == "Add" &&
					    (code.operand as MethodInfo)?.GetParameters().Length == 2)
					{
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Ldarg_1);
						yield return new CodeInstruction(OpCodes.Call,
							AccessTools.Method(typeof(ObjectManagerPatch), nameof(AddCustomXsd)));
					}
				}
			}
		}
	}
}