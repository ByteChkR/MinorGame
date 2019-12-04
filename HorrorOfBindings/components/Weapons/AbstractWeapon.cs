using Assimp;
using Engine.Core;
using Engine.Rendering;

namespace HorrorOfBindings.components.Weapons
{
    public abstract class AbstractWeapon : AbstractComponent
    {
        public Bullet BulletPrefab { get; set; }
        public GameObject WeaponNozzle { get; set; }
        public float FireRate { get; set; }
        public float ReloadTime { get; set; }
        public int MaxAmmoCount { get; set; }
        public int CurrentAmmoCount { get; set; }
        public bool AutoReload { get; set; }
        private float _reloadTimer = 0;
        private float _fireRateTimer = 0;
        private bool _isShooting = false;
        private bool _isReloading = false;
        private bool _pullTrigger = false;
        public virtual bool EmptyMagazine => CurrentAmmoCount == 0;
        public virtual bool CanReload => CurrentAmmoCount < MaxAmmoCount;

        public virtual void Reload()
        {
            if (!CanReload)
            {
                return;
            }

            _isReloading = true;
            _reloadTimer = ReloadTime;
        }

        public void PullTrigger()
        {
            _pullTrigger = true;
        }

        public void ReleaseTrigger()
        {
            _pullTrigger = false;
            //Clear Shoot flag, to enable direct shooting when pulling trigger again
            _isShooting = false;
            _fireRateTimer = 0;
        }

        private void Shoot()
        {
            if (!EmptyMagazine)
            {
                if (_isShooting)
                {
                    return;
                }

                GameObject bullet = BulletPrefab.CreateBullet(WeaponNozzle);
                Owner.Scene.Add(bullet);
                _fireRateTimer = FireRate;
                _isShooting = true;
                return;
            }
            else if (AutoReload)
            {
                Reload();
                return;
            }
        }


        protected override void Update(float deltaTime)
        {
            if (_isShooting)
            {
                _fireRateTimer -= deltaTime;
                if (_fireRateTimer <= 0)
                {
                    _fireRateTimer = 0;
                    _isShooting = false;
                }
            }

            if (_isReloading)
            {
                _reloadTimer -= deltaTime;
                if (_reloadTimer <= 0)
                {
                    _reloadTimer = 0;
                    _isReloading = false;
                    CurrentAmmoCount = MaxAmmoCount;
                }
            }
        }
    }
}