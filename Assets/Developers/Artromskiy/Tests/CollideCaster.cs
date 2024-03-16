using Rewired;
using System.Collections.Generic;
using UnityEngine;

public class CollideCaster : MonoBehaviour
{
    public int index;
    public WithColliders controller;
    public bool obstacle
    {
        get
        {
            return triggeredObstacles.Count > 0;
        }
    }
    public bool enemy
    {
        get
        {
            return triggeredEnemies.Count > 0;
        }
    }
    public List<GameObject> triggeredEnemies;
    public List<GameObject> triggeredObstacles;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<WithColliders>();
        triggeredEnemies = new List<GameObject>();
        triggeredObstacles = new List<GameObject>();
    }

    public void OnTriggerEnter(Collider hit)
    {
        //Debug.Log(hit.collider.gameObject.name);
        if (hit.gameObject.CompareTag("Character"))
        {
            triggeredEnemies.Add(hit.gameObject);
            controller.enemy[index] = triggeredObstacles.Count > 0;
        }
        else
        {
            triggeredObstacles.Add(hit.gameObject);
            controller.obstacle[index] = triggeredObstacles.Count > 0;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //Debug.Log(hit.collider.gameObject.name);
        if (other.gameObject.CompareTag("Character"))
        {
            triggeredEnemies.Remove(other.gameObject);
            controller.enemy[index] = triggeredObstacles.Count > 0;
        }
        else
        {
            triggeredObstacles.Remove(other.gameObject);
            controller.obstacle[index] = triggeredObstacles.Count > 0;
        }
    }
}
