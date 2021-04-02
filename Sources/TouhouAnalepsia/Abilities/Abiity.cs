using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ObjectSystem;

namespace TouhouAnalepsia.Abilities {
	class Abiity : MBObjectBase {
		public double hpStatic { get; private set; }
		public double hpScaled { get; private set; }
		public int hpHealedPerMinute { get; private set; }
		public double damageStatic { get; private set; } //Non-working
		public double damageScaled { get; private set; } //Non-working
		public double movementSpeed { get; private set; }

		public override void Deserialize(MBObjectManager objectManager, XmlNode node) {
			base.Deserialize(objectManager, node);
		}

		public override void Initialize() {
			hpStatic = 0.0;
			hpScaled = 1.0;
			hpHealedPerMinute = 0;

			damageStatic = 0.0;
			damageScaled = 1.0;

			movementSpeed = 1.0;
			base.Initialize();
		}
	}
}
