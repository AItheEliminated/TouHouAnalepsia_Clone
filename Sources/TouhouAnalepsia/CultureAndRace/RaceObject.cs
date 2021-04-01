using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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
						List<RaceTransformationTraitInfo> traitInfo = new List<RaceTransformationTraitInfo>();
						foreach (XmlNode subchild in child.ChildNodes) {
							switch(subchild.Name) {
								case "Traits":
									foreach(XmlNode trait in subchild.ChildNodes) {
										traitInfo.Add(new RaceTransformationTraitInfo(trait["id"].Value, trait["minimum"]?.Value, trait["maximum"]?.Value));
									}
									break;
							}
						}

						string trait_requirements = child.Attributes["requirements"]?.Value;//TODO handle if null

						const string class_name = "CharacterTraits";
						Expression<Func<CharacterTraits, bool>> expression = null;

						if (trait_requirements != null) {
							foreach (RaceTransformationTraitInfo info in traitInfo) {
								if (info.minimum != null) trait_requirements.Replace(info.trait, info.minimum + " < " + info.trait);
								if (info.maximum != null) trait_requirements.Replace(info.trait, info.trait + " < " + info.maximum);

								trait_requirements = trait_requirements.Replace(info.trait, class_name + info.trait);
							}
							expression = DynamicExpressionParser.ParseLambda<CharacterTraits, bool>(new ParsingConfig(), true, trait_requirements);
							//expression.Compile().DynamicInvoke();
						}

						TransformationPaths.Add(new RaceTransformationPath(child.Attributes["result"].Value,
												int.TryParse(child.Attributes["reknown"]?.Value, out reknown) ? reknown : 0, 
												expression));
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
		public RaceTransformationPath(string result, int reknown = 0, Expression<Func<CharacterTraits, bool>> trait_requirements = null) {
			this.result = result;
			this.reknown = reknown;
			this.trait_requirements = trait_requirements;
		}

		public string result { get; private set; }
		public int reknown { get; private set; }

		public Expression<Func<CharacterTraits, bool>> trait_requirements;
	}
	public struct RaceTransformationTraitInfo {
		public string trait;
		public string minimum;
		public string maximum;

		public RaceTransformationTraitInfo(string trait, string minimum, string maximum) {
			this.trait = trait;
			this.minimum = minimum;
			this.maximum = maximum;
		}
	}
}
