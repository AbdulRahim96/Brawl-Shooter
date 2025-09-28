/*
 * Copyright (c) 2024 VAUXLAND
 * Part of the "Fusion Shooter Brawler" Asset.
 * You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * otherwise make available to any third party the Service or the Content of this Asset.
 * Use of this asset is governed by the Unity Asset Store End User License Agreement.
 * See https://unity3d.com/legal/as_terms for more information.
 */

using UnityEngine;
using Fusion;

namespace Vauxland.FusionBrawler
{
    public class NetworkPickup : NetworkBehaviour
    {
        [Header("Powerup Setup")]
        public PowerupConfig powerupConfig;

        [Header("Weapon Pickup")]
        public WeaponConfig weaponPickup;

        [Header("Stat Effect")]
        public StatEffectConfig statEffect;

        private SpawnManager spawnManager;


        public override void Spawned()
        {

        }

        public void SetSpawnManager(SpawnManager manager)
        {
            spawnManager = manager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) // if a player walks into the projectile
            {
                var playerStatsManager = other.GetComponent<PlayerStatsManager>();
                if(Object == null)
                    print("Object is null");
                else
                    print("Object is not null");


                if (playerStatsManager != null && Object.HasStateAuthority)
                {
                    ApplyPowerUp(playerStatsManager);

                    // notify the SpawnManager
                    if (spawnManager != null)
                    {
                        spawnManager.PickupPickedUp(Object);
                    }

                    Runner.Despawn(Object); // despawn the pickup after it has been picked up
                }
            }
        }

        // apply the pickup stats or weapon to our player
        private void ApplyPowerUp(PlayerStatsManager playerStatsManager)
        {
            if (powerupConfig != null)
            {
                foreach (var statEntry in powerupConfig.powerUpStats.stats)
                {
                    playerStatsManager.ModifyStat(statEntry.statType, statEntry.value);
                }
            }

            if (statEffect != null)
            {
                statEffect.ApplyEffect(playerStatsManager);
            }

            if (weaponPickup != null)
            {
                int weaponId = PlayerGameData.PlayerData.GetWeaponID(weaponPickup);

                if (weaponId >= 0)
                {
                    playerStatsManager.WeaponID = weaponId;
                }
                else
                {
                    Debug.LogError("Weapon not found in PlayerGameData weapons array.");
                }
            }
        }

    }
}

