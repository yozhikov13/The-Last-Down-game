using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileInfo : Monosingleton<ProfileInfo>
{

	public string PlayerNickname;
	public int PlayerLevel;
	public string Player3DAvatar;
	public string Player2DAvatar;
	public string ActiveCharacter;
	public int PlayerMoney;
	public int PlayerPremiumMoney;
	public int PlayerXP;
	public string PlayerCurrentRegion;
	public bool PrimarySlotAvailable = true;
	public bool SecondarySlotAvailable;
	public bool TertiarySlotAvailable;
	public bool EquipmentSlotAvailable;

}
