using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Utils
{

    public static Dictionary<string, StageManager> stages = new Dictionary<string, StageManager>();

    public static void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public static void SetStage(StageManager sm)
    {
        string stageName = GetStageName(sm.transform);
        stages.Add(stageName, sm);
        Debug.Log($"{stageName}加入了stages");
    }

    public static StageManager GetStage(Transform t)
    {
        return stages[GetStageName(t)];
    }
    public static string GetStageName(Transform t)
    {
        string name = t.name;
        while (!name.StartsWith("Stage"))
        {
            t = t.parent;
            name = t.name;
        }
        return name;
    }

    public static bool EndWithTag(Collider collider,string tag)
    {
        return collider.tag.EndsWith(tag);
    }

}
