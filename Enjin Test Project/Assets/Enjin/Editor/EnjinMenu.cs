using UnityEngine;
using UnityEditor;

public class EnjinMenu : MonoBehaviour
{
    [MenuItem("Window/Enjin/MainNet Cloud")]
    private static void OpenMainNet()
    {
        Application.OpenURL("https://cloud.enjin.io");
    }
    
    [MenuItem("Window/Enjin/Kovan Cloud")]
    private static void OpenKovan()
    {
        Application.OpenURL("https://kovan.cloud.enjin.io");
    }
}