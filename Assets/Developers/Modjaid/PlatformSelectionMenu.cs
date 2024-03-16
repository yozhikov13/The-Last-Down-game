using GameData;
using Mirror;
using Mirror.Websocket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
public class PlatformSelectionMenu : MonoBehaviour
{
    public List<GameObject> UIforms;
    public GameObject managers;
    private NetworkManager networkManager;

    void Start()
    {
    #if UNITY_ANDROID || UNITY_IPHONE
        turnBackButton(false);
        switchUI("Client");
    #endif
    #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        turnBackButton(false);
        switchUI("Server");
    #endif
    #if UNITY_EDITOR
        turnBackButton(true);
        switchUI("Editor");
    #endif
        //networkManager = managers.transform.GetChild(1).GetComponent<NetworkManager>();
        //DontDestroyOnLoad(managers);
    }


    private void turnBackButton(bool isActive)
    {
        Transform button = null;
        foreach (GameObject item in UIforms)
        {
            button = item.transform.Find("Back_Button");
            if (button != null)
            {
                item.gameObject.SetActive(isActive);
                button.gameObject.SetActive(isActive);
                button = null;
            }
        }
    }

    private void switchUI(string gameObjectName)
    {
        GameObject form = UIforms.Find(x => x.name.Equals(gameObjectName));
        foreach (GameObject item in UIforms)
        {
            if (form != item)
            {
                item.SetActive(false);
            }
            else
            {
                item.SetActive(true);
            }
        }
    }

    public void clickToServerUI()
    {
        switchUI("Server");
    }
    public void clickToClientUI()
    {
        switchUI("Client");
    }
    public void clickToTestUI()
    {
        switchUI("Test");
    }
    public void clickToBack()
    {
        switchUI("Editor");
    }
    public void clickToStartServer()
    {
        SceneManager.LoadScene("SampleScene");
     //   networkManager.StartServer();

    }
    public void clickToStartClient()
    {
        SceneManager.LoadScene("SampleScene");
      //  networkManager.StartClient();
    }
    public void clickToStartHost()
    {
        SceneManager.LoadScene("SampleScene");
      //  networkManager.StartHost();
    }



}
