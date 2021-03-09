using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace TouhouAnalepsia.BeastFunction {
	class BeastUnit : Ballista, IMoveableSiegeWeapon {
		public override SiegeEngineType GetSiegeEngineType() {
			return TouhouSiegeEngineTypes.BeastUnit;
		}

		public string PathEntity {
			get {
				return this._pathEntityName;
			}
		}
		private string _pathEntityName = "Path";

		public SiegeWeaponMovementComponent MovementComponent { get; private set; }
		private void AddRegularMovementComponent() {
			this.MovementComponent = new SiegeWeaponMovementComponent {
				PathEntityName = this.PathEntity,
				MinSpeed = this.MinSpeed,
				MaxSpeed = this.MaxSpeed,
				MainObject = this,
				WheelDiameter = this.WheelDiameter,
				NavMeshIdToDisableOnDestination = this.NavMeshIdToDisableOnDestination,
				MovementSoundCodeID = SoundEvent.GetEventIdFromString("event:/mission/siege/batteringram/move"),
				GhostEntitySpeedMultiplier = this.GhostEntitySpeedMultiplier
			};
			base.AddComponent(this.MovementComponent);
		}
		internal void SetUpGhostEntity() {
			PathLastNodeFixer component = new PathLastNodeFixer {
				PathHolder = (IPathHolder)this
			};
			base.AddComponent(component);
			this.MovementComponent = new SiegeWeaponMovementComponent {
				PathEntityName = this.PathEntity,
				MainObject = this,
				GhostEntitySpeedMultiplier = this.GhostEntitySpeedMultiplier
			};
			base.AddComponent(this.MovementComponent);
			//this.MovementComponent.SetupGhostEntity(); //TODO Reimplement this if needed
		}

		protected override void OnInit() {
			this.AddRegularMovementComponent();
			base.OnInit();
		}

		public MatrixFrame GetInitialFrame() {
			if (this.MovementComponent != null) {
				//return this.MovementComponent.GetInitialFrame();
			}
			return base.GameEntity.GetGlobalFrame();
		}

		public void HighlightPath() {
			Logger.Log("Marco! (BeastUnit.HighlightPath())");
			//throw new NotImplementedException();
		}

		public void SwitchGhostEntityMovementMode(bool isGhostEnabled) {
			if (isGhostEnabled) {
				if (!this._isGhostMovementOn) {
					base.RemoveComponent(this.MovementComponent);
					this.SetUpGhostEntity();
					this.GhostEntityMove = true;
					SiegeWeaponMovementComponent component = base.GetComponent<SiegeWeaponMovementComponent>();
					component.GhostEntitySpeedMultiplier *= 3f;
					component.SetGhostVisibility(true);
				}
				this._isGhostMovementOn = true;
				return;
			}
			if (this._isGhostMovementOn) {
				base.RemoveComponent(this.MovementComponent);
				PathLastNodeFixer component2 = base.GetComponent<PathLastNodeFixer>();
				base.RemoveComponent(component2);
				this.AddRegularMovementComponent();
				this.MovementComponent.SetGhostVisibility(false);
			}
			this._isGhostMovementOn = false;
		}

		public float MinSpeed = 0.5f;
		public float MaxSpeed = 1f;
		public float WheelDiameter = 1.3f;
		public int NavMeshIdToDisableOnDestination = -1;

		private bool _isGhostMovementOn;
		public bool GhostEntityMove = true;
		public float GhostEntitySpeedMultiplier = 1f;
	}
}
