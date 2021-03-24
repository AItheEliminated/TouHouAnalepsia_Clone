using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia
{
    internal class DanmakuBattleLogic : MissionLogic
    {
        private struct DanmakuMissile
        {
            public Agent ShooterAgent { get; set; }
            public MissionWeapon FiringWeapon { get; set; }
            public bool HasRigidBody { get; set; }
            public int ForcedMissileIndex { get; set; }
            public float BaseSpeed { get; set; }
            public int MissileIdx { get; set; }
        }

        private readonly List<DanmakuItem> _items = new List<DanmakuItem>();
        //private readonly List<DanmakuAmmo> _ammoItems = new List<DanmakuAmmo>(); // unused ammo parser
        private readonly List<DanmakuBattleLogic.DanmakuMissile> _danmakuMissiles =
            new List<DanmakuBattleLogic.DanmakuMissile>();
        private bool _hasInitializedFlag = false;

        private bool Initialize()
        {
            DanmakuValsList xmlVals = XmlHelper.GetSettings<DanmakuValsList>("DanmakuVals");

            if (xmlVals is null)
            {
                Logger.LogException($"Failed to import DanmakuVals settings. Check the xml for incorrect module/xml tags!");
                return false;
            }

            foreach (DanmakuVals danmakuVals in xmlVals.Data)
            {
                DanmakuItem tmpItem = new DanmakuItem();

                try
                {
                    tmpItem.Item =
                        ItemObject.All.First(x => x.StringId == danmakuVals.Id);
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to find weapon using item id: {danmakuVals.Id}", e);
                    continue;
                }

                try
                {
                    tmpItem.DanmakuNum = int.Parse(danmakuVals.DanmakuNum);
                    if (tmpItem.DanmakuNum < 1)
                    {
                        Logger.LogException(
                            $"Danmaku number must be above 1 for id: {danmakuVals.Id}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to parse number of danmaku for id: {danmakuVals.Id}", e);
                    continue;
                }

                try
                {
                    tmpItem.DanmakuDisp = float.Parse(danmakuVals.DanmakuDisp);
                    if (tmpItem.DanmakuDisp < 0.0)
                    {
                        Logger.LogException(
                            $"Danmaku dispersion must be above 0.0 for id: {danmakuVals.Id}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to parse danmaku dispersion for id: {danmakuVals.Id}", e);
                    continue;
                }

                try
                {
                    DanmakuType parsedType =
                        (DanmakuType)Enum.Parse(typeof(DanmakuType), danmakuVals.DanmakuType);
                    if (Enum.IsDefined(typeof(DanmakuType), parsedType))
                    {
                        tmpItem.Type = parsedType;
                    }
                    else
                    {
                        Logger.LogException(
                            $"Unknown {danmakuVals.DanmakuType} danmaku type for item id: {danmakuVals.Id}");
                        continue;
                    }

                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to parse danmaku type for item id: {danmakuVals.Id}", e);
                    continue;
                }

                this._items.Add(tmpItem);
            }

            return true;
        }

        // unused ammo initialization
        /*private void InitializeAmmo()
        {
            DanmakuAmmoValsList xmlVals = XmlHelper.GetSettings<DanmakuAmmoValsList>("DanmakuAmmoVals");

            if (xmlVals is null)
            {
                Logger.LogException($"Failed to import DanmakuAmmoVals settings. Check the xml for incorrect module/xml tags!");
                return false;
            }

            foreach (DanmakuAmmoVals danmakuAmmoVals in xmlVals.Data)
            {
                DanmakuAmmo tmpItem = new DanmakuAmmo();

                try
                {
                    tmpItem.Item =
                        ItemObject.All.First(x => x.StringId == danmakuAmmoVals.Id);
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to find ammo using item id: {danmakuAmmoVals.Id}", e);
                    continue;
                }

                try
                {
                    tmpItem.Homing = float.Parse(danmakuAmmoVals.Homing);
                    if (tmpItem.Homing < 0.0f || tmpItem.Homing > 1.0f)
                    {
                        Logger.LogException(
                            $"Danmaku homing cannot be smaller than 0 or larger than 1 for id: {danmakuAmmoVals.Id}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to parse homing degrees for id: {danmakuAmmoVals.Id}", e);
                    continue;
                }

                try
                {
                    tmpItem.HomingRange = float.Parse(danmakuAmmoVals.HomingRange);
                    if (tmpItem.HomingRange < 0.0f)
                    {
                        Logger.LogException(
                            $"Danmaku homing range cannot be smaller than 0 for id: {danmakuAmmoVals.Id}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(
                        $"Failed to parse homing range for id: {danmakuAmmoVals.Id}", e);
                    continue;
                }

                this._ammoItems.Add(tmpItem);
            }
            
            return true;
        }*/

        public override void OnCreated()
        {
            base.OnCreated();

            if (!this._hasInitializedFlag)
            {
                this._hasInitializedFlag = this.Initialize()/* && this.InitializeAmmo()*/;
            }

            if (!this._hasInitializedFlag)
            {
                Logger.DisplayInGameConsole("DanmakuLogic failed to initialize!", TAColor.Error);
            }
        }

        public override void OnAgentShootMissile(Agent shooterAgent,
            EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity,
            Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            // I don't think we need this since the base is an empty virtual
            //base.OnAgentShootMissile(
            //    shooterAgent, weaponIndex, position, velocity, orientation, 
            //    hasRigidBody, forcedMissileIndex);

            if (!this._hasInitializedFlag)
            {
                return;
            }

            // check to see if firing weapon is a danmaku weapon
            IEnumerable<DanmakuItem> res = this._items.Where(
                x => x.Item == shooterAgent.Equipment[weaponIndex].Item);
            if (res.Count() > 0)
            {
                DanmakuItem danmakuItem = res.First();

                // IF YES, get ammo
                MissionWeapon missileWeapon =
                    this.GetMissileWeapon(shooterAgent.Equipment[weaponIndex],
                        out bool editedMissileFlag, out int defaultDamage);

                // compute base speed
                // MAYDO what is the difference between this and the weaponIndex?
                //var probe4 = shooterAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                float baseSpeed =
                    (float)shooterAgent.Equipment[weaponIndex].GetModifiedMissileSpeedForCurrentUsage();

                switch (danmakuItem.Type)
                {
                    case DanmakuType.burst:
                        // if burst,
                        //  given dispersion, compute directions/orientations and create each shot individually
                        for (int i = 0; i < danmakuItem.DanmakuNum - 1; i++)
                        {
                            Vec3 modDirection = velocity;
                            modDirection += MBRandom.InsideUnitSphere * danmakuItem.DanmakuDisp;

                            float speed = modDirection.Normalize(); // this also normalizes modDirection

                            //Mat3 modOrientation = orientation;
                            //modOrientation.f = modDirection;

                            // MAYDO what does baseSpeed do?
                            // I think baseSpeed does nothing
                            Mission.Current.AddCustomMissile(
                                shooterAgent, missileWeapon, position, modDirection,
                                TAMath.LookAtWorld(position + modDirection, position),
                                baseSpeed, speed, hasRigidBody, null, forcedMissileIndex);
                        }
                        break;
                    case DanmakuType.spread:
                        // if spread,
                        //  given dispersion and number of shots, compute directions/orientations such that it is an even horizontal spread
                        //  create all shots
                        // MAYDO if we remove the default missile should we allow for 
                        //  even spreads? Would be very hard to aim for since there's no center
                        //  shot
                        int spreadNum;
                        if (danmakuItem.DanmakuNum % 2 == 0)
                        {
                            spreadNum = danmakuItem.DanmakuNum / 2;
                        }
                        else
                        {
                            spreadNum = (danmakuItem.DanmakuNum - 1) / 2;
                        }

                        // in order to spread the shots, we must rotate the velocity vector to the 
                        //  left and right based on our firing agent; however, the vertical angle 
                        //  of our shot and the location of our agent makes things a bit more complicated
                        // in 2D, rotating a vector assumes we rotate around the z axis which points
                        //  'straight up'; in this case our z axis needs to be at the xy location of our
                        //  agent and we need to tilt based on our angle of fire
                        // we can first find our exact left or right vectors, these vectors are
                        //  orthogonal to the plane defined by the velocity and the firing direction,
                        //  which is simply our velocity with no z (height) component
                        // if our z component is zero, set the firing direction z to 1 as the velocity
                        //  is our firing direction
                        Vec3 firingDirection = velocity;
                        firingDirection.z = (velocity.z == 0) ? 1 : 0;
                        Vec3 leftOrRight = Vec3.CrossProduct(velocity, firingDirection).NormalizedCopy();

                        // our tilted 'straight up' (or down) vector can then be computed by 
                        //  finding the vector orthogonal to our left or right vector and our velocity
                        Vec3 rotationVector = Vec3.CrossProduct(leftOrRight, velocity).NormalizedCopy();

                        foreach (float dispVal in
                            this.ComputeSpread(
                                danmakuItem.DanmakuDisp / spreadNum, spreadNum)) // this computes the list of wanted rotations
                        {
                            // now that we have our rotation vector, we simply rotate our velocity
                            //  based on the rotation vector however many degrees we want
                            Vec3 modDirection = velocity.RotateAboutAnArbitraryVector(rotationVector, dispVal);

                            float speed = modDirection.Normalize();

                            //Mat3 modOrientation = orientation;
                            //modOrientation.f = modDirection;

                            Mission.Current.AddCustomMissile(
                                shooterAgent, missileWeapon, position, modDirection,
                                TAMath.LookAtWorld(position + modDirection, position),
                                baseSpeed, speed, hasRigidBody, null, forcedMissileIndex);
                        }
                        break;
                    default:
                        Logger.LogException("Invalid danmaku type");
                        break;
                }

                // reset missile damage if needed
                if (editedMissileFlag)
                {
                    this.ModifyThrustDamage(missileWeapon, defaultDamage);
                }
            }

            // for reference
            //Mission.Current.AddCustomMissile(
            //    shooterAgent, missileWeapon, position, direction, orientation,
            //    baseSpeed, speed, addRigidBody, missionObjectToIgnore,
            //    forcedMissileIndex);

        }

        private List<float> ComputeSpread(float dispStep, int spreadNum)
        {
            // computes all angles we should rotate
            List<float> totalSteps = new List<float>();
            for (int i = 1; i < spreadNum + 1; i++)
            {
                float val = (float)TAMath.D2R(dispStep * i);
                totalSteps.Add(val);
                totalSteps.Add(-val);
            }

            return totalSteps;
        }

        private void ModifyThrustDamage(MissionWeapon baseWeapon, int modThrustDamage)
        {
            MatrixFrame tmpFrame = baseWeapon.CurrentUsageItem.Frame;
            baseWeapon.CurrentUsageItem.Init(
                baseWeapon.CurrentUsageItem.WeaponUsageId,
                baseWeapon.CurrentUsageItem.PhysicsMaterial,
                baseWeapon.CurrentUsageItem.ItemUsage,
                baseWeapon.CurrentUsageItem.ThrustDamageType,
                baseWeapon.CurrentUsageItem.SwingDamageType,
                baseWeapon.CurrentUsageItem.BodyArmor,
                baseWeapon.CurrentUsageItem.WeaponLength,
                baseWeapon.CurrentUsageItem.WeaponBalance,
                baseWeapon.CurrentUsageItem.Inertia,
                baseWeapon.CurrentUsageItem.CenterOfMass,
                baseWeapon.CurrentUsageItem.Handling,
                baseWeapon.CurrentUsageItem.SwingDamageFactor,
                baseWeapon.CurrentUsageItem.ThrustDamageFactor,
                baseWeapon.CurrentUsageItem.MaxDataValue,
                baseWeapon.CurrentUsageItem.PassbySoundCode,
                baseWeapon.CurrentUsageItem.Accuracy,
                baseWeapon.CurrentUsageItem.MissileSpeed,
                baseWeapon.CurrentUsageItem.StickingFrame,
                baseWeapon.CurrentUsageItem.AmmoClass,
                baseWeapon.CurrentUsageItem.SweetSpotReach,
                baseWeapon.CurrentUsageItem.SwingSpeed,
                baseWeapon.CurrentUsageItem.SwingDamage,
                baseWeapon.CurrentUsageItem.ThrustSpeed,
                modThrustDamage,
                baseWeapon.CurrentUsageItem.RotationSpeed,
                baseWeapon.CurrentUsageItem.WeaponTier);
            baseWeapon.CurrentUsageItem.SetFrame(tmpFrame);
        }

        private MissionWeapon GetMissileWeapon(MissionWeapon missionWeapon,
            out bool editedMissileFlag, out int defaultDamage)
        {
            MissionWeapon missileWeapon;
            editedMissileFlag = false;
            defaultDamage = 0;

            if (missionWeapon.CurrentUsageItem.IsRangedWeapon &&
                missionWeapon.CurrentUsageItem.IsConsumable)
            {
                // MAYDO Do we need to modify missile weapon for thrown?
                missileWeapon = missionWeapon;
            }
            else
            {
                // we need to modify the missile weapon as the custom missile code
                //  does not assign the missile any missile damage from the weapon
                editedMissileFlag = true;
                missileWeapon = missionWeapon.AmmoWeapon;
                defaultDamage = missileWeapon.CurrentUsageItem.ThrustDamage;
                this.ModifyThrustDamage(missileWeapon, defaultDamage +
                    missionWeapon.GetModifiedThrustDamageForCurrentUsage());
            }

            return missileWeapon;
        }
    }
}
