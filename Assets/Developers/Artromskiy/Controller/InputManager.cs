using UnityEngine;
using UnityEngine.EventSystems;
using MatchMaking;
using UnityEngine.Rendering;

namespace MatchMaking
{
public class InputManager : MonoBehaviour
{
		public Player player;
		public CameraController cam;
		private Volume pproc;
		[Range(0.01f, 1f)]
		public float speedMult;
		public GameObject skillButton;

		private void Start()
		{
			pproc = FindObjectOfType<Volume>();
		}

		public void LeftJoystick(Vector2 vec)
		{
			player.moveController.SetSpeed(vec);
		}

		public void RightDrag(Vector2 vec)
		{
			vec *= speedMult;
			player.moveController.SetRotation(vec);
			player.weaponSet.SetRotation(vec);
		}

		public void ShootDown()
		{
			player.weaponSet.ClientShoot();
		}

		public void ShootUp()
		{
			player.weaponSet.ClientStopShoot();
		}

		public void Reload()
		{
			player.weaponSet.ClientReload();
		}

		public void NextWeapon()
		{
			player.weaponSet.NextWeapon();
		}

		public void PrevWeapon()
		{
			player.weaponSet.PrevWeapon();
		}

		public void Skill()
		{

		}

		public void Jump()
		{
			player.moveController.Jump();
		}

		public void KnifeDown()
		{
			player.weaponSet.KnifeStart();
		}

		public void KnifeUp()
		{
			player.weaponSet.KnifeEnd();
		}

		public void ChangeView()
		{
			var avatar = !player.isTps;
			player.SetAvatar(avatar);
			cam.ChangeVision(avatar);
		}

		bool anim = false;
		public void Aim()
		{
			player.weaponSet.Aim(anim);
			anim = !anim;
		}

		bool post = true;
		public void PostProcessing()
		{
				post = !post;
				pproc?.gameObject.SetActive(post);
		}
}
}
