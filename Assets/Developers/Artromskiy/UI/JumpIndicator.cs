using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JumpIndicator : MonoBehaviour
{
    [SerializeField]
    private InputManager im;
    [SerializeField]
    private GameObject[] sprites;


    private void Update()
    {
        ChangeSprint();
    }

    private void ChangeSprint()
    {
        if (!im.pClass.pMove.isJumping && im.pClass.pMove.canJump)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].SetActive(false);
            }
        }
    }
}
