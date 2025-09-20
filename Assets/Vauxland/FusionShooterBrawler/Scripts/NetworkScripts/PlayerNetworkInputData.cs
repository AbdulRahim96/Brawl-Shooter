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

// our Network Input // any other controls or input you would like to add add here
namespace Vauxland.FusionBrawler
{
    public struct PlayerNetworkInputData : INetworkInput
    {
        public NetworkButtons networkButtons;

        public Vector3 inputPlayerMove;
        public Vector3 inputPlayerDirection;
        public Vector3 mousePosition;

        public NetworkBool inputPlayerJump;
        public NetworkBool inputPlayerShoot;
    }

    public enum NetInputButtons
    {
        Shoot,
        Jump,
    }
}

