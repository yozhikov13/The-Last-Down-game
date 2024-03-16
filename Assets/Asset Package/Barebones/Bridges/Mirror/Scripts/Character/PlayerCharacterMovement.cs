using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterInput), typeof(CharacterController))]
    public class PlayerCharacterMovement : PlayerCharacterBehaviour
    {
        #region INSPECTOR

        [Header("Gravity Settings"), SerializeField]
        protected float gravityMultiplier = 3f;
        [SerializeField, Range(0, 100)]
        protected float stickToGroundPower = 5f;

        [Header("Movement Settings"), SerializeField, Range(0, 100)]
        protected float walkSpeed = 5f;
        [SerializeField, Range(0, 100)]
        protected float runSpeed = 10f;

        [Header("Jump Settings"), SerializeField]
        protected bool jumpIsAllowed = true;
        [SerializeField, Range(0, 100)]
        protected float jumpPower = 8f;
        [SerializeField, Range(0, 100)]
        protected float jumpRate = 1f;

        [Header("Components"), SerializeField]
        protected PlayerCharacterInput inputController;
        [SerializeField]
        protected CharacterController characterController;

        #endregion

        /// <summary>
        /// Check if running mode is allowed for character
        /// </summary>
        [SyncVar]
        protected bool runningIsAllowed = true;

        /// <summary>
        /// Current calculated movement direction
        /// </summary>
        protected Vector3 calculatedMovementDirection = new Vector3();
        protected Vector3 calculatedInputDirection = new Vector3();
        protected float nextJumpTime = 0f;

        /// <summary>
        /// Check if this behaviour is ready
        /// </summary>
        public override bool IsReady => inputController && characterController;

        /// <summary>
        /// Speed of the character
        /// </summary>
        public float CurrentMovementSpeed { get; protected set; }

        /// <summary>
        /// Check if jumping is available for the character
        /// </summary>
        public bool IsJumpAvailable { get; protected set; }

        /// <summary>
        /// If character is currently walking
        /// </summary>
        public bool IsWalking { get; protected set; }

        /// <summary>
        /// If character is currently running
        /// </summary>
        public bool IsRunning { get; protected set; }

        [Client]
        protected void Update()
        {
            if (isLocalPlayer && IsReady)
            {
                UpdateJumpAvailability();
                UpdateMovementStates();
                UpdateMovement();
            }
        }

        protected virtual void UpdateJumpAvailability()
        {
            if (jumpIsAllowed)
            {
                IsJumpAvailable = Time.time >= nextJumpTime;
            }
            else
            {
                IsJumpAvailable = jumpIsAllowed;
            }
        }

        protected virtual void UpdateMovementStates()
        {
            IsWalking = inputController.IsMoving();
            IsRunning = IsWalking && inputController.IsRunnning() && runningIsAllowed;
        }

        protected virtual void UpdateMovement()
        {
            if (characterController.isGrounded)
            {
                calculatedInputDirection = transform.forward * inputController.Vertical() + transform.right * inputController.Horizontal();

                if (IsRunning && runningIsAllowed)
                {
                    CurrentMovementSpeed = runSpeed;
                }
                else if (IsWalking)
                {
                    CurrentMovementSpeed = walkSpeed;
                }

                calculatedMovementDirection.y = -stickToGroundPower;
                calculatedMovementDirection.x = calculatedInputDirection.x * CurrentMovementSpeed;
                calculatedMovementDirection.z = calculatedInputDirection.z * CurrentMovementSpeed;

                if (inputController.IsJump() && IsJumpAvailable)
                {
                    calculatedMovementDirection.y = jumpPower;
                    nextJumpTime = Time.time + jumpRate;
                }
            }
            else
            {
                calculatedMovementDirection += Physics.gravity * gravityMultiplier * Time.deltaTime;
            }

            characterController.Move(calculatedMovementDirection * Time.deltaTime);
        }

        /// <summary>
        /// Enable or disable running mode
        /// </summary>
        /// <param name="value"></param>
        [Server]
        public void AllowRunning(bool value)
        {
            runningIsAllowed = value;
        }
    }
}
