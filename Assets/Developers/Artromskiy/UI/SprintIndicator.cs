using UnityEngine;

public class SprintIndicator : MonoBehaviour
{
    [SerializeField]
    private InputManager im;
    [SerializeField]
    private GameObject gm;

    private void Update()
    {
        ChangeSprint();
    }

    private void ChangeSprint()
    {
        if(im.pClass.pMove.isMoving && im.pClass.pMove.isSprinting)
            gm.SetActive(true);
        else
            gm.SetActive(false);
    }
}
