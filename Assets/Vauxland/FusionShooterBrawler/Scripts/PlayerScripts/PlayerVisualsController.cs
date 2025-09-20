/*
 * Copyright (c) 2024 VAUXLAND
 * Part of the "Fusion Shooter Brawler" Asset.
 * You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * otherwise make available to any third party the Service or the Content of this Asset.
 * Use of this asset is governed by the Unity Asset Store End User License Agreement.
 * See https://unity3d.com/legal/as_terms for more information.
 */

using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vauxland.FusionBrawler
{
    public class PlayerVisualsController : NetworkBehaviour
    {
        [Header("StatUIElements")]
        public TMP_Text playerNickNameText; // the floating name text ui above the player object
        public GameObject hpHolder; // the UI object above the player object that holds the fills of HP and BodyArmor
        public GameObject reloadBar; // the floating yellow image fill object above the player objec that sets active when reloading
        public Image hpFill; // the floating red image fill ui above the player object that shows the players current health amount
        public Image bodyArmorFill; // the floating blue image fill ui above the player object that shows the players current body armor amount
        public Image reloadFill; // the fill image of the reload bar
        public TMP_Text hpAmount; // the text UI above the player object that shows the HP Amount

        [Header("EmotesSetup")]
        public GameObject emoteHolder; // the players emotes ui holder where the emotes show when used
        public List<GameObject> emoteImages; // list of emote images
        [Networked] private TickTimer emoteTimer { get; set; } // timer to hide the emote after a duration
        [Networked] private int currentEmoteIndex { get; set; } // the current emote index being displayed

        [Header("PlayerEffects")]
        public GameObject deathEffect; // the effect that plays when the player dies
        public GameObject shieldEffect; // the shield effect that plays when the player gets a shield on respawn
        public AudioSource fxPlayer; // the audio source to play sound effects from the player

        [Header("PlayerSetup")]
        [HideInInspector] public CharacterModelConfig playerModel; // the players model to show
        [HideInInspector] public WeaponModelComponent weaponModelComponent; // the weapon model
        public Transform effectPlayer; // the transform you can use to spawm effects from on the player
        public Transform playerModelHolder; // the transform where the selected player model will spawn

        public GameObject playerUIHolder; // the players floating UI holder to control the visibility when entering and exiting hiding zones
        public GameObject aimingVisual; // the players aiming UI when shooting
        public GameObject noTeamVisual; // the no team UI visual seen in deathmatch
        public GameObject[] teamRedVisuals; // team reds visuals when on team red
        public GameObject[] teamBlueVisuals; // team blues visuals when on team blue

        private Animator cacheAnimator; // the players character model configs' animator
        protected PlayerManager _playerManager;
        protected PlayerMovementManager _playerMovement;

        [HideInInspector][Networked] public NetworkBool AnimatorSet { get; set; }

        public override void Spawned()
        {
            _playerManager = GetComponent<PlayerManager>();
            _playerMovement = GetComponent<PlayerMovementManager>();
            fxPlayer = GetComponent<AudioSource>();
            AnimatorSet = false;
        }

        // sets our models for the selected character, weapon and cosmetic
        public virtual void OnSetPlayerModel()
        {
            var weaponConfig = _playerManager._playerStats.playerWeapon;
            var characterConfig = _playerManager._playerStats.playerCharacter;
            var cosmeticConfig = _playerManager._playerStats.playerCosmetic;
            if (characterConfig == null || characterConfig.characterModel == null)
                return;

            if (playerModel != null)
                Destroy(playerModel.gameObject);

            if (characterConfig != null)
                playerModel = Instantiate(characterConfig.characterModel, playerModelHolder);
            if (weaponConfig != null && playerModel != null)
                playerModel.SetWeaponModel(weaponConfig.weaponModel);
            if (cosmeticConfig != null && playerModel != null)
                playerModel.SetCosmeticModel(cosmeticConfig.cosmeticModel);
            if (playerModel != null)
                weaponModelComponent = playerModel.GetComponentInChildren<WeaponModelComponent>();

            playerModel.gameObject.SetActive(true);
            if (playerModel != null)
            {
                if (Object.HasInputAuthority)
                {
                    RpcSetUpAnimator();
                }

                SetUpAnimator();

            }
        }

        // sets up the character models animator
        protected virtual void SetUpAnimator()
        {
            if (playerModel == null)
                return;

            cacheAnimator = playerModel._cacheAnimator;

            if (cacheAnimator == null)
                return;

            AnimatorSet = true;
        }

        public override void Render()
        {
            if (AnimatorSet)
            {
                UpdatePlayerAnims();
            }

            if (_playerManager._playerStats.PlayerIsReady)
            {
                UpdateUI();
            }

            UpdateEmoteDisplay();
        }

        // updates health and body armor fill bars.
        private void UpdateUI()
        {
            hpAmount.text = _playerManager._playerStats.Hp.ToString();
            if (hpFill != null)
                hpFill.fillAmount = _playerManager._playerStats.Hp / (float)_playerManager._playerStats.TotalHp;

            if (bodyArmorFill != null)
                bodyArmorFill.fillAmount = _playerManager._playerStats.BodyArmor / (float)_playerManager._playerStats.TotalBodyArmor;
        }

        private void UpdateEmoteDisplay()
        {
            // hide all emote images
            foreach (var emote in emoteImages)
            {
                emote.SetActive(false);
            }

            if (emoteTimer.ExpiredOrNotRunning(Runner))
            {
                return;
            }

            // show the current emote
            if (currentEmoteIndex >= 0 && currentEmoteIndex < emoteImages.Count)
            {
                emoteImages[currentEmoteIndex].SetActive(true);
            }
        }

        // updates the player's animations
        protected virtual void UpdatePlayerAnims()
        {
            if (cacheAnimator == null) return;

            if (Runner.IsForward == false)
                return;

            if (_playerManager._playerStats.Hp > 0)
            {
                Vector3 velocity = _playerMovement.GetCurrentVelocity();
                var xzMagnitude = new Vector3(velocity.x, 0, velocity.z).magnitude;
                cacheAnimator.SetBool("IsDead", false);
                cacheAnimator.SetFloat("MoveSpeed", xzMagnitude);
                cacheAnimator.SetBool("IsShooting", _playerManager._playerStats.IsShooting && _playerManager._playerStats.CanShoot);
            }

            cacheAnimator.SetInteger("AnimID", _playerManager._playerStats.playerWeapon != null ? _playerManager._playerStats.playerWeapon.animID : 0);

            cacheAnimator.SetBool("IsIdle", !cacheAnimator.GetBool("IsDead") || !cacheAnimator.GetBool("IsShooting"));
        }

        // changes the visibility of the player
        public void SetVisible(bool isVisible)
        {
            if (playerModel != null)
            {
                playerModel.gameObject.SetActive(isVisible);
            }
            if (!_playerManager._playerStats.IsRespawning)
            {
                hpHolder.SetActive(isVisible);
                playerNickNameText.gameObject.SetActive(isVisible);
                playerUIHolder.SetActive(isVisible);
            }
           
        }

        // adjust our Health bar based on the damage taken
        public void PlayerHpEffected(int oldHp, int newHp, int totalHp)
        {
            //Debug.Log($"[Host: {Object.HasStateAuthority}] PlayerHpEffected called. Old HP: {oldHp}, New HP: {newHp}, Total HP: {totalHp}");

            // Debug.Log($"PlayerHpEffected called. Old HP: {oldHp}, New HP: {newHp}, Total HP: {totalHp}"); // can test with these debugs if you need to

            if (cacheAnimator == null)
                return;

            if (newHp > 0 && oldHp <= 0)
            {
                // player has just respawned
                cacheAnimator.SetBool("IsDead", false);
            }
            else if (newHp <= 0 && oldHp > 0)
            {
                // player has just died
                cacheAnimator.SetBool("IsDead", true);
                cacheAnimator.SetFloat("MoveSpeed", 0);
                cacheAnimator.SetBool("IsShooting", false);
                cacheAnimator.SetBool("IsIdle", false);
            }

            if (newHp != oldHp)
            {
                hpAmount.text = newHp.ToString();
                if (hpFill != null)
                {
                    hpFill.fillAmount = Mathf.Clamp01(newHp / (float)totalHp);
                }

                if (newHp < oldHp)
                {
                    PlayerReceiveDamageEffects();
                }
            }
        }

        private void PlayerReceiveDamageEffects()
        {
            // play damage effects here, sounds etc.
        }

        public void PlayBodyArmorDamageEffects()
        {
            // play bodyarmor loss effects and sounds here
        }

        public void ShowEmote(int emoteIndex)
        {
            if (Object.HasInputAuthority)
            {
                RpcShowEmote(emoteIndex);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RpcShowEmote(int emoteIndex)
        {
            currentEmoteIndex = emoteIndex;
            emoteTimer = TickTimer.CreateFromSeconds(Runner, 3f);
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RpcSetUpAnimator()
        {
            SetUpAnimator();
        }
    }
}

