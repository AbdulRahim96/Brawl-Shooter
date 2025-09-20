/*
 * Copyright (c) 2024 VAUXLAND
 * Part of the "Fusion Shooter Brawler" Asset.
 * You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * otherwise make available to any third party the Service or the Content of this Asset.
 * Use of this asset is governed by the Unity Asset Store End User License Agreement.
 * See https://unity3d.com/legal/as_terms for more information.
 */

using UnityEngine;

namespace Vauxland.FusionBrawler
{
    public class WeaponModelComponent : MonoBehaviour
    {
        // just a script we use to access the weapons muzzle flash particle effect, can we used to access other components to you add to the model
        [SerializeField] public ParticleSystem muzzleEffect;
        [SerializeField] public Transform projectileLauncher;
    }
}