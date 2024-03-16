using DB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DBInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Amazon.UnityInitializer.AttachToGameObject(gameObject);
        Amazon.AWSConfigs.HttpClient = Amazon.AWSConfigs.HttpClientOption.UnityWebRequest;
        DBAccessor.Initialize();
    }
}
