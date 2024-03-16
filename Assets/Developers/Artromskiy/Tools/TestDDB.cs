using System.Collections.Generic;
using UnityEngine;
using DB;
using GameData;
using Barebones.MasterServer;
using Newtonsoft.Json;
using System;

public class TestDDB : MonoBehaviour
{
    private void Awake()
    {
        Amazon.UnityInitializer.AttachToGameObject(gameObject);
        Amazon.AWSConfigs.HttpClient = Amazon.AWSConfigs.HttpClientOption.UnityWebRequest;
        DBAccessor.Initialize(); // Загружает все виды орудий в словарь
    }


    [ContextMenu("First Method")]
    public void Method1()
    {
        CreateTestProfile();
    }

    [ContextMenu("Second Method")]
    public void Method2()
    {

    }

    public void CreateTestAccount()
    {
        AccountsDynamoDBAccessor aDBA = new AccountsDynamoDBAccessor();
        var AccInfo = aDBA.CreateAccountInstance();
        AccInfo.Username = "Keshevich";
        AccInfo.Email = "keshkesh@gmail.com";
        AccInfo.Token = "kshkshk5hk54";
        aDBA.InsertNewAccountAsync(AccInfo, Debug.Log);
    }

    public void CreateTestProfile()
    {
        ProfilesDynamoDBAccessor pDBA = new ProfilesDynamoDBAccessor();
        var PrfInfo = new ObservableServerProfile("KeshKesh");
        SuccessCallback info = ShowInfo;
        pDBA.UpdateProfileAsync(PrfInfo, info);
    }

    public void ShowInfo(bool x, string y)
    {
        if (x)
            return;
        Debug.Log(y);
    }
}
