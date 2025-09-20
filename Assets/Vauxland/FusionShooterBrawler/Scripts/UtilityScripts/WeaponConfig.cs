/*
 * Copyright (c) 2024 VAUXLAND
 * Part of the "Fusion Shooter Brawler" Asset.
 * You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * otherwise make available to any third party the Service or the Content of this Asset.
 * Use of this asset is governed by the Unity Asset Store End User License Agreement.
 * See https://unity3d.com/legal/as_terms for more information.
 */

using Fusion;
using UnityEngine;


namespace Vauxland.FusionBrawler
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "FusionBrawler/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Weapon Setup")]
        public string weaponName; // the name of the weapon to reference when showing a kill
        public Texture weaponIcon; // the weapons UI icon to show
        public GameObject weaponModel; // the weapoons model mesh
        public NetworkObject projectile; // the projectile prefab the gun shoots
        public float delayBetweenShots; // the rate of fire
        public float delayBeforeShooting; // the delay before the weapon actually starts the initial stooting
        public float launchOffset; //the launch point offset to shoot the bullet further away or behind the projectile launch point position;
        public int destructibleDamage; // damage done to destructible objects

        public int animID; // this will be used to distinguish the weapon's animation to play in the animator

        [Header("Shotgun Setup")] // if the weapon is a shotgun or uses a spread shot
        public bool isShotgun;
        public int shotgunPelletCount = 5; // number of pellets for the shotgun
        public float shotSpreadAngleHorizontal = 10f; // spread angle for the shotgun
        public float shotSpreadAngleVertical = 5f; // vertical spread angle

        [Header("Weapon Stats")]
        public StatDictionary weaponStats; // set the weapon stats here how much damage it does, other stats it gives to player like Hp, etc.

        [Header("ReloadingSettings")]
        public bool reloadOneAmmoAtATime; // Reload one ammo at a time
        public float reloadDuration; // The time it takes to reload the weapon
        public int reloadClipSize; // how much ammo to reload at once and max clip size if reload at one time;
        public int ammoAmountShotUse;// The amount of ammo to use in a single shot

        [Header("SFX")]
        public AudioClip shootFx; //shooting sound
        public AudioClip reloadFx; // reload sound
        public AudioClip emptyFx;// out of ammo sound
    }
}

