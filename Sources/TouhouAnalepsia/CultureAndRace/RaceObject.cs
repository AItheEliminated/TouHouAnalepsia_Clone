using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TouhouAnalepsia.CultureAndRace {
	class RaceObject : MBObjectBase {
		public TextObject Name { get; private set; }
		public BasicCultureObject Culture { get; set; }

		public List<RaceTransformationPath> TransformationPaths { get; private set; }

		public override void Deserialize(MBObjectManager objectManager, XmlNode node) {
			base.Deserialize(objectManager, node);
			Name = new TextObject(node.Attributes["name"].Value, null);
			Culture = objectManager.ReadObjectReferenceFromXml<BasicCultureObject>("culture", node);

			if (TransformationPaths == null) TransformationPaths = new List<RaceTransformationPath>();
			foreach (XmlNode child in node.ChildNodes) {
				switch(child.Name) {
					case "TransformationPath":
						int reknown;
						TransformationPaths.Add(new RaceTransformationPath(child.Attributes["result"].Value,
												int.TryParse(child.Attributes["reknown"]?.Value, out reknown) ? reknown : 0));
						break;
				}
			}
		}

		public override string ToString() {
			return base.ToString();
		}
	}
	public struct RaceTransformationPath {
		public RaceTransformationPath(string result, int reknown = 0) {
			this.result = result;
			this.reknown = reknown;
		}

		public string result { get; private set; }
		public int reknown { get; private set; }
	}
}
