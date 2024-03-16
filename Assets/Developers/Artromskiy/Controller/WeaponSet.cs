using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchMaking
{
    public class WeaponSet : NetworkBehaviour
    {
        [SerializeField]
        private Weapon[] weapons;
        [SerializeField]
        private GameObject[] tpsWeapon;
        [SerializeField]
        private GameObject[] fpsWeapon;
        [SerializeField]
        private Weapon melle;
        public Transform shootPoint;
        public Transform shootBone;
        private int currentIndex;

        private Player player;
        private bool isTps => player.isTps;
        private bool knifeAval => player.colliders.knifeAttackAvailable;

        public float lastVert = 0;

        private bool isHost => isClient && isServer;

        private void Awake()
        {
            player = GetComponent<Player>();
            player.fpsAnim = fpsWeapon[currentIndex].gameObject.GetComponent<Animator>();
        }

        public void SetRotation(Vector2 rot)
        {
            var sp = shootBone.eulerAngles;
            sp.x -= rot.y;
            if (sp.x > 180)
                sp.x -= 360;
            sp.x = Mathf.Clamp(sp.x, -90, 90);
            shootBone.eulerAngles = sp;
            CmdSetRotation(rot.y);
            VerticalAnimation(sp.x);
        }

        private void CmdSetRotation(float y)
        {
            var sp = shootBone.eulerAngles;
            sp.x -= y;
            if (sp.x > 180)
                sp.x -= 360;
            sp.x = Mathf.Clamp(sp.x, -90, 90);
            shootBone.eulerAngles = sp;
            VerticalAnimation(y);
        }

        private void VerticalAnimation(float y)
        {
            lastVert = y;
            if (isTps)
            {
                player.tpsAnim.SetFloat("VerticalCamera", -(y) / 2);
            }
            else
            {
                var euler = player.fpsAnim.gameObject.transform.eulerAngles;
                player.fpsAnim.gameObject.transform.eulerAngles = new Vector3(y, euler.y, euler.z);
            }
        }

        #region Change of Weapons

        private void SetWeapon(int index)
        {
            if (index >= weapons.Length || index < 0)
            {
                return;
            }
            weapons[currentIndex]?.gameObject.SetActive(false);
            weapons[index].gameObject.SetActive(true);
            var arr = isTps ? tpsWeapon : fpsWeapon;
            arr[currentIndex]?.gameObject.SetActive(false);
            arr[index].gameObject.SetActive(true);
            currentIndex = index;
            if (!isTps)
            {
                player.fpsAnim = arr[index].gameObject.GetComponent<Animator>();
            }
            VerticalAnimation(lastVert);
        }

        [ClientRpc]
        private void RpcSetWeapon(int index)
        {
            SetWeapon(index);
        }

        public void SetSkillWeapon()
        {
            CmdSetSkillWeapon();
        }

        private void CmdSetSkillWeapon()
        {
            if (weapons.Length != 5)
                return;
            var skill = GetComponent<SkillComponent>();
            if (skillWeapons.Contains(skill.GetType()))
            {
                SetWeapon(4);
                RpcSetWeapon(4);
            }
        }

        [ContextMenu("Next")]
        public void NextWeapon()
        {
            CmdNextWeapon();
        }

        private void CmdNextWeapon()
        {
            if (currentIndex == 3 || currentIndex == 4)
            {
                SetWeapon(0);
                RpcSetWeapon(0);
            }
            else
            {
                SetWeapon(currentIndex + 1);
                RpcSetWeapon(currentIndex);
            }
        }

        public void PrevWeapon()
        {
            CmdPrevWeapon();
        }

        private void CmdPrevWeapon()
        {
            if (currentIndex == 0 || currentIndex == 4)
            {
                SetWeapon(3);
                RpcSetWeapon(3);
            }
            else
            {
                SetWeapon(currentIndex - 1);
                RpcSetWeapon(currentIndex);
            }
        }
        #endregion

        #region Shoot functions

        public void ClientShoot()
        {
            CmdShoot();
        }

        [Command]
        private void CmdShoot()
        {
            weapons[currentIndex].StartShooting();
            RpcShoot();
        }

        [ClientRpc]
        private void RpcShoot()
        {
            weapons[currentIndex].StartShooting();
        }

        public void ClientStopShoot()
        {
            CmdStopShoot();
        }

        [Command]
        private void CmdStopShoot()
        {
            weapons[currentIndex].EndShooting();
            RpcStopShoot();
        }

        [ClientRpc]
        private void RpcStopShoot()
        {
            weapons[currentIndex].EndShooting();
        }

        public void ClientReload()
        {
            CmdReload();
        }

        [Command]
        private void CmdReload()
        {
            weapons[currentIndex].ForceReload();
            RpcReload();
        }

        [ClientRpc]
        private void RpcReload()
        {
            weapons[currentIndex].ForceReload();
        }

        public void ShootAnim()
        {
            if (isTps)
            {
                player.tpsAnim.Play("Fire");
            }
            else
            {
                player.fpsAnim.Play("Fire");
            }
        }

        public void ReloadAnim()
        {
            if (isTps)
            {
                player.tpsAnim.Play("Reload");
            }
            else
            {
                player.fpsAnim.Play("ReloadOutOfAmmo");
            }
        }

        private void KnifeAnim()
        {
            if (isTps)
                player.tpsAnim.Play("KnifeAttack");
            else
                player.fpsAnim.Play("Knife Attack 2");
        }

        public void KnifeStart()
        {
            if (!knifeAval)
                return;
            KnifeAnim();
            CmdKnifeDown();
        }

        public void KnifeEnd()
        {
            CmdKnifeUp();
        }

        private void CmdKnifeDown()
        {
            melle.StartShooting();
            if (!isHost)
                KnifeAnim();
        }

        private void CmdKnifeUp()
        {
            melle.EndShooting();
        }

        #endregion

        private void SetAvatar(bool tps)
        {
            var arr1 = tps ? tpsWeapon : fpsWeapon;
            var arr2 = tps ? fpsWeapon : tpsWeapon;
            arr1[currentIndex].gameObject.SetActive(true);
            arr2[currentIndex].gameObject.SetActive(false);
        }

        [Client]
        public void Aim(bool value)
        {
            player.tpsAnim.SetBool("Aim", value);
        }

        [Client]
        public void ClientSetAvatar(bool tps)
        {
            SetAvatar(tps);
        }

        [Server]
        public void SetDefaultStats()
        {
            player?.tpsAnim?.SetBool("Aim", false);
            lastVert = 0;
            SetWeapon(0);
            RpcSetWeapon(0);
            SetAvatar(true);

            //���������� ���������� ����� ������ � ����
            foreach (Weapon w in weapons)
            {
                w.RespawnCleaning();
            }
        }

        private static List<Type> skillWeapons = new List<Type> { typeof(Safari), typeof(Antiquarian) };
    }
}
