using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanTool : MonoBehaviour
{
    [ContextMenu("CleanAll")]
    public void CleanAll()
    {
	Resources.UnloadUnusedAssets();
    }
}
