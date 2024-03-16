using Mirror;
using System.Collections;
using UnityEngine;

public class Anarchist : SkillComponent
{
    [SyncVar]
    private bool canCast = true;
    [SerializeField]
    private HealthSystem barricade;

    [ServerCallback]
    private void OnEnable()
    {
        canCast = true;
		this.SetSkillName("Anarchist");
    }

    [ContextMenu("Skill")]
    public override void Skill()
    {
        CmdSkill();
    }

    private void CmdSkill()
    {
        if (curCount > 0 && canCast)
        {
            StartCoroutine(Action());
            StartCoroutine(RollBack());
        }
    }

    IEnumerator Action()
    {
        var movement = GetComponent<ExpPlayerMovement>();
        if (movement == null)
            yield break;
        canCast = false;
        movement.canMove = false;
        yield return new WaitForSeconds(delay);
        movement.canMove = true;
        canCast = true;
        SpawnBarricade();
        InvokeFX();
    }
    private IEnumerator RollBack()
    {
        yield return new WaitForSeconds(rollbackTime);
        curCount = Mathf.Min(curCount + 1, maxCount);
    }

    [ServerCallback]
    private void SpawnBarricade()
    {
        var b = Instantiate(barricade);
        b.transform.position = transform.position + transform.forward;
        if (Physics.Raycast(b.transform.position, -b.transform.up, out RaycastHit hit, 30))
            b.transform.position = hit.point;
        NetworkServer.Spawn(b.gameObject);
    }
}
