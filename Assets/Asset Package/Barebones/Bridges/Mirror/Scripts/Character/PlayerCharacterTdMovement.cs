using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterInput), typeof(CharacterController))]
    public class PlayerCharacterTdMovement : PlayerCharacterMovement
    {
        [Header("Components"), SerializeField]
        protected PlayerCharacterTdLook lookController;

        [Header("Rotation Settings"), SerializeField, Range(5f, 20f)]
        private float rotationSmoothTime = 5f;

        /// <summary>
        /// The direction to which the character is required to look
        /// </summary>
        private Quaternion playerTargetDirectionAngle;

        protected override void UpdateMovement()
        {
            if (characterController.isGrounded)
            {
                var aimDirection = lookController.AimDirection();

                // If we are moving but not armed mode
                if (inputController.IsMoving() && !inputController.IsArmed())
                {
                    // Вычисляем новый угол поворота игрока
                    Vector3 t_currentDirection = inputController.MovementAxisDirection();

                    if (!t_currentDirection.Equals(Vector3.zero))
                    {
                        playerTargetDirectionAngle = Quaternion.LookRotation(t_currentDirection) * lookController.GetRotation();
                    }
                }
                // If we are moving and armed mode
                else if (inputController.IsMoving() && inputController.IsArmed())
                {
                    playerTargetDirectionAngle = Quaternion.LookRotation(new Vector3(aimDirection.x, 0f, aimDirection.z));
                }
                // If we are not moving and not armed mode
                else if (!inputController.IsMoving() && inputController.IsArmed())
                {
                    playerTargetDirectionAngle = Quaternion.LookRotation(new Vector3(aimDirection.x, 0f, aimDirection.z));
                }

                // Rotate character to target direction
                transform.rotation = Quaternion.Lerp(transform.rotation, playerTargetDirectionAngle, Time.deltaTime * rotationSmoothTime);

                // Let's calculate input direction
                var inputAxisAngle = inputController.MovementAxisDirection().Equals(Vector3.zero) ? Vector3.zero : Quaternion.LookRotation(inputController.MovementAxisDirection()).eulerAngles;
                var compositeAngle = inputAxisAngle - transform.eulerAngles;

                calculatedInputDirection = Quaternion.Euler(compositeAngle) * lookController.GetRotation() * transform.forward * inputController.MovementAxisMagnitude();

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
    }
}
