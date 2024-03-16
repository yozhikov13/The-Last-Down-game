﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterInput))]
    public class PlayerCharacterTdLook : PlayerCharacterBehaviour
    {
        #region INSPECTOR

        [Header("Components"), SerializeField]
        private Camera topDownPlayerCamera;
        [SerializeField]
        private PlayerCharacterInput inputController = null;
        [SerializeField]
        private PlayerCharacterMovement movementController = null;

        [Header("Distance Settings"), SerializeField, Range(5f, 100f)]
        private float minDistance = 5f;
        [SerializeField, Range(5f, 100f)]
        private float maxDistance = 15f;
        [SerializeField, Range(5f, 100f)]
        private float startDistance = 15f;
        [SerializeField, Range(1f, 15f)]
        private float distanceSmoothTime = 5f;
        [SerializeField, Range(0.01f, 1f)]
        private float distanceScrollPower = 1f;
        [SerializeField]
        private bool useOffsetDistance = true;
        [SerializeField, Range(1f, 25f)]
        private float maxOffsetDistance = 5f;
        [SerializeField, Range(35f, 90f)]
        private float pitchAngle = 65f;

        [Header("Padding Settings"), SerializeField, Range(5f, 100f)]
        private float minHorizontal = 100f;
        [SerializeField, Range(5f, 100f)]
        private float minVertical = 100f;
        [SerializeField, Range(5f, 100f)]
        private float maxHorizontal = 100f;
        [SerializeField, Range(5f, 100f)]
        private float maxVertical = 100f;
        [SerializeField]
        private bool usePadding = false;

        [Header("Movement Settings"), SerializeField, Range(1f, 5f)]
        private float followSmoothTime = 2f;
        [SerializeField, Range(1f, 10f)]
        private float rotationSencitivity = 3f;

        [Header("Misc Settings"), SerializeField]
        protected bool resetCameraAfterDestroy = true;

        #endregion

        /// <summary>
        /// The starting parent of the camera. It is necessary to return the camera to its original place after the destruction of the current object
        /// </summary>
        private Transform initialCameraParent;

        /// <summary>
        /// The starting position of the camera. It is necessary to return the camera to its original place after the destruction of the current object
        /// </summary>
        private Vector3 initialCameraPosition;

        /// <summary>
        /// The starting rotation of the camera. It is necessaryto return the camera to its original angle after the destruction of the current object
        /// </summary>
        private Quaternion initialCameraRotation;

        private GameObject cameraRoot = null;
        private GameObject cameraYPoint = null;
        private GameObject cameraXPoint = null;
        private GameObject cameraOffsetPoint = null;

        private float currentCameraDistance = 0f;
        private float currentCameraYawAngle = 0f;

        public override bool IsReady => topDownPlayerCamera
            && cameraRoot
            && cameraYPoint
            && cameraXPoint
            && inputController
            && cameraOffsetPoint
            && movementController;

        [Client]
        private void Update()
        {
            if (isLocalPlayer && IsReady)
            {
                UpdateCameraDistance();
                UpdateCameraPosition();
                UpdateCameraOffset();
                UpdateCameraRotation();
            }
        }

        protected virtual void OnDestroy()
        {
            if (isLocalPlayer)
            {
                if (resetCameraAfterDestroy && topDownPlayerCamera)
                {
                    topDownPlayerCamera.transform.localPosition = initialCameraPosition;
                    topDownPlayerCamera.transform.localRotation = initialCameraRotation;
                    topDownPlayerCamera.transform.SetParent(initialCameraParent);

                    topDownPlayerCamera = null;
                }
            }
        }

        /// <summary>
        /// When local player is created
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            RecalculateDistance();
            CreateCameraControls();
        }

        /// <summary>
        /// Create camera control elements
        /// </summary>
        private void CreateCameraControls()
        {
            if (topDownPlayerCamera == null)
                topDownPlayerCamera = Camera.main;

            if (topDownPlayerCamera == null)
            {
                var cameraObject = new GameObject("--PlayerCamera");
                var cameraComponent = cameraObject.AddComponent<Camera>();

                topDownPlayerCamera = cameraComponent;
            }

            if (topDownPlayerCamera.transform.parent != null)
            {
                initialCameraPosition = topDownPlayerCamera.transform.localPosition;
                initialCameraRotation = topDownPlayerCamera.transform.localRotation;
                initialCameraParent = topDownPlayerCamera.transform.parent;
            }
            else
            {
                initialCameraPosition = topDownPlayerCamera.transform.position;
                initialCameraRotation = topDownPlayerCamera.transform.rotation;
            }

            cameraRoot = new GameObject("-- PLAYER_CAMERA_ROOT");

            cameraYPoint = new GameObject("CamYPoint");
            cameraYPoint.transform.SetParent(cameraRoot.transform);
            cameraYPoint.transform.localPosition = Vector3.zero;
            cameraYPoint.transform.localRotation = Quaternion.identity;

            cameraXPoint = new GameObject("CamXPoint");
            cameraXPoint.transform.SetParent(cameraYPoint.transform);
            cameraXPoint.transform.localPosition = Vector3.zero;
            cameraXPoint.transform.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);

            topDownPlayerCamera.transform.SetParent(cameraXPoint.transform);
            topDownPlayerCamera.transform.localPosition = new Vector3(0f, 0f, startDistance * -1f);
            topDownPlayerCamera.transform.localRotation = Quaternion.identity;

            cameraOffsetPoint = new GameObject("CameraOffsetPoint");
            cameraOffsetPoint.transform.SetParent(transform);
            cameraOffsetPoint.transform.localPosition = Vector3.zero;

            cameraRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        /// <summary>
        /// Just calculates the distance. If max distance less than min
        /// </summary>
        private void RecalculateDistance()
        {
            if (minDistance >= maxDistance)
            {
                maxDistance = minDistance + 1f;
            }

            currentCameraDistance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        }

        /// <summary>
        /// Updates camera offset in front of character
        /// </summary>
        private void UpdateCameraOffset()
        {
            if (inputController.IsMoving() && !IsCharacterOutOfBounds())
            {
                cameraOffsetPoint.transform.localPosition = Vector3.Lerp(cameraOffsetPoint.transform.localPosition, Vector3.forward * maxOffsetDistance, Time.deltaTime * followSmoothTime);
            }
            else
            {
                cameraOffsetPoint.transform.localPosition = Vector3.Lerp(cameraOffsetPoint.transform.localPosition, Vector3.zero, Time.deltaTime * followSmoothTime);
            }
        }

        /// <summary>
        /// Updates camera rotation
        /// </summary>
        private void UpdateCameraRotation()
        {
            // Если нажата кнопка мыши вращения камеры
            if (inputController.IsRotateCameraMode())
            {
                // Устанавливаем новый угол камеры
                currentCameraYawAngle += inputController.MouseX() * rotationSencitivity;
            }

            // Интерполируем угол камеры плавно
            float t_newAngle = Mathf.LerpAngle(cameraRoot.transform.rotation.eulerAngles.y, currentCameraYawAngle, Time.deltaTime * 4f);
            cameraRoot.transform.rotation = Quaternion.Euler(0f, t_newAngle, 0f);
        }

        /// <summary>
        /// Updates distance between camera and character
        /// </summary>
        private void UpdateCameraDistance()
        {
            currentCameraDistance = Mathf.Clamp(currentCameraDistance - (inputController.MouseVerticalScroll() * distanceScrollPower), minDistance, maxDistance);
            topDownPlayerCamera.transform.localPosition = Vector3.Lerp(topDownPlayerCamera.transform.localPosition, new Vector3(0f, 0f, currentCameraDistance * -1f), Time.deltaTime * distanceSmoothTime);
        }

        /// <summary>
        /// Updates camera position
        /// </summary>
        private void UpdateCameraPosition()
        {
            if (useOffsetDistance)
            {
                cameraRoot.transform.position = Vector3.Lerp(cameraRoot.transform.position, cameraOffsetPoint.transform.position, Time.deltaTime * followSmoothTime);
            }
            else
            {
                cameraRoot.transform.position = transform.position;
            }
        }

        private bool IsCharacterOutOfBounds()
        {
            Vector3 t_screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (usePadding) return (t_screenPos.x <= minHorizontal || t_screenPos.y <= minVertical || t_screenPos.x >= Screen.width - maxHorizontal || t_screenPos.y >= Screen.height - maxVertical);
            else return false;
        }

        /// <summary>
        /// Gets camera root rotation angle in <see cref="Quaternion"/>
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRotation()
        {
            return cameraRoot.transform.rotation;
        }

        /// <summary>
        /// Gets camera root rotation angle in <see cref="Vector3"/>
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEulerRotation()
        {
            return cameraRoot.transform.rotation.eulerAngles;
        }

        /// <summary>
        /// Gets camera root yaw angle in <see cref="Quaternion"/>
        /// </summary>
        /// <returns></returns>
        public float GetQuaternionYaw()
        {
            return cameraRoot.transform.rotation.y;
        }

        /// <summary>
        /// Gets camera root yaw angle in <see cref="Vector3"/>
        /// </summary>
        /// <returns></returns>
        public float GetEulerYaw()
        {
            return cameraRoot.transform.rotation.eulerAngles.y;
        }

        /// <summary>
        /// Direction to the point at what the character is looking in armed mode
        /// </summary>
        /// <returns></returns>
        public Vector3 AimDirection()
        {
            if (inputController.ScreenPointHit(out RaycastHit hit))
            {
                return hit.point - (transform.position + new Vector3(0f, 1.4f, 0f));
            }
            else
            {
                return Vector3.forward;
            }
        }

        /// <summary>
        /// Character aim yaw angle
        /// </summary>
        /// <returns></returns>
        public float AimYawAngle()
        {
            return Vector3.Angle(transform.forward, new Vector3(AimDirection().x, 0f, AimDirection().z));
        }

        /// <summary>
        /// Character aim yaw signed angle
        /// </summary>
        /// <returns></returns>
        public float AimSignedYawAngle()
        {
            return Vector3.SignedAngle(transform.forward, new Vector3(AimDirection().x, 0f, AimDirection().z), Vector3.up);
        }

        /// <summary>
        /// Character aim pitch signed angle
        /// </summary>
        /// <returns></returns>
        public float AimSignedPitchAngle()
        {
            return Vector3.SignedAngle(transform.forward, new Vector3(AimDirection().x, 0f, AimDirection().z), Vector3.left);
        }
    }
}