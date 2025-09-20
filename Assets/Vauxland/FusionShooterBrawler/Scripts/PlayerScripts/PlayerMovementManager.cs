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
    public class PlayerMovementManager : NetworkBehaviour
    {
        private PlayerInputManager _playerInput;
        protected PlayerManager _playerManager;

        public Camera targetCamera;
        public CameraFollow cameraFollow;
        public Transform _cacheTransform;
        private NetworkCharacterController _networkCharacterController;

        private Vector3 playerMovement;
        private Vector3 playerRotation;

        protected Vector3? previousPosition;
        public Vector3 currentVelocity;

        public override void Spawned()
        {
            _cacheTransform = transform;
            _playerManager = GetComponent<PlayerManager>();
            _networkCharacterController = GetComponent<NetworkCharacterController>();
            _networkCharacterController.rotationSpeed = 0;

            // only set the camera to follow the local player with input authority
            if (Object.HasInputAuthority)
            {              
                cameraFollow = FindObjectOfType<CameraFollow>(); // find the camera follow component
                _playerInput = GetComponent<PlayerInputManager>(); // get the player input manager
                if (_playerInput != null)
                    _playerInput.SetInputComponents(transform, cameraFollow, _playerManager); // set input components for the player
            }
        }

        public override void Render()
        {
            // we put this in render so that animations update smoothly on all clients
            if (_cacheTransform != null)
            {
                if (!previousPosition.HasValue)
                    previousPosition = _cacheTransform.position; // store the initial position if not already stored

                // calculate current movement and velocity
                var currentMove = _cacheTransform.position - previousPosition.Value;
                currentVelocity = currentMove / Runner.DeltaTime;
                previousPosition = _cacheTransform.position; // update the previous position
            }
        }

        public override void FixedUpdateNetwork()
        {
            // stop here if player input is not allowed
            if (!_playerManager._playerStats.AcceptPlayerInput)
                return;

            playerMovement = Vector3.zero;
            playerRotation = Vector3.zero;

            // retrieve player input data
            if (Runner.TryGetInputForPlayer<PlayerNetworkInputData>(Object.InputAuthority, out var inputData))
            {
                playerMovement = inputData.inputPlayerMove; // movement input
                playerRotation = inputData.inputPlayerDirection; // rotation input
            }

            UpdateMovements(); // update player movement and rotation

        }

        // returns the current velocity of the player
        public Vector3 GetCurrentVelocity()
        {
            return currentVelocity;
        }

        // updates player movement and rotation
        protected virtual void UpdateMovements()
        {

            if (!_playerManager._playerController.IsBot)
            {
                MovePlayer(playerMovement);
                RotatePlayer(playerRotation);
            }

        }

        // moves the bot in the direction
        public void BotMove(Vector3 direction)
        {
            MovePlayer(direction);
        }

        // moves the player in this direction
        protected void MovePlayer(Vector3 direction)
        {
            if (direction.sqrMagnitude > 1)
                direction = direction.normalized;
            direction.y = 0;

            // sets acceleration and braking values to super high so that its smooth movement, can play around with this if you'd like
            float instantAcceleration = 1000f; // adjust as needed
            _networkCharacterController.acceleration = instantAcceleration;
            _networkCharacterController.braking = instantAcceleration;
            _networkCharacterController.rotationSpeed = 0;

            if (_playerManager._playerController.IsBot)
            {
                _networkCharacterController.maxSpeed = _playerManager._playerStats.MoveSpeed - 1;
            }
            else
            {
                _networkCharacterController.maxSpeed = _playerManager._playerStats.MoveSpeed;
            }

            _networkCharacterController.Move(direction);
        }

        // rotates the bots
        public void BotRotate(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude != 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _cacheTransform.rotation = Quaternion.Slerp(_cacheTransform.rotation, targetRotation, Runner.DeltaTime * 12);
            }
        }

        // rotates the players
        protected void RotatePlayer(Vector3 direction)
        {
            if (_playerManager._playerStats.IsShooting)
            {
                // if shooting directly set rotation based on input
                if (direction.sqrMagnitude != 0)
                {
                    _cacheTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
            else
            {
                // if not shooting rotate towards input direction
                if (direction.sqrMagnitude != 0)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _cacheTransform.rotation = Quaternion.Slerp(_cacheTransform.rotation, targetRotation, Runner.DeltaTime * 15);
                }
            }
        }
    }
}

