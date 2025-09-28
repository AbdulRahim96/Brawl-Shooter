/*
 * Copyright (c) 2024 VAUXLAND
 * Part of the "Fusion Shooter Brawler" Asset.
 * You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * otherwise make available to any third party the Service or the Content of this Asset.
 * Use of this asset is governed by the Unity Asset Store End User License Agreement.
 * See https://unity3d.com/legal/as_terms for more information.
 */

using CandyCoded.HapticFeedback;
using Fusion;
using System.Collections;
using UnityEngine;

namespace Vauxland.FusionBrawler
{
    public class ProjectileController : NetworkBehaviour
    {
        public ProjectileLauncherTransform projectileLauncher; // the transform the projectile will launch from
        private PlayerManager _playerManager = null;
        private NetworkObject _projectile = null;
        private CameraFollow cameraFollow;

        [Networked] private NetworkButtons _buttonsPrevious { get; set; }
        [Networked] private TickTimer _shootCooldown { get; set; }

        private float _shootDelay = .2f; // delay before shooting
        private float launchOffset;
        private Coroutine _shootCoroutine;
        private bool _initialDelayApplied = false;

        [SerializeField]
        private NetworkProjectileLoader _projectileLoader;

        [Networked] private int _fireCount { get; set; }
        private int _currentFireCount;

        public override void Spawned()
        {
            _playerManager = GetComponent<PlayerManager>();
        }
        
        // sets the loaded weapons info for the weapons set projectile
        public void SetWeaponInfo()
        {
            var playerWeapon = _playerManager._playerStats.playerWeapon;
            _projectile = playerWeapon.projectile;
            _shootDelay = playerWeapon.delayBeforeShooting;
            launchOffset = playerWeapon.launchOffset;
            _projectileLoader.SetProjectileObject(_projectile); // setting the projectile prefab from the weapon data
        }

        public override void Render()
        {
            if (!_playerManager.PlayerSetUp) return;

            // check if a new shot was fired // this allows effects like muzzle effect to show on all clients
            if (_currentFireCount < _fireCount)
            {
                var fxPlayer = _playerManager._playerVisuals.fxPlayer;
                AudioClip shootFx = null;

                if (_playerManager._playerStats.playerWeapon != null)
                    shootFx = _playerManager._playerStats.playerWeapon.shootFx;

                if (_playerManager._playerVisuals.weaponModelComponent != null)
                    _playerManager._playerVisuals.weaponModelComponent.muzzleEffect.Play();

                if (fxPlayer != null && shootFx != null)
                {
                    _playerManager._playerVisuals.fxPlayer.PlayOneShot(shootFx);
                }

                if (Object.HasInputAuthority)
                {
                    // trigger camera shake
                    if (cameraFollow == null)
                    {
                        cameraFollow = Camera.main.GetComponent<CameraFollow>();
                    }

                    if (cameraFollow != null && cameraFollow.useCameraShake)
                    {
                        cameraFollow.StartShake();
                    }

                    HapticFeedback.HeavyFeedback(); // trigger haptic feedback on shooting
                }
            }

            // keep track of fire count for proxies
            _currentFireCount = _fireCount;

            // only local player should see the aiming UI
            if (Object.HasInputAuthority)
                _playerManager._playerVisuals.aimingVisual.SetActive(_playerManager._playerStats.IsShooting);

        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.TryGetInputForPlayer<PlayerNetworkInputData>(Object.InputAuthority, out var inputData) && _playerManager._playerStats.AcceptPlayerInput
                && _playerManager._playerStats.CanShoot && !_playerManager._matchManager.GameIsOver)
            {
                ShootWeapon(inputData);
            }

            if (_playerManager._matchManager.GameIsOver)
            {
                _playerManager._playerStats.IsShooting = false;
            }
        }

        // called when players weapon is changed
        public void OnWeaponChanged()
        {
            // stop shooting
            _playerManager._playerStats.IsShooting = false;

            // stop any shooting coroutine
            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
            }

            // reset the initial delay flag
            _initialDelayApplied = false;

            // reset shoot cooldown
            _shootCooldown = TickTimer.None;

