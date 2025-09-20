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
using UnityEngine.EventSystems;

namespace Vauxland.FusionBrawler
{
    // where we handle all of our players movement input
    public class PlayerInputManager : MonoBehaviour
    {
        [HideInInspector]
        public CameraFollow cameraFollow; // the camera thats following the player

        // the camera vectors
        public Vector3 cameraForward;
        protected Vector3 cameraRight;

        // player movement set up
        protected Vector3 inputPlayerMove;
        protected Vector3 inputPlayerDirection;
        protected Vector3 mousePosition;
        protected Vector3 aimVector;
        
        // player buttons set up
        protected bool inputPlayerShoot;
        protected bool inputPlayerJump;

        protected PlayerStatsManager cachePlayerStats;
        protected PlayerManager _playerManager;
        public Transform _cacheTransform { get; private set; }

        [HideInInspector] public bool isSet = false;

        // method to set the input components for the player
        public void SetInputComponents(Transform transform, CameraFollow camera, PlayerManager playerManager)
        {
            _cacheTransform = transform;
            cameraFollow = camera;
            _playerManager = playerManager;
        }

        protected void Update()
        {
            UpdatePlayerInput();
        }

        // handles updating player input each frame
        protected virtual void UpdatePlayerInput()
        {
            // exit early if input is not set or playerManager is not available
            if (isSet == false)
                return;

            if (_playerManager == null)
                return;

            // only process input if the player has input authority
            if (_playerManager.Object.HasInputAuthority == false)
                return;

            bool isMobileInput = false;

            // determine if running on a mobile platform
            isMobileInput = Application.isMobilePlatform;
#if UNITY_EDITOR
            isMobileInput = PlayerGameData.PlayerData.testMobileControls;
#endif

            // update camera directions if camera follow is assigned
            if (cameraFollow != null)
            {
                cameraForward = cameraFollow.CacheCameraTransform.forward;
                cameraRight = cameraFollow.transform.right;
            }

            // normalize camera directions and remove vertical component
            cameraForward.y = 0;
            cameraForward = cameraForward.normalized;
            cameraRight.y = 0;
            cameraRight = cameraRight.normalized;

            // unlock cursor and make it visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // process shooting input for desktop if not over UI element
            if (!isMobileInput && !EventSystem.current.IsPointerOverGameObject())
            {
                inputPlayerShoot = Input.GetButton("Fire1");          
            }

            // disable shooting if the player is not shooting
            if (_playerManager.PlayerSetUp && !_playerManager._playerStats.IsShooting)
            {
                inputPlayerShoot = false;
            }

            // reset movement and direction inputs
            inputPlayerJump = false;
            inputPlayerMove = Vector3.zero;
            inputPlayerDirection = Vector3.zero;

            // get jump input
            inputPlayerJump = Input.GetButton("Jump");

            // get mouse position for aiming
            mousePosition = Input.mousePosition;

            // apply movement input based on camera direction
            inputPlayerMove += cameraForward * Input.GetAxis("Vertical");
            inputPlayerMove += cameraRight * Input.GetAxis("Horizontal");

            // enable mobile controls if on mobile
            if (isMobileInput)
            {
                _playerManager._mobileControls.useMobileControls = true;
            }

            // handle aiming and shooting
            if (inputPlayerShoot || _playerManager._playerStats.IsShooting)
            {
                if (isMobileInput)
                {
                    inputPlayerDirection += Input.GetAxis("Mouse Y") * cameraForward;
                    inputPlayerDirection += Input.GetAxis("Mouse X") * cameraRight;
                }
                else
                {
                    inputPlayerDirection = (mousePosition - cameraFollow.CacheCamera.WorldToScreenPoint(_cacheTransform.position)).normalized;
                    inputPlayerDirection = new Vector3(inputPlayerDirection.x, 0, inputPlayerDirection.y);
                }

            }
            else
            {
                // if not shooting and not mobile input, use movement input for direction
                if (!isMobileInput)
                    inputPlayerDirection = inputPlayerMove;
            }

        }

        // mobile controls methods

        // sets movement input for mobile controls
        public void SetMovementInput(Vector3 move)
        {
            inputPlayerMove = move;
        }

        // sets aim input for mobile controls
        public void SetAimInput(Vector3 aim)
        {
            inputPlayerDirection = aim;
        }

        // sets shooting input for mobile controls
        public void SetShootInput(bool shoot)
        {
            inputPlayerShoot = shoot;
        }

        // sets jump input for mobile controls
        public void SetJumpInput(bool jump)
        {
            inputPlayerJump = jump;
        }

        // prepares and returns network input data for the player
        public PlayerNetworkInputData SetPlayerNetworkInput()
        {

            PlayerNetworkInputData inputData = new PlayerNetworkInputData();

            // input vectors
            inputData.inputPlayerMove = inputPlayerMove;

            inputData.inputPlayerDirection = inputPlayerDirection;

            //input Bools
            inputData.inputPlayerShoot = inputPlayerShoot;

            inputData.inputPlayerJump = inputPlayerJump;


            inputData.mousePosition = mousePosition;

            // handle network shooting input based on platform
            if (!_playerManager._mobileControls.useMobileControls)
                inputData.networkButtons.Set(NetInputButtons.Shoot, Input.GetButton("Fire1"));
            else if (_playerManager._mobileControls.useMobileControls)
                inputData.networkButtons.Set(NetInputButtons.Shoot, _playerManager._mobileControls.isShooting);

            // reset shooting and jumping inputs
            inputPlayerShoot = false;
            inputPlayerJump = false;

            return inputData;
        }
    }
}

