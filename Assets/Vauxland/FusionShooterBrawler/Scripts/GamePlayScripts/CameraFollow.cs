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
    public enum CameraMode
    {
        TopDown
    }

    public class CameraFollow : MonoBehaviour
    {
        public CameraMode cameraMode = CameraMode.TopDown;
        public Camera targetCamera; // the camera in the scene
        public Transform target; // the player as the target to follow
        public Vector3 targetOffset;
        [Header("Rotation")]
        public float xRotation = 45.0f; // xRotation of the camera
        public float yRotation = 45.0f; // yRotation of the camera
        [Header("Zoom")]
        public float zoomDistance = 10.0f; // distance to the player

        [Header("Camera Shake")]
        public bool useCameraShake = true; // use camera shake or not
        public float shakeDuration = 0.1f;  // duration of the shake effect
        public float shakeMagnitude = 0.2f; // magnitude of the shake effect
        private Vector3 shakeOffset; // current shake offset
        private float shakeTimeRemaining; // time remaining for the shake

        private float noiseSeedX; // seed values for Perlin noise
        private float noiseSeedY;

        public GameObject targetFollower; // the target of our camera

        public Transform CacheCameraTransform { get; private set; }

        public Camera CacheCamera
        {
            get { return targetCamera; }
        }

        void Awake()
        {
            // set our target camera
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();

            CacheCameraTransform = CacheCamera.transform;

            if (targetFollower == null)
            {
                targetFollower = new GameObject("CameraTarget");
                targetFollower.transform.position = target != null ? target.position : Vector3.zero;
            }
        }

        void OnDestroy()
        {
            if (targetFollower != null)
                Destroy(targetFollower);
        }

        // shake the camera
        public void StartShake()
        {
            shakeTimeRemaining = shakeDuration;
            noiseSeedX = Random.Range(0f, 100f); // x position
            noiseSeedY = Random.Range(0f, 100f); // y position
        }

        void LateUpdate()
        {
            // gets our camera target
            if (PlayerNetworkController.LocalPlayer != null)
            {
                target = PlayerNetworkController.LocalPlayer.GetComponent<PlayerMovementManager>()._cacheTransform;
            }

            if (target == null)
                return;

            switch (cameraMode)
            {
                case CameraMode.TopDown:
                    UpdateTopDownCamera();
                    break;
            }
        }

        void UpdateTopDownCamera()
        {
            // set the position to follow
            targetFollower.transform.position = target.position;
            targetFollower.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

            // update camera shake
            UpdateCameraShake();

            // calculate camera position
            Vector3 cameraPosition = targetFollower.transform.position - targetFollower.transform.forward * zoomDistance + targetOffset + shakeOffset;
            targetCamera.transform.position = cameraPosition;
            targetCamera.transform.LookAt(targetFollower.transform.position);

            // adjust orthographic size if necessary
            if (targetCamera.orthographic)
                targetCamera.orthographicSize = zoomDistance / 2;
        }

        void UpdateCameraShake()
        {
            // camera shake logic
            if (shakeTimeRemaining > 0)
            {
                shakeTimeRemaining -= Time.deltaTime;

                float normalizedTime = 1f - (shakeTimeRemaining / shakeDuration);

                // apply damping to the shake magnitude
                float dampingFactor = Mathf.Lerp(shakeMagnitude, 0, normalizedTime);

                // update noise seeds over time to get smooth movement
                noiseSeedX += Time.deltaTime * 10f;
                noiseSeedY += Time.deltaTime * 10f;

                // here we generate smooth shake offsets using Perlin noise
                float shakeX = (Mathf.PerlinNoise(noiseSeedX, 0f) - 0.5f) * 2f * dampingFactor;
                float shakeY = (Mathf.PerlinNoise(0f, noiseSeedY) - 0.5f) * 2f * dampingFactor;

                shakeOffset = new Vector3(shakeX, shakeY, 0f);
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }
    }
}

