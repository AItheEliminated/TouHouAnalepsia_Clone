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
	class BeastUnit : Ballista, ISpawnable, IPathHolder, IPrimarySiegeWeapon, IMoveableSiegeWeapon {
		private string _pathEntityName;
		public bool GhostEntityMove = true;
		public int GateNavMeshId = 0;
		private FormationAI.BehaviorSide _weaponSide;
		private MissionObject _targetWallSegment;
		private bool _isGhostMovementOn;

		public string PathEntity {
			get {
				return this._pathEntityName;
			}
		}

		public bool EditorGhostEntityMove {
			get {
				return this.GhostEntityMove;
			}
		}

		public float SiegeWeaponPriority {
			get {
				return 15f;
			}
		}

		// Token: 0x1700076E RID: 1902
		// (get) Token: 0x06002860 RID: 10336 RVA: 0x0009CC5F File Offset: 0x0009AE5F
		public int OverTheWallNavMeshID {
			get {
				return this.GetGateNavMeshId();
			}
		}

		// Token: 0x1700076F RID: 1903
		// (get) Token: 0x06002861 RID: 10337 RVA: 0x0009CC67 File Offset: 0x0009AE67
		public bool HoldLadders {
			get {
				return !this.MovementComponent.HasArrivedAtTarget;
			}
		}

		// Token: 0x17000770 RID: 1904
		// (get) Token: 0x06002862 RID: 10338 RVA: 0x0009CC77 File Offset: 0x0009AE77
		public bool SendLadders {
			get {
				return this.MovementComponent.HasArrivedAtTarget;
			}
		}

		// Token: 0x06002863 RID: 10339 RVA: 0x0009CC84 File Offset: 0x0009AE84
		public int GetGateNavMeshId() {
			if (this.GateNavMeshId != 0) {
				return this.GateNavMeshId;
			}
			if (this.DynamicNavmeshIdStart == 0) {
				return 0;
			}
			return this.DynamicNavmeshIdStart + 3;
		}

		public MissionObject TargetCastlePosition {
			get {
				return this._targetWallSegment;
			}
		}

		public FormationAI.BehaviorSide WeaponSide {
			get {
				return this._weaponSide;
			}
		}

		public SiegeWeaponMovementComponent MovementComponent { get; private set; }

		MatrixFrame IMoveableSiegeWeapon.GetInitialFrame() {
			throw new NotImplementedException();
		}

		public bool HasCompletedAction() {
			return this.IsDeactivated && !base.IsDestroyed;
		}

		void IMoveableSiegeWeapon.HighlightPath() {
			throw new NotImplementedException();
		}

		public void SwitchGhostEntityMovementMode(bool isGhostEnabled) {
			if (isGhostEnabled) {
				if (!this._isGhostMovementOn) {
					base.RemoveComponent(this.MovementComponent);
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
		private void AddRegularMovementComponent() {
			this.MovementComponent = new SiegeWeaponMovementComponent {
				PathEntityName = this._pathEntityName,
				MinSpeed = this.MinSpeed,
				MaxSpeed = this.MaxSpeed,
				MainObject = this,
				WheelDiameter = this.WheelDiameter,
				NavMeshIdToDisableOnDestination = this.NavMeshIdToDisableOnDestination,
				MovementSoundCodeID = SoundEvent.GetEventIdFromString("event:/mission/siege/siegetower/move"),
				GhostEntitySpeedMultiplier = this.GhostEntitySpeedMultiplier
			};
			base.AddComponent(this.MovementComponent);
		}
		public float WheelDiameter = 1.3f;
		public float MinSpeed = 0.5f;
		public float MaxSpeed = 1f;
		public int NavMeshIdToDisableOnDestination = -1;
		public float GhostEntitySpeedMultiplier = 1f;

		protected override void OnInit() {
			this.CleanState = base.GameEntity.GetFirstChildEntityWithTag("body");
			base.OnInit();
			base.DestructionComponent.BattleSide = BattleSideEnum.Attacker;
			this.AddRegularMovementComponent();
		}
		private GameEntity CleanState {
			get {
				if (this._cleanState == null) {
					return base.GameEntity;
				}
				return this._cleanState;
			}
			set {
				this._cleanState = value;
			}
		}
		private GameEntity _cleanState;
	}
}
