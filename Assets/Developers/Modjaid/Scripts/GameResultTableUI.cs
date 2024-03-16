using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using static Mirror.SyncList<PlayerInfo>;



/*
 *  Для удобного внедрения префабной таблицы в разные менюшки
 *  
 */
public class GameResultTableUI : MonoBehaviour
{
    public GameObject contentObject;
    public GameObject playerResumeRowPrefab;
    public MatchManager matchManager; // Передать сюда ссылку на объект извне

    private Image avatarImage;
    private TextMeshProUGUI nickText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI killsText;
    private TextMeshProUGUI helpsText;
    private TextMeshProUGUI deathsText;
    private TextMeshProUGUI pingText;

    private List<PlayerInfo> tableData;


    public void Start()
    {
        initPrefabComponents();
    }

    public void OnEnable()
    {
        tableData = new List<PlayerInfo>();
        foreach (PlayerInfo item in matchManager.PlayersStats)
        {
            tableData.Add(item);
        }

        matchManager.PlayersStats.Callback += eventMatch_PlayersStats; // подписываемся на события

        showTable(); //В дальнейшем можно заменить на перегруженный с анимацией showTable(animationQueue)
    }

    public void OnDisable()
    {
        matchManager.PlayersStats.Callback -= eventMatch_PlayersStats; // отписываемся от событий
        hideTable(); 
    }

    private void eventMatch_PlayersStats(Operation op, int itemIndex, PlayerInfo oldItem, PlayerInfo newItem)
    {
        tableData[itemIndex] = newItem;
        clearPrefabsOnTable();
        setPlayersOnTable(tableData);
    }

    public List<PlayerInfo> TEST_initRandomPlayers(int countProfiles)
    {
        string[] nickNames = { "Modjaid", "Artromskiy", "Nagibator", "Tokugawa", "SosokVorobushka" };
        List<PlayerInfo> resultProfiles = new List<PlayerInfo>();
        for(int i = 0; i < countProfiles; i++)
        {
            PlayerInfo item = new PlayerInfo(nickNames[Random.Range(0, 4)]);
            item.Score = Random.Range(0, 99);
            item.Kills = Random.Range(0, 99);
            item.Helps = Random.Range(0, 99);
            item.Deaths = Random.Range(0, 99);
            resultProfiles.Add(item);
        }
        return resultProfiles;
    }

    public void setPlayersOnTable(List<PlayerInfo> profiles)
    {
        foreach (PlayerInfo item in profiles)
        {
            // avatarImage = //
            nickText.text = item.Name.ToString();
            scoreText.text = item.Score.ToString();
            killsText.text = item.Kills.ToString();
            helpsText.text = item.Helps.ToString();
            deathsText.text = item.Deaths.ToString();
            pingText.text = "??"; // PlayerInfo не имеет такой переменной
            GameObject newRow = Instantiate(playerResumeRowPrefab, contentObject.transform);
        }
    }

    public void initPrefabComponents()
    {
      avatarImage  = playerResumeRowPrefab.transform.GetChild(0).GetComponent<Image>();
      nickText     = playerResumeRowPrefab.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
      scoreText    = playerResumeRowPrefab.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
      killsText    = playerResumeRowPrefab.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
      helpsText    = playerResumeRowPrefab.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
      deathsText   = playerResumeRowPrefab.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
      pingText     = playerResumeRowPrefab.transform.GetChild(6).GetComponent<TextMeshProUGUI>();
      
    }

    public void clearPrefabsOnTable()
    {
        foreach(Transform child in contentObject.transform)
        {
            Destroy(child.gameObject);
        }
    }


    public void hideTable()
    {
        Color color = this.gameObject.GetComponent<Image>().color;
        color.a = 0f;
        this.gameObject.GetComponent<Image>().color = color;

        foreach (Transform child in contentObject.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    //Перегрузка включает данный префаб с анимацией!
    public void showTable(IEnumerator animationQueue)
    {
        StartCoroutine(animationQueue);
    }

    //Перегрузка включает данный префаб без анимации
    public void showTable()
    {
        Color color = this.gameObject.GetComponent<Image>().color;
        color.a = 1f;
        this.gameObject.GetComponent<Image>().color = color;
        foreach (Transform child in contentObject.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    
    public IEnumerator animationQueue_1()
    {
        this.gameObject.GetComponent<Animation>().Play("animTable_00");
        yield return new WaitForSeconds(0.3f);

        foreach (Transform child in contentObject.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<Animation>().Play("animRow_00");
            yield return new WaitForSeconds(0.05f);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(GameResultTableUI))]
public class GameResultEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
         DrawDefaultInspector();

        GameResultTableUI table = (GameResultTableUI)target;
        EditorGUILayout.BeginVertical("Box");
        if (GUILayout.Button("add test random prefab PlayerInfo",GUILayout.Height(30)))
        {
            table.initPrefabComponents();
            List<PlayerInfo> profiles = table.TEST_initRandomPlayers(1);
            table.setPlayersOnTable(profiles);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.HelpBox("only by runtime",MessageType.Info);
        if (GUILayout.Button("Clear Table"))
        {
            table.initPrefabComponents();
            table.clearPrefabsOnTable();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hide Table"))
        {
            table.hideTable();
        }
        if (GUILayout.Button("Show Table (Animation)"))
        {
            table.showTable(table.animationQueue_1());
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
#endif