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
    public class CharacterModelConfig : MonoBehaviour
    {
        [Header("Weapon Model Holder")]
        public Transform weaponHolder; // the transform on our character rig where the weapon model will be attached to when instantiated

        [Header("Cosmetic Model Holder")]
        public Transform cosmeticHolder; // the transform on our character rig where the cosmetic model will be attached to when instantiated

        [Header("Character's Animator")]
        public Animator _cacheAnimator; // the animator on the character model

        private GameObject weaponModel; // the weapon model object

        private GameObject cosmeticModel; // the cosmetic model object

        // set our weapon model of our weapon config when setting weapons
        public void SetWeaponModel(GameObject model)
        {
            if (weaponHolder != null)
            {
                foreach (Transform child in weaponHolder.transform)
                {
                    Destroy(child.gameObject);
                }
                if (weaponModel != null)
                    Destroy(weaponModel);
                var newWeaponModel = AddModel(model, weaponHolder);
            }      
        }

        // sets our cosmetic model of our selected cosmetic when setting cosmetics
        public void SetCosmeticModel(GameObject model)
        {
            if (cosmeticHolder != null)
            {
                foreach (Transform child in cosmeticHolder.transform)
                {
                    Destroy(child.gameObject);
                }
                if (cosmeticModel != null)
                    Destroy(cosmeticModel);
                var newCosmeticModel = AddModel(model, cosmeticHolder);
            }           
        }

        private GameObject AddModel(GameObject model, Transform transform)
        {
            if (model == null)
                return null;
            var newModel = Instantiate(model);
            newModel.transform.parent = transform;
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            newModel.transform.localScale = Vector3.one;
            newModel.gameObject.SetActive(true);
            return newModel;
        }
    }
}

