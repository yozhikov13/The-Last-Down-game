using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MatchMaking;

public class Respawner : NetworkBehaviour
{
    private readonly List<GameObject> players = new List<GameObject>();
    public void Respawn(Player pc)
    {
        var l = NetworkManager.startPositions;
        pc.transform.position = l[Random.Range(0, l.Count)].position;
    }

    public void AddPlayer(Player pc)
    {
        pc.OnDead += Respawn;
        players.Add(pc.gameObject);
    }
}
