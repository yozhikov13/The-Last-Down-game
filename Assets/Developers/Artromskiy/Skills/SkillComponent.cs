using Mirror;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillComponent : NetworkBehaviour
{
	[SyncVar]
	protected string skillName;

	[SerializeField]
    protected float delay;
    [SerializeField]
    protected float rollbackTime;
    [SerializeField]
    protected float useTime;
    [SerializeField]
    protected int maxCount;
    [SerializeField]
    protected int curCount;
    [SerializeField]
    private List<ParticleSystem> FX;

    private delegate void SyncEventDelegate();
    [SyncEvent]
    private event SyncEventDelegate EventSkill;

    public void Awake()
    {
        foreach (var item in FX)
        {
            EventSkill += item.Play;
        }
    }

    [ClientCallback]
    public abstract void Skill();

    protected void InvokeFX()
    {
        EventSkill();
    }

	public virtual void SetSkillName(string name)
	{

		skillName = name;

	}

	public virtual string GetSkillName()
	{

		return this.skillName;

	}
}