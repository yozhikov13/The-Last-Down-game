using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ServerLog : MonoBehaviour
{
    public delegate void PlayersChangedDelegate(GameObject player);
    public event PlayersChangedDelegate OnPlayersAddingEvent;
    public event PlayersChangedDelegate OnPlayersRemoveEvent;


    public static ServerLog instance = null;

    public static readonly List<GameObject> playersData = new List<GameObject>();

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }

        // Объект не уничтожался при переходе на другую сцену игры
        DontDestroyOnLoad(gameObject);

        InitializeManager();
    }

    private void InitializeManager()
    {
        
    }

    public void Add(GameObject player)
    {
        playersData.Add(player);
        OnPlayersAddingEvent?.Invoke(player);
    }

    public void Remove(GameObject player)
    {
        playersData.Remove(player);
        OnPlayersRemoveEvent?.Invoke(player);
    }
    public void ClearServerLog()
    {
        playersData.Clear();
    }

}