            // reset fire count
            _fireCount = 0;
            _currentFireCount = 0;
        }

        // bot's shoot method
        public void BotShoot(PlayerNetworkInputData inputData)
        {
            ShootWeapon(inputData);
        }

        // players shoot method
        private void ShootWeapon(PlayerNetworkInputData input)
        {
            _playerManager._playerStats.IsShooting = false;

            // an extra check here to stop the shooting coroutine if player is reloading
            if (_playerManager._playerStats.IsReloading || !_playerManager._playerStats.CanShoot)
            {
                if (_shootCoroutine != null)
                {
                    StopCoroutine(_shootCoroutine);
                    _shootCoroutine = null;
                }

                // reset the initial delay flag
                _initialDelayApplied = false;
            }

            // prevent shooting if not allowed
            if (!_playerManager._playerStats.CanShoot || !_playerManager._playerStats.AcceptPlayerInput)
            {
                _playerManager._playerStats.IsShooting = false;
                return;
            }

            // if holding down the shoot button
            if (input.networkButtons.IsSet(NetInputButtons.Shoot))
            {
                if (_playerManager._playerStats.LoadedAmmo > 0 && !_playerManager._playerStats.IsReloading)
                {
                    _playerManager._playerStats.IsShooting = true;
                    if (_initialDelayApplied)
                    {
                        LaunchProjectileWithLoader();
                    }
                    else
                    {
                        // start the Coroutine for the initial shooting delay
                        if (_shootCoroutine == null)
                        {
                            _shootCoroutine = StartCoroutine(ShootAfterInitialDelay());
                        }
                    }
                }
                else
                {
                    //Debug.Log("No ammo loaded, need to reload."); uncomment for debugging
                    _playerManager._playerStats.StartReloading();
                    _playerManager._playerStats.IsShooting = false;
                }
            }
            else
            {
                _playerManager._playerStats.IsShooting = false;

                // stop the shooting coroutine if the shoot button is released
                if (_shootCoroutine != null)
                {
                    StopCoroutine(_shootCoroutine);
                    _shootCoroutine = null;
                }

                // reset the initial delay flag
                _initialDelayApplied = false;
            }

            _buttonsPrevious = input.networkButtons;
        }

        private IEnumerator ShootAfterInitialDelay()
        {
            yield return new WaitForSeconds(_shootDelay); // waiting for the shoot delay this allows proper aiming and animation to play       

            LaunchProjectileWithLoader(); // the method we use to launch the projectile

            _initialDelayApplied = true; // sets the flag to indicate the initial delay has been applied
            _shootCoroutine = null; // reset coroutine reference after shooting

        }

        // clients use this to mitigate lag when spawning projectiles
        private void LaunchProjectileWithLoader()
        {
            // if our shot cooldown is still running dont fire another shot
            if (_shootCooldown.ExpiredOrNotRunning(Runner) == false) return;

            int AmmoUseAmount = _playerManager._playerStats.playerWeapon.ammoAmountShotUse;
            float offset = 0.5f; //adjust this as needed or use launchOffset to uses specific offsets from weapon data

            // gets the player's current cacheTransform
            var cacheTransform = _playerManager._playerMovement._cacheTransform;
            Vector3 launchPosition;
            if (_playerManager._playerVisuals.weaponModelComponent != null)
            {
                launchPosition = _playerManager._playerVisuals.weaponModelComponent.projectileLauncher.position + cacheTransform.forward * offset;
            }
            else
            {
                launchPosition = projectileLauncher.transform.position + projectileLauncher.transform.forward * offset;
            }
            launchPosition.y = projectileLauncher.transform.position.y;
            Vector3 launchForward = cacheTransform.forward;
            Quaternion launchRotation = Quaternion.LookRotation(launchForward);

            // launches the projectile if the weapon is marked as a shotgun
            if (_playerManager._playerStats.playerWeapon.isShotgun)
            {
                int pelletCount = _playerManager._playerStats.playerWeapon.shotgunPelletCount;
                float spreadAngleHorizontal = _playerManager._playerStats.playerWeapon.shotSpreadAngleHorizontal;
                float spreadAngleVertical = _playerManager._playerStats.playerWeapon.shotSpreadAngleVertical;

                for (int i = 0; i < pelletCount; i++)
                {
                    // calculate spread angle for shotgun projectiles
                    float horizontalAngle = Random.Range(-spreadAngleHorizontal / 2, spreadAngleHorizontal / 2);
                    float verticalAngle = Random.Range(-spreadAngleVertical / 2, spreadAngleVertical / 2);

                    Quaternion spreadRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

                    Vector3 pelletDirection = spreadRotation * launchForward;

                    var projectile = _projectileLoader.Request<Projectile>(launchPosition, Quaternion.LookRotation(pelletDirection), Object.InputAuthority);
                    if (projectile != null)
                    {
                        projectile.ShootProjectile(launchPosition, pelletDirection, Object.InputAuthority, _playerManager._networkObject, _playerManager._playerStats.playerWeapon, _playerManager._playerController);
                    }
                }
            }
            else // launches all other weapons projectiles
            {
                var projectile = _projectileLoader.Request<Projectile>(launchPosition, launchRotation, Object.InputAuthority);
                if (projectile != null)
                {
                    projectile.ShootProjectile(launchPosition, launchForward, Object.InputAuthority, _playerManager._networkObject, _playerManager._playerStats.playerWeapon, _playerManager._playerController);
                }
            }

            _fireCount++;
            _playerManager._playerStats.LoadedAmmo -= AmmoUseAmount; // use the ammo shot
            _shootCooldown = TickTimer.CreateFromSeconds(Runner, _playerManager._playerStats.playerWeapon.delayBetweenShots); // restart the shot cooldown
        }
    }
}

