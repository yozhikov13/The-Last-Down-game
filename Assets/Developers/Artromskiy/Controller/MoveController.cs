using UnityEngine;
using Mirror;
using System.Collections;

namespace MatchMaking
{
    public class MoveController : NetworkBehaviour
    {
        private Player player;
        private bool isTps => player.isTps;
        private float walkSpeed => player.walkSpeed;
        private float runSpeed => player.runSpeed;
        private bool jumpAvaliable => player.colliders.canJump;

        private Vector3 lastSpeed;
        private CharacterController characterController;

        [SyncVar]
        private bool canMove = true;
        private IEnumerator jumpRoutine;
        private bool isHost => isClient && isServer;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            player = GetComponent<Player>();
        }

        private void Update()
        {
            if (characterController)
            {
                if (canMove)
                    characterController.SimpleMove(transform.rotation * lastSpeed);
                if (jumpRoutine == null)
                    characterController.Move(Vector3.down * 10f);
            }
        }

        [Client]
        public void SetSpeed(Vector2 speed)
        {
            SpeedSetting(speed);
            CmdSetSpeed(speed);
            SetAnimations(speed);
        }

        [Command]
        private void CmdSetSpeed(Vector2 speed)
        {
            //SpeedSetting(speed);
            SetAnimations(speed);
        }

        private void SpeedSetting(Vector2 speed)
        {
            if (!canMove)
                speed = Vector2.zero;
            if (speed.magnitude > 1)
                speed.Normalize();
            if (speed.y >= 0.8)
                lastSpeed = new Vector3(speed.x * walkSpeed, 0f, speed.y * runSpeed);
            else
                lastSpeed = new Vector3(speed.x * walkSpeed, 0f, speed.y * walkSpeed);
        }

        private void SetAnimations(Vector2 speed)
        {
            if (!canMove)
                speed = Vector2.zero;
            if (speed.magnitude > 1)
                speed.Normalize();
            if (isTps)
            {
                player.tpsAnim.SetFloat("Vertical", speed.y);
                player.tpsAnim.SetFloat("Horizontal", speed.x);
            }
            else
            {
                var running = speed.y >= 0.8;
                player.fpsAnim.SetBool("Run", running);
                player.fpsAnim.SetBool("Walk", !running);
                player.fpsAnim.SetFloat("WalkSpeed", Mathf.Max(Mathf.Abs(speed.x), Mathf.Abs(speed.y)));
            }
        }

        private void SetJumpAnim(bool jumping)
        {
            if (isTps)
                player.tpsAnim.SetBool("isJumping", jumping);
        }

        [Client]
        public void SetRotation(Vector2 rot)
        {
            transform.rotation *= Quaternion.Euler(0, rot.x, 0);
            //CmdSetRotation(rot.x);
        }

        [Command]
        private void CmdSetRotation(float y)
        {
            transform.rotation *= Quaternion.Euler(0, y, 0);
        }

        [Server]
        public void BlockMove(bool value)
        {
            canMove = value;
            if (value == false)
            {
                SpeedSetting(Vector2.zero);
                SetAnimations(Vector2.zero);
            }
            else
            {
                SpeedSetting(lastSpeed);
                SetAnimations(lastSpeed);
            }
        }

        public void Jump()
        {
            JumpSetting();
            CmdJump();
        }

        private void CmdJump()
        {
            if (isHost)
                return;
            JumpSetting();
        }

        private void JumpSetting()
        {
            if (jumpRoutine == null && canMove && jumpAvaliable)
            {
                jumpRoutine = JumpCicle(1, 1, 1);
                StartCoroutine(jumpRoutine);
            }
        }

        private IEnumerator JumpCicle(float length, float height, float tMain)
        {
            float tCurr = 0;
            float prevY = 0;
            canMove = false;
            SetJumpAnim(true);
            do
            {
                Debug.Log(characterController.isGrounded);
                var currY = (tCurr + Time.deltaTime) / tMain * length;
                characterController.Move(transform.rotation * new Vector3(0, Parabola(length, height, prevY) - Parabola(length, height, currY), length / tMain * Time.deltaTime));
                prevY = currY;
                tCurr += Time.deltaTime;
                yield return null;
            }
            while (tCurr < tMain || !characterController.isGrounded);
            SetJumpAnim(false);
            canMove = true;
            jumpRoutine = null;
            yield break;
        }

        private float Parabola(float length, float height, float x)
        {
            return ((4 * height * x) / length) * ((x / length) - 1) + height;
        }

        [Server]
        public void SetDefaultStats()
        {
            lastSpeed = Vector2.zero;
            canMove = true;
            transform.rotation = Quaternion.identity;
            SetJumpAnim(false);
            if (jumpRoutine != null)
                StopCoroutine(jumpRoutine);
            jumpRoutine = null;
        }
    }
}
