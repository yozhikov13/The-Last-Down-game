using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    [DisallowMultipleComponent]
    public class PlayerCharacterInput : PlayerCharacterBehaviour
    {
        public void SetMouseActive(bool value)
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public virtual float Horizontal()
        {
            return Input.GetAxis("Horizontal");
        }

        public virtual float Vertical()
        {
            return Input.GetAxis("Vertical");
        }

        public float MouseVerticalScroll()
        {
            return Input.mouseScrollDelta.y;
        }

        public virtual float MouseX()
        {
            return Input.GetAxis("Mouse X");
        }

        public virtual float MouseY()
        {
            return Input.GetAxis("Mouse Y");
        }

        public virtual bool IsRotateCameraMode()
        {
            return Input.GetMouseButton(2);
        }

        public virtual Vector3 MovementAxisDirection()
        {
            return new Vector3(Horizontal(), 0.0f, Vertical()).normalized;
        }

        public virtual float MovementAxisMagnitude()
        {
            return MovementAxisDirection().magnitude;
        }

        public virtual bool IsMoving()
        {
            bool hasHorizontalInput = !Mathf.Approximately(Horizontal(), 0f);
            bool hasVerticalInput = !Mathf.Approximately(Vertical(), 0f);
            return hasHorizontalInput || hasVerticalInput;
        }

        public virtual bool IsArmed()
        {
            return Input.GetMouseButton(1);
        }

        public virtual bool IsCrouching()
        {
            return Input.GetKeyDown(KeyCode.C);
        }

        public virtual bool IsJump()
        {
            return Input.GetButton("Jump");
        }

        public virtual bool IsRunnning()
        {
            return Input.GetKey(KeyCode.LeftShift) && IsMoving();
        }

        public bool ScreenPointHit(out RaycastHit hit, float maxCheckDistance = Mathf.Infinity)
        {
            Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(t_ray, out hit, maxCheckDistance);
        }
    }
}