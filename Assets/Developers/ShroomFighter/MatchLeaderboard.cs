using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Chat;
public class MatchLeaderboard :  NetworkBehaviour
{
    List< PlayerInfo> PlayerInfos;
    //PlayerInfo First,Second ,Third;
   
    
    public Transform FirstPlace, SecondPlace, ThirdPlace;
    

    GameObject FirstModel, SecondModel, ThirdModel;
    

    public override void OnStartServer()
    {
        //First.Score =Second.Score=Third.Score= 0;

        //PlayerInfos.AddRange(FindObjectOfType<MatchManager>().PlayersStats);
        //PlayerComponents.AddRange( FindObjectsOfType<NetworkIdentity>());
        StartCoroutine("CountDown");
       
    }
    [Server]
    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(600f);
        // Добваить событие остановки матча , отключение всех игроков и появление интервейса
        UnactivatePLayers();
        

        Comparer c = new Comparer();
        PlayerInfos.Sort(c);// получение отсортированного списка

        GameObject FirstPlayer, SecondPlayer, ThirdPlayer;

        FirstPlayer = ServerLog.playersData.Find(x=>x.GetComponentInChildren<NetworkIdentity>().netId.ToString("G")== PlayerInfos[0].Name);
        SecondPlayer = ServerLog.playersData.Find(x => x.GetComponentInChildren<NetworkIdentity>().netId.ToString("G") == PlayerInfos[1].Name);
        ThirdPlayer = ServerLog.playersData.Find(x => x.GetComponentInChildren<NetworkIdentity>().netId.ToString("G") == PlayerInfos[3].Name);
        

        FirstModel = SecondModel = ThirdModel = new GameObject();

        CopyComponents(FirstModel, FirstPlayer);
        CopyComponents(SecondModel, SecondPlayer);
        CopyComponents(ThirdModel, ThirdPlayer);

        FirstModel = Instantiate(FirstModel, FirstPlace);
        SecondModel = Instantiate(SecondModel, SecondPlace);
        ThirdModel = Instantiate(ThirdModel, ThirdPlace);

    }
    [Server]
    void CopyComponents(GameObject To,GameObject From)
    {
        
        CopyComponent(From.GetComponentInChildren<Animator>(), To);//копирование значений аниматора
        CopyComponent(From.GetComponent<PlayerWeapon>(), To);
    }

    [Server]
    void UnactivatePLayers()
    {
        foreach(var i in ServerLog.playersData)
        {
            if(i.transform.root.GetComponent<CharacterHitBox>()!=null)
            {
                i.transform.root.gameObject.SetActive(false);
                TargetUnactivate(i?.GetComponentInChildren<NetworkIdentity>().connectionToClient, i.transform.root.gameObject);
                

            }
        }
    }

    [TargetRpc]
    public void TargetUnactivate(NetworkConnection target,GameObject obj)
    {
        obj.SetActive(false);
    }


    [Server]
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        if (!dst) dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

    class Comparer: IComparer<PlayerInfo>
    {
         public int Compare(PlayerInfo A, PlayerInfo B)
        {
            if (A.Score > B.Score)
            {
                return 1;
            }
            else if (A.Score < B.Score)
            {
                return -1;
            }
            else
                return 0;
        }
    }
}
