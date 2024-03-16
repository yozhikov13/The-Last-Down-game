using System.Collections;
using UnityEngine;
using MatchMaking;

public class CameraController : MonoBehaviour
{
    public Transform fps;
    public Transform tps;
    public float speedChange;
    private IEnumerator routine;
    public Player player;

    [ContextMenu("Change")]
    public void ChangeVision(bool isTps)
    {
        if(isTps)
        {
            transform.SetParent(tps);
        }
        else
        {
            transform.SetParent(fps);
        }
        if (routine != null)
            StopCoroutine(routine);
        routine = MoveTo();
        StartCoroutine(routine);
    }

    private IEnumerator MoveTo()
    {
        while(Vector3.Distance(transform.position, transform.parent.position) > 0.1)
        {
            transform.position = Vector3.Lerp(transform.position, transform.parent.position, speedChange * Time.deltaTime);
            yield return null;
        }
    }

    public void Update()
    {
      if(player.isTps)
      {
      if(Physics.Raycast(player.weaponSet.shootPoint.position, player.weaponSet.shootPoint.forward, out RaycastHit hit, 500))
      {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(hit.point - transform.position, player.weaponSet.shootPoint.up), Time.deltaTime * 2f);
      }
      else
      {
        var p = player.weaponSet.shootPoint.position + player.weaponSet.shootPoint.forward * 500;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(p - transform.position, player.weaponSet.shootPoint.up), Time.deltaTime * 2f);
      }
      }
      else
      {
        transform.rotation = player.weaponSet.shootPoint.rotation;
      }
    }
}
