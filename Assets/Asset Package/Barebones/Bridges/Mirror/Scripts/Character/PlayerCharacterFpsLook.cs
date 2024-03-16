using Mirror;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterInput))]
    public class PlayerCharacterFpsLook : PlayerCharacterBehaviour
    {
        #region INSPECTOR

        [Header("Positioning"), SerializeField]
        protected Vector3 cameraPoint = new Vector3(0, 1.75f, 0.15f);

        [Header("Input Settings"), SerializeField]
        protected Vector2Int lookSensitivity = new Vector2Int(8, 8);
        [SerializeField, Range(-90, 0)]
        protected float minLookAngle = -60f;
        [SerializeField, Range(0, 90)]
        protected float maxLookAngle = 60f;

        [Header("Smoothness Settings"), SerializeField]
        protected bool useSmoothness = true;
        [SerializeField, Range(0.01f, 1f)]
        protected float smoothnessTime = 0.1f;

        [Header("Misc Settings"), SerializeField]
        protected bool resetCameraAfterDestroy = true;

        [Header("Components"), SerializeField]
        protected Camera lookCamera;
        [SerializeField]
        protected PlayerCharacterInput inputBehaviour;

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

        /// <summary>
        /// Current camera and character rotation
        /// </summary>
        private Vector3 cameraRotation;

        /// <summary>
        /// Velocity of smoothed rotation vector
        /// </summary>
        private Vector3 currentCameraRotationVelocity;

        /// <summary>
        /// Smoothed rotation vector
        /// </summary>
        private Vector3 smoothedCameraRotation;

        /// <summary>
        /// Check if this behaviour is ready
        /// </summary>
        public override bool IsReady => lookCamera && inputBehaviour;

        [Client]
        protected void Update()
        {
            if (isLocalPlayer && IsReady)
            {
                UpdateCameraPosition();
                UpdateCameraRotation();
            }
        }

        protected virtual void OnDestroy()
        {
            if (isLocalPlayer)
            {
                if (resetCameraAfterDestroy && lookCamera)
                {
                    if (initialCameraParent != null)
                    {
                        lookCamera.transform.localPosition = initialCameraPosition;
                        lookCamera.transform.localRotation = initialCameraRotation;
                        lookCamera.transform.SetParent(initialCameraParent);
                    }
                    else
                    {
                        lookCamera.transform.position = initialCameraPosition;
                        lookCamera.transform.rotation = initialCameraRotation;
                    }

                    lookCamera = null;
                }
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.rotation * cameraPoint, 0.20f);
        }

        public override void OnStartLocalPlayer()
        {
            SetupPlayerCamera();
        }

        /// <summary>
        /// Setup player camera to <see cref="lookCamera"/> field
        /// </summary>
        protected virtual void SetupPlayerCamera()
        {
            if (lookCamera == null)
                lookCamera = Camera.main;

            if (lookCamera == null)
            {
                var cameraObject = new GameObject("--PlayerCamera");
                var cameraComponent = cameraObject.AddComponent<Camera>();

                lookCamera = cameraComponent;
            }

            if (lookCamera.transform.parent != null)
            {
                initialCameraPosition = lookCamera.transform.localPosition;
                initialCameraRotation = lookCamera.transform.localRotation;

                initialCameraParent = lookCamera.transform.parent;
                lookCamera.transform.SetParent(null);
            }
            else
            {
                initialCameraPosition = lookCamera.transform.position;
                initialCameraRotation = lookCamera.transform.rotation;
            }
        }

        protected virtual void UpdateCameraPosition()
        {
            Vector3 newCameraPosition = transform.position + transform.rotation * cameraPoint;
            lookCamera.transform.position = newCameraPosition;
        }

        protected virtual void UpdateCameraRotation()
        {
            cameraRotation.y += inputBehaviour.MouseX() * lookSensitivity.x;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x - inputBehaviour.MouseY() * lookSensitivity.y, minLookAngle, maxLookAngle);

            if (useSmoothness)
            {
                transform.rotation = Quaternion.Euler(0f, smoothedCameraRotation.y, 0f);
                smoothedCameraRotation = Vector3.SmoothDamp(smoothedCameraRotation, cameraRotation, ref currentCameraRotationVelocity, smoothnessTime);
                lookCamera.transform.rotation = Quaternion.Euler(smoothedCameraRotation.x, smoothedCameraRotation.y, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, cameraRotation.y, 0f);
                lookCamera.transform.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0f);
            }
        }
    }
}
