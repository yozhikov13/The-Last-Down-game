using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchMaking
{
public class PlayerConstructor : NetworkBehaviour
{
    public List<GameObject> parentedClientSideObjects;
    public List<GameObject> clientSideObjects;
    public GameObject UI;


    public CameraController camController;
    public Transform FPSCam;
    public Transform TPSCam;

    private void Start()
    {
        if (!hasAuthority)
            return;
        CreateParentedClientSideObjects();
        CreateUI();
        AddCamera();
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ClientCallback]
    private void CreateParentedClientSideObjects()
    {
        foreach (var item in parentedClientSideObjects)
        {
            Instantiate(item, gameObject.transform);
        }
    }

    [ClientCallback]
    private void CreateClientSideObjects()
    {
        foreach (var item in clientSideObjects)
        {
            Instantiate(item, gameObject.transform);
        }
    }

    [ClientCallback]
    private void CreateUI()
    {
        UI = Instantiate(UI);
        var p = gameObject.GetComponent<Player>();
        UI.GetComponentInChildren<InputManager>().player = p;
        p.OnDestroyed += () => {Destroy(UI);};
		    this.GetComponent<SkillIconSetter>().SetSkillIcon(UI.GetComponentInChildren<InputManager>().skillButton);

	   }

    [ClientCallback]
    private void AddCamera()
    {
        camController = Instantiate(camController, TPSCam);
        camController.fps = FPSCam;
        camController.tps = TPSCam;
        camController.player = gameObject.GetComponent<Player>();
        UI.GetComponentInChildren<InputManager>().cam = camController;
    }
}
}
