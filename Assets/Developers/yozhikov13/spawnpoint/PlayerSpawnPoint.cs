using UnityEngine;
using System.Collections.Generic;
using Mirror;

[RequireComponent(typeof(SphereCollider))]
[DisallowMultipleComponent]
[AddComponentMenu("Network/PlayerSpawnPoint")]
public class PlayerSpawnPoint : MonoBehaviour
{
    public float radius;
    private readonly List<PlayerClass> triggered = new List<PlayerClass>();
    private void Awake() => NetworkManager.RegisterStartPosition(transform);
    private void OnDestroy() => NetworkManager.UnRegisterStartPosition(transform);


    /// <summary>
	/// Определяем радиус вокруг точки спавна, чтобы нельзя было воспользоваться точкой, 
    /// если в пределах радиуса стоит противник
	/// </summary>
    private void Start()
    {
        GetComponent<SphereCollider>().radius = radius;
    }

    /// <summary>
	/// Если в пределах радиуса стоит противик, убираем возможность возрождения на точке
	/// </summary>
	/// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        var v = other.gameObject.GetComponent<PlayerClass>();
        if (v)
        {
            triggered.Add(v);
            if (triggered.Count > 0)
                NetworkManager.UnRegisterStartPosition(transform);
        }
    }

    /// <summary>
	/// Если противников нет в радиусе, добавляем возможность возрождения на точке
	/// </summary>
	/// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        var v = other.gameObject.GetComponent<PlayerClass>();
        if (v)
        {
            triggered.Remove(v);
            if (triggered.Count == 0)
                NetworkManager.RegisterStartPosition(transform);
        }
    }
}

