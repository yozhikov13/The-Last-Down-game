using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public PlayerClass Player
    {
        get
        {
            if (Player == null)
                Player = GetComponent<PlayerClass>();
            return Player;
        }
        private set
        {
            Player = value;
        }
    }

    public List<CharacterBuff> buffs;

    private void Start()
    {
        buffs = new List<CharacterBuff>();
    }

    public void AddBuff(CharacterBuff buff)
    {
        buffs.Add(buff);
    }
    public void AddBuff(ConditionBuff buff)
    {
        buffs.Add(buff);
        buff.OnStart();
    }

    public void RemoveBuff(CharacterBuff buff)
    {
        if(buffs.Remove(buff))
        {
            RetrieveBuff(buff);
        }
    }
    public void AppendBuff(CharacterBuff buff)
    {
        Player.runSpeed += buff.speed;
        Player.walkSpeed += buff.speed;
        Player.Armor += buff.armor;
        Player.Noiseness += buff.noiseness;
    }

    public void RetrieveBuff(CharacterBuff buff)
    {
        Player.runSpeed -= buff.speed;
        Player.walkSpeed -= buff.speed;
        Player.Armor -= buff.armor;
        Player.Noiseness -= buff.noiseness;
    }
}
