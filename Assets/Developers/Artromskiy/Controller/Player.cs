using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Collections;

namespace MatchMaking
{
	public class Player : NetworkBehaviour
	{
		#region Heal
		[SerializeField]
		private float healPerSecond;
		[SerializeField]
		private float healDelay;
		private IEnumerator healRoutine;
		#endregion

		public MoveController moveController;
		public WeaponSet weaponSet;
		public WithColliders colliders;

		[SerializeField]
		[SyncVar]
		private float health;
		[SerializeField]
		[SyncVar]
		private float armor;
		[SerializeField]
		[SyncVar]
		private float noiseness;
		[SerializeField]
		[SyncVar]
		public float walkSpeed;
		[SerializeField]
		[SyncVar]
		public float runSpeed;

		public bool isTps;
		[HideInInspector]
		public Animator tpsAnim;
		[HideInInspector]
		public Animator fpsAnim;

		public float Health
		{
			get
			{
				return health;
			}
			set
			{
				var h = health;
				health = value;
				if (h > value)
					OnDamage?.Invoke();
				if (h < value)
					OnHeal?.Invoke(this);
				if (h <= 0)
					OnDead.Invoke(this);
			}
		}

		public float Armor
		{
			get
			{
				return armor;
			}
			set
			{
				armor = value;
			}
		}

		public float Noiseness
		{
			get
			{
				return noiseness;
			}
			set
			{
				noiseness = value;
			}
		}

		public delegate void DamageDelegate();
		public event DamageDelegate OnDamage;
		public delegate void HealDelegate(Player healed);
		public event HealDelegate OnHeal;
		public delegate void DeadDelegate(Player dead);
		public event DeadDelegate OnDead;
		public delegate void DestroyedDelegate();
		public event DestroyedDelegate OnDestroyed;

		private void Awake()
		{
			GetPlayerInfo();
		}

		private void Start()
		{
			SetAvatar(true);
			if (isServer)
			{
				GetPlayerInfo();
				OnDamage += StartHeal;
				FindObjectOfType<Respawner>().AddPlayer(this);
			}
		}

		[Server]
		public override void OnStartClient()
		{
			base.OnStartClient();
			SetDefaultStats();
		}

		/// <summary>
		/// Функция отвечает за получение ссылок на компоненты игрока и пересылает их в InputManager.
		/// </summary>
		private void GetPlayerInfo()
		{
			moveController = gameObject.GetComponent<MoveController>();
			weaponSet = gameObject.GetComponent<WeaponSet>();
			colliders = gameObject.GetComponent<WithColliders>();
		}

		[Server]
		/// <summary>
		/// Функция отвечает за установку характеристик персонажа, получаются из БД.
		/// </summary>
		public void SetDefaultStats()
		{
			runSpeed = CharStats.Instance.runSpeed;
			walkSpeed = CharStats.Instance.walkSpeed;
			armor = CharStats.Instance.armor;
			noiseness = CharStats.Instance.noiseness;
			health = CharStats.Instance.health;
			weaponSet.SetDefaultStats();
			moveController.SetDefaultStats();
		}

		private void StartHeal()
		{
			if (healRoutine != null)
				StopCoroutine(healRoutine);
			healRoutine = Heal();
			StartCoroutine(healRoutine);
		}

		IEnumerator Heal()
		{
			yield return new WaitForSeconds(healDelay);
			while (Health < 100)
			{
				Health += healPerSecond;
				yield return new WaitForSeconds(1);
				if (Health > 100)
				{
					Health = 100;
					healRoutine = null;
					yield break;
				}
			}
		}

		public void SetAvatar(bool tps)
		{
			if (tps)
				tpsAnim.gameObject.SetActive(true);
			else
				tpsAnim.gameObject.SetActive(false);
			weaponSet.ClientSetAvatar(tps);
			isTps = tps;
		}

		private void OnDestroy()
		{
			OnDestroyed?.Invoke();
		}
	}
}
