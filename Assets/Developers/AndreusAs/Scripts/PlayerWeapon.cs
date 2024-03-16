using UnityEngine;
using Mirror;

public class PlayerWeapon : NetworkBehaviour
{
	private PlayerClass pClass;
	private CharacterAvatar pAva;
	int currentIndex;

	[field: SerializeField]
	public Transform shootPoint;

	public Transform serverHand;

	[SyncVar]
	public bool isKnifeSlashing;

	[SerializeField]
	public Weapon currentWeaponClassRef;

	private GameObject currentWeapon;
	private GameObject nextWeapon;

	[SerializeField] ///Тут хранятся ссылки на префабы слотов оружия игрока.
	private GameObject[] weaponSlots;

	[SerializeField] ///Тут хранятся ссылки на префабы конкретных видов оружия игрока.
	private GameObject[] weaponInSlots;

	[SerializeField] ///Тут хранятся ссылки на Weapon классы конкретных видов оружия игрока.
	public Weapon[] weaponClassReferences;

	private void Awake()
	{
		pAva = this.GetComponent<CharacterAvatar>();
		pClass = this.GetComponent<PlayerClass>();

	}

	/// <summary>
	/// На старте устанавливаем актуальное оружие игрока из базы данных в оружейные слоты.
	/// </summary>
	private void Start()
	{


		if (!isLocalPlayer && !isServer)
			return;

		weaponSlots = new GameObject[4];
		weaponInSlots = new GameObject[4];
		weaponClassReferences = new Weapon[4];

		weaponSlots[0] = pAva.clientSideAvatar.transform.Find("FirstSlotPrefab").gameObject;
		weaponSlots[1] = pAva.clientSideAvatar.transform.Find("SecondSlotPrefab").gameObject;
		weaponSlots[2] = pAva.clientSideAvatar.transform.Find("ThirdSlotPrefab").gameObject;
		weaponSlots[3] = pAva.clientSideAvatar.transform.Find("EquipmentSlotPrefab").gameObject;

		PlayerClientSetWeaponSlots(WeaponSetInfo.Instance.firstSlotWeapon, 
								   WeaponSetInfo.Instance.secondSlotWeapon,
							       WeaponSetInfo.Instance.thirdSlotWeapon,
							       WeaponSetInfo.Instance.equipmentSlotWeapon);

		if (isServer)
		{
			PlayerServerSetWeaponSlots(WeaponSetInfo.Instance.firstSlotWeapon,
										  WeaponSetInfo.Instance.secondSlotWeapon,
										  WeaponSetInfo.Instance.thirdSlotWeapon,
										  WeaponSetInfo.Instance.equipmentSlotWeapon);
		}						  
									  



	}

	private void Update()
	{/*
		if (isServer)
		{
			isKnifeSlashing = true;
			if (isKnifeSlashing && !pClass.CharAnimator.GetCurrentAnimatorStateInfo(0).IsName("KnifeAttack"))
			{
				isKnifeSlashing = false;

			}
		}
		*/
	}

	/// <summary>
	/// Устанвливаем оружие в слоты игрока на стороне клиента.
	/// </summary>
	/// <param name="first"></param>
	/// <param name="second"></param>
	/// <param name="third"></param>
	/// <param name="equipment"></param>
	private void PlayerClientSetWeaponSlots (string first, string second, string third, string equipment)
	{
		if (first != "null")
		{
			Transform fTrans = weaponSlots[0].transform.Find(first);
			weaponInSlots[0] = fTrans.gameObject;
			weaponClassReferences[0] = fTrans.gameObject.GetComponent<Weapon>();
			currentWeapon = fTrans.gameObject;
			currentWeaponClassRef = fTrans.gameObject.GetComponent<Weapon>();
			fTrans.gameObject.SetActive(true);
			shootPoint = currentWeaponClassRef.shootPoint;

			AvatarInfo tempAvaInfo = new AvatarInfo(fTrans.gameObject.GetComponent<AvatarInfo>().charArms,
													fTrans.gameObject.GetComponent<AvatarInfo>().charAnimator);

			pClass.SetClientArms(tempAvaInfo);

		}

		if (second != "null")
		{

			Transform sTrans = weaponSlots[1].transform.Find(second);
			weaponInSlots[1] = sTrans.gameObject;
			weaponClassReferences[1] = sTrans.gameObject.GetComponent<Weapon>();
		}

		if (third != "null")
		{

			Transform tTrans = weaponSlots[2].transform.Find(third);
			weaponInSlots[2] = tTrans.gameObject;
			weaponClassReferences[2] = tTrans.gameObject.GetComponent<Weapon>();
		}

		if (equipment != "null")
		{
			Transform eTrans = weaponSlots[3].transform.Find(equipment);
			weaponInSlots[3] = eTrans.gameObject;
		}
	}
	/// <summary>
	/// Устанавливаем оружие в слоты игрока на стороне сервера TODO
	/// </summary>
	/// <param name="first"></param>
	/// <param name="second"></param>
	/// <param name="third"></param>
	/// <param name="equipment"></param>

