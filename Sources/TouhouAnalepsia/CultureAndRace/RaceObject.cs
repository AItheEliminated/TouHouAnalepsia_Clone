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

		public Ability ability = new Ability();

		public override void Deserialize(MBObjectManager objectManager, XmlNode node) {
			base.Deserialize(objectManager, node);
			Name = new TextObject(node.Attributes["name"].Value, null);
			Culture = objectManager.ReadObjectReferenceFromXml<BasicCultureObject>("culture", node);

			if (TransformationPaths == null) TransformationPaths = new List<RaceTransformationPath>();
			foreach (XmlNode child in node.ChildNodes) {
				switch(child.Name) {
					case "TransformationPath":
						int reknown;
						Game.Current.ObjectManager.GetObject<TraitObject>(child.Attributes["trait"]?.Value);
						foreach (XmlNode subchild in child.ChildNodes) {
							switch(subchild.Name) {

							}
						}

						TransformationPaths.Add(new RaceTransformationPath(child.Attributes["result"].Value,
												int.TryParse(child.Attributes["reknown"]?.Value, out reknown) ? reknown : 0));
						break;
					case "Ability":
						double hpStatic;
						double hpScaled;
						double hpHealedPerMinute;
						double damageStatic;
						double damageScaled;
						double movementSpeed;

						if (!double.TryParse(child.Attributes["hpStatic"]?.Value, out hpStatic)) hpStatic=0.0;
						if (!double.TryParse(child.Attributes["hpScaled"]?.Value, out hpScaled)) hpScaled=1.0;
						if (!double.TryParse(child.Attributes["hpHealedPerMinute"]?.Value, out hpHealedPerMinute)) hpHealedPerMinute=0;
						if (!double.TryParse(child.Attributes["damageStatic"]?.Value, out damageStatic)) damageStatic=0.0;
						if (!double.TryParse(child.Attributes["damageScaled"]?.Value, out damageScaled)) damageScaled=1.0;
						if (!double.TryParse(child.Attributes["movementSpeed"]?.Value, out movementSpeed)) movementSpeed=1.0;

						ability.add(new Ability(hpStatic,hpScaled,(int)hpHealedPerMinute,damageStatic,damageScaled,movementSpeed));
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
