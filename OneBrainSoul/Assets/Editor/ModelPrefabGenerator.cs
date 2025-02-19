using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(ModelEditor))]
public class ModelPrefabGenerator : UnityEditor.Editor
{
    string inputPath = "Generate";
    string outputPath = "GeneratedModels";

    private List<GameObject> GetAllModels(string dir)
    {
        string[] foldersToSearch = { "Assets/Models/"+inputPath+"/"+dir };
        List<GameObject> allPrefabs = GetAssets<GameObject>(foldersToSearch, "");
        return allPrefabs;
    }

    public static List<T> GetAssets<T>(string[] foldersToSearch, string filter) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets(filter, foldersToSearch);
        var assets = new List<T>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            assets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
        }
        return assets;
    }

    public void CreatePrefabs()
    {
        List<string> dirs = Directory.GetDirectories("Assets/Models/" + inputPath).ToList();
        for (int i = 0; i < dirs.Count; i++)
        {
            dirs[i] = dirs[i].Substring(("Assets/Models/" + inputPath).Length + 1);
        }
        dirs.Add("");

        foreach (string dir in dirs)
        {
            var models = GetAllModels(dir);

            for (int i = 0; i < models.Count; i++)
            {
                GameObject prefab = new GameObject();
                Mesh modelMesh = models[i].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;

                var prefabMesh = prefab.AddComponent<MeshFilter>();
                var prefabRenderer = prefab.AddComponent<MeshRenderer>();
                prefabRenderer.material = new Material(Shader.Find("Autodesk Interactive"));
                prefabMesh.sharedMesh = modelMesh;
                prefab.name = models[i].name;

                SavePrefab(prefab, dir);
                DestroyImmediate(prefab);
            }
        }
    }
    
    public void SavePrefab(GameObject prefab, string dir)
    {
        Directory.CreateDirectory("Assets/Prefabs/" + outputPath + "/" + dir);
        string localPath = "Assets/Prefabs/" + outputPath + "/" + dir + "/" + prefab.name + ".prefab";

        var a = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
        if (a !=null)
        {
            a.GetComponent<MeshFilter>().sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            return;
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, localPath, InteractionMode.UserAction);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
        {
            CreatePrefabs();
        }

        inputPath = GUILayout.TextField(inputPath, 25);
        outputPath = GUILayout.TextField(outputPath, 25);
    }
}