	private void PlayerServerSetWeaponSlots(string first, string second, string third, string equipment)
	{
		if (first != "null")
		{
			Transform fTrans = serverHand.Find(first);
			weaponInSlots[0] = fTrans.gameObject;
			weaponClassReferences[0] = fTrans.gameObject.GetComponent<Weapon>();
			currentWeapon = fTrans.gameObject;
			currentWeaponClassRef = fTrans.gameObject.GetComponent<Weapon>();
			fTrans.gameObject.SetActive(true);


		}

		if (second != "null")
		{

			Transform sTrans = serverHand.Find(second);
			weaponInSlots[1] = sTrans.gameObject;
			weaponClassReferences[1] = sTrans.gameObject.GetComponent<Weapon>();
		}

		if (third != "null")
		{

			Transform tTrans = serverHand.Find(third);
			weaponInSlots[2] = tTrans.gameObject;
			weaponClassReferences[2] = tTrans.gameObject.GetComponent<Weapon>();
		}

		if (equipment != "null")
		{
			Transform eTrans = serverHand.Find(equipment);
			weaponInSlots[3] = eTrans.gameObject;
		}
	}


	/// <summary>
	/// Функция, отвечающая за стрельбу у клиента.
	/// </summary>
	public void ClientShoot()
	{
		currentWeaponClassRef.SetShootingRay(pClass.GetShootRay());
		currentWeaponClassRef.StartShooting();
		if (isLocalPlayer && hasAuthority)
			CmdShoot(pClass.GetShootRay());
	}

	[Command]
	void CmdShoot(Ray ray)
	{
		currentWeaponClassRef.SetShootingRay(ray);
		currentWeaponClassRef.StartShooting();
		RpcShoot();
	}

	[ClientRpc]
	void RpcShoot()
	{
		currentWeaponClassRef.StartShooting();
	}

	public void ClientStopShoot()
	{
		currentWeaponClassRef.EndShooting();
		if (isLocalPlayer && hasAuthority)
			CmdStopShoot();
	}

	[ClientRpc]
	void RpcStopShoot()
	{
		currentWeaponClassRef.EndShooting();
	}

	[Command]
	void CmdStopShoot()
	{
		currentWeaponClassRef.EndShooting();
		RpcStopShoot();
	}

	/// <summary>
	/// Функция, отвечающая за перезарядку оружия у клиента.
	/// </summary>
	public void ClientReload()
	{
		pClass.CharAnimator.Play("ReloadAmmoLeft");
		currentWeaponClassRef.ForceReload();
		if (isLocalPlayer && hasAuthority)
			CmdReload();
	}

	[Command]
	void CmdReload()
	{
		pClass.CharAnimator.Play("ReloadAmmoLeft");
		currentWeaponClassRef.ForceReload();
		RpcReload();
	}

	[ClientRpc]
	void RpcReload()
	{
		pClass.CharAnimator.Play("ReloadAmmoLeft");
		currentWeaponClassRef.ForceReload();
	}


	public void ClientKnifeSlash()
	{
		if (!isKnifeSlashing)
		{
			if (isLocalPlayer)
				CmdKnifeSlash();

			pClass.CharAnimator.Play("Knife Attack 2");
		}
	}

