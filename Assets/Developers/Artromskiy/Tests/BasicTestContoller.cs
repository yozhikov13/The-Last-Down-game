using Mirror;
using System.Collections;
using UnityEngine;

public class BasicTestContoller : NetworkBehaviour
{
    CharacterController chController;
    Weapon weapon;
    public Vector2 speed;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        weapon = GetComponent<Weapon>();
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer)
            return;
        var camObj = new GameObject();
        camObj.AddComponent<Camera>();
        camObj.transform.SetParent(gameObject.transform);
        camObj.transform.position = gameObject.transform.position + Vector3.up;
        camObj.transform.position -= gameObject.transform.forward* 3;
        camObj.transform.LookAt(gameObject.transform.position);
    }

    private void Update()
    {
        if (isLocalPlayer && hasAuthority)
        {
            CmdMove(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
            simplMove(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
            if (Input.GetMouseButtonDown(0))
                CmdStartShoot();
            if (Input.GetMouseButtonUp(0))
                CmdEndShoot();
        }
    }

    [Command]
    public void CmdMove(Vector2 vec)
    {
        simplMove(vec);
    }

    public void simplMove(Vector2 vec)
    {
        if (chController == null)
            return;
        chController.transform.Rotate(Vector3.up, vec.x * Time.deltaTime * speed.x);
        chController.SimpleMove(transform.forward * vec.y * Time.deltaTime * speed.y);
    }

    [Command]
    public void CmdStartShoot()
    {
        if(weapon)
        {
            weapon.StartShooting();
        }
    }
    [Command]
    public void CmdEndShoot()
    {
        if(weapon)
        {
            weapon.EndShooting();
        }
    }

}
