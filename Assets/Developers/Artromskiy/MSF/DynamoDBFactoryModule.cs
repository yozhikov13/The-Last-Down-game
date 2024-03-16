using Barebones.MasterServer;
using Barebones.MasterServer.Examples.BasicAuthorization;
using System;
using DB;

public class DynamoDBFactoryModule : DatabaseFactoryModule
{
    public override void Initialize(IServer server)
    {
#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
        try
        {
            Msf.Server.DbAccessors.SetAccessor<IAccountsDatabaseAccessor>(new AccountsDynamoDBAccessor());
            Amazon.UnityInitializer.AttachToGameObject(gameObject);
            Amazon.AWSConfigs.HttpClient = Amazon.AWSConfigs.HttpClientOption.UnityWebRequest;
            DBAccessor.Initialize(); // Загружает все виды орудий в словарь
        }
        catch (Exception e)
        {
            logger.Error("Failed to setup DynamoDB");
            logger.Error(e);
        }
#endif
    }
}