	[Command]
	void CmdKnifeSlash()
	{
		pClass.CharAnimator.Play("KnifeAttack");
	}

	public void ClientChangeWeapon(bool right)
	{


		if (right)
		{
			for (int i = 0; i < weaponInSlots.Length - 1; i++)
			{


				if (currentWeapon == weaponInSlots[i])
				{

					currentWeapon.SetActive(false);

					if ((i + 1) <= weaponInSlots.Length - 2)
					{

						ChangeCurrentWeapon(weaponInSlots[i + 1], i + 1);
						currentWeapon.SetActive(true);
						break;
					}
					else
					{

						ChangeCurrentWeapon(weaponInSlots[0], 0);
						currentWeapon.SetActive(true);
						break;
					}

				}
				else

					continue;

			}
		}
		else
		{

			for (int i = weaponInSlots.Length - 1; i >= 0; i--)
			{


				if (currentWeapon == weaponInSlots[i])
				{

					currentWeapon.SetActive(false);

					if ((i - 1) >= 0)
					{

						ChangeCurrentWeapon(weaponInSlots[i - 1], i - 1);
						currentWeapon.SetActive(true);
						break;
					}
					else
					{

						ChangeCurrentWeapon(weaponInSlots[weaponInSlots.Length - 2], weaponInSlots.Length - 2);
						currentWeapon.SetActive(true);
						break;
					}

				}
				else

					continue;

			}

		}
	}
	/// <summary>
	/// Устанавливаем новое активное оружие игрока после его смены на стороне сервера. Здесь же вызываются функции для смены оружия на стороне клиента TODO
	/// </summary>
	[Command]
	public void CmdChangeWeapon(bool right)
	{


		if (right)
		{
			for (int i = 0; i < weaponInSlots.Length - 1; i++)
			{


				if (currentWeapon == weaponInSlots[i])
				{

					currentWeapon.SetActive(false);

					if ((i + 1) <= weaponInSlots.Length - 2)
					{

						ChangeCurrentWeapon(weaponInSlots[i + 1], i + 1);
						currentWeapon.SetActive(true);
						break;
					}
					else
					{

						ChangeCurrentWeapon(weaponInSlots[0], 0);
						currentWeapon.SetActive(true);
						break;
					}

				}
				else

					continue;

			}
		}
		else
		{

			for (int i = weaponInSlots.Length - 1; i >= 0; i--)
			{


				if (currentWeapon == weaponInSlots[i])
				{

					currentWeapon.SetActive(false);

					if ((i - 1) >= 0)
					{

						ChangeCurrentWeapon(weaponInSlots[i - 1], i - 1);
						currentWeapon.SetActive(true);
						break;
					}
					else
					{

						ChangeCurrentWeapon(weaponInSlots[weaponInSlots.Length - 2], weaponInSlots.Length - 2);
						currentWeapon.SetActive(true);
						break;
					}

				}
				else

					continue;

			}

		}
	}

	private void SetWeapon(int index)
	{
		if(weaponSlots.Length >= index)
        {
			return;
		}
		if(currentWeaponClassRef)
			currentWeaponClassRef.gameObject.SetActive(false);
		currentWeaponClassRef = weaponClassReferences[index];
		currentWeaponClassRef.gameObject.SetActive(true);
		currentIndex = index;
	}

	public void NextWeapon()
    {
		if (currentIndex == 2)
			SetWeapon(0);
		else
			SetWeapon(currentIndex + 1);
	}

	public void PrevWeapon()
	{
		if (currentIndex == 0)
			SetWeapon(2);
		else
			SetWeapon(currentIndex - 1);
	}

	void ChangeCurrentWeapon (GameObject weapon, int weaponRefIndex)
	{
		currentWeapon = weapon;
		currentWeaponClassRef = weaponClassReferences[weaponRefIndex];
		shootPoint = currentWeaponClassRef.shootPoint;

		if (isClient)
		{
			AvatarInfo avaInfo = weapon.GetComponent<AvatarInfo>();
			pClass.SetClientArms(avaInfo);
		}
	}
}
