using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Collections;

public class PlayerClass : NetworkBehaviour
{

	public Camera cam;

	#region NetworkIdentity, Weapon and Movement components references

	[field: SerializeField]
	public NetworkIdentity netID { get; private set; }

	[field: SerializeField]
	public ExpPlayerMovement pMove { get; private set; }

	[field: SerializeField]
	public PlayerWeapon pWeapon { get; private set; }

	[field: SerializeField]
	public SensorController pSensors { get; set; }
	#endregion


	#region Character avatar information
	[Header("First-person and third-person avatars")]
	[SerializeField]
	private AvatarInfo FPPrefab;
	[SerializeField]
	private AvatarInfo TPPrefab;



	[field: SerializeField]
	public GameObject CharArms { get; private set; }

	[field: SerializeField]
	public Animator CharAnimator { get; private set; }
	#endregion

	#region Character stats
	[SerializeField]
	private float healPerSecond;
	[SerializeField]
	private float healDelay;
	private IEnumerator healRoutine;
	[SerializeField]
	[SyncVar]
	private float health;
	[SerializeField]
	[SyncVar]
	private float armor;
	[SerializeField]
	[SyncVar]
	private float noiseness;

	public float walkSpeed;

	public float runSpeed;

	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			if (health > value)
				onDamage?.Invoke();
			if(health != value)
				OnHealthChanged?.Invoke();
			if (value <= 0)
				OnDead.Invoke(this);
			health = value;
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
	public UnityEvent onDamage;
	public delegate void HealthChangedDelegate();
	public event HealthChangedDelegate OnHealthChanged;
	public delegate void DeadDelegate(PlayerClass pc);
	public event DeadDelegate OnDead;
	#endregion

	#region User settings
	public float RotateVerSpeed { get; private set; }

	public float RotateHorSpeed { get; private set; }
	#endregion


	public Ray GetShootRay()
	{

		Vector3 vec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
		Ray shootRay = cam.ScreenPointToRay(vec);
		return shootRay;
	}

	public void SetFPPosition (Vector3 pos)
	{
		FPPrefab.gameObject.transform.localPosition = pos;
	}

	private void Start()
	{
		onDamage.AddListener(StartHeal);
		GetPlayerInfo();
		ApplyCharStats();
		ApplyUserSettings();

		if (!isLocalPlayer)
		{
			SetServerAvatar();
		}
		else
		{
			SetClientAvatar();
		}
	}

    public override void OnStartServer()
    {
        base.OnStartServer();
		var mm = FindObjectOfType<Respawner>();
		//mm.AddPlayer(this);
    }

    /// <summary>
    /// Функция отвечает за получение ссылок на компоненты игрока и пересылает их в InputManager.
    /// </summary>
    private void GetPlayerInfo()
	{
		netID = this.GetComponent<NetworkIdentity>();
		pMove = this.GetComponent<ExpPlayerMovement>();
		pWeapon = this.GetComponent<PlayerWeapon>();
	}

	/// <summary>
	/// Функция отвечает за получение данных "серверного" аватара игрока.
	/// </summary>
	private void SetServerAvatar()
	{
		CharArms = TPPrefab.charArms;
		CharAnimator = TPPrefab.charAnimator;
	}

	private void SetClientAvatar()
	{
		CharArms = FPPrefab.charArms;
		CharAnimator = FPPrefab.charAnimator;
	}

	/// <summary>
	/// Функция отвечает отвечает за установку актуальной информации о текущем "клиентском" аватаре.
	/// </summary>
	/// <param name="charArms"></param>
	/// <param name="charAnimator"></param>
	public void SetClientArms(AvatarInfo avaInfo)
	{
		if (!isLocalPlayer)
			return;

		if (CharArms != null)
			avaInfo.charArms.transform.rotation = CharArms.transform.rotation;

		CharArms = avaInfo.charArms;
		CharAnimator = avaInfo.charAnimator;
	}

	/// <summary>
	/// Функция отвечает за установку характеристик персонажа, получаются из БД.
	/// </summary>
	public void ApplyCharStats()
	{
		runSpeed = CharStats.Instance.runSpeed;
		walkSpeed = CharStats.Instance.walkSpeed;
		Armor = CharStats.Instance.armor;
		Noiseness = CharStats.Instance.noiseness;
		Health = CharStats.Instance.health;
	}

	/// <summary>
	/// Функция отвечает за применение пользовательских настроек к конроллеру.
	/// </summary>
	public void ApplyUserSettings ()
	{

		RotateHorSpeed = PlayerSettings.Instance.rotateSensitivityHor;
		RotateVerSpeed = PlayerSettings.Instance.rotateSensitivityVer;

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
				Health = 100;
        }
    }
}
