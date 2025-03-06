using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModelEditor))]
public class ModelPrefabGenerator : UnityEditor.Editor
{
    string modelInputPath = "Generate";
    string modelOutputPath = "GeneratedModels";
    string textureInputPath = "Generate";
    string textureOutputPath = "GeneratedMaterials";

    private List<GameObject> GetAllModels(string dir)
    {
        string[] foldersToSearch = { "Assets/Models/"+modelInputPath+"/"+dir };
        List<GameObject> allPrefabs = GetAssets<GameObject>(foldersToSearch, "");
        return allPrefabs;
    }
    private List<Texture> GetAllTextures(string dir)
    {
        Debug.Log("Assets/Textures/" + textureInputPath + "/" + dir);
        string[] foldersToSearch = { "Assets/Textures/"+textureInputPath+"/"+dir };
        List<Texture> allTextures = GetAssets<Texture>(foldersToSearch, "t:Texture");
        return allTextures;
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

    public void CreateModelPrefabs()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Autodesk Interactive/AutodeskInteractive");
        List<string> dirs = Directory.GetDirectories("Assets/Models/" + modelInputPath).ToList();
        for (int i = 0; i < dirs.Count; i++)
        {
            dirs[i] = dirs[i].Substring(("Assets/Models/" + modelInputPath).Length + 1);
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
                prefabRenderer.material = new Material(shader);
                prefabMesh.sharedMesh = modelMesh;
                prefab.name = models[i].name;

                SavePrefab(prefab, dir);
                DestroyImmediate(prefab);
            }
        }
    }

    public void CreateMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Autodesk Interactive/AutodeskInteractive");
        List<string> dirs = Directory.GetDirectories("Assets/Textures/" + modelInputPath).ToList();
        List<List<string>> dirs2 = new List<List<string>>();
        for (int i = 0; i < dirs.Count; i++)
        {
            dirs2.Add((Directory.GetDirectories(dirs[i]).ToList()));
        }
        for (int i = 0; i < dirs2.Count; i++)
        {
            for (int j = 0; j < dirs2[i].Count; j++)
            {
                dirs2[i][j] = dirs2[i][j].Substring(("Assets/Textures/" + textureInputPath).Length + 1);
            }
        }

        for (int d = 0; d < dirs2.Count; d++) {
            foreach (string dir in dirs2[d])
            {
                var textures = GetAllTextures(dir);
                Debug.Log(dir);

                if (textures == null || textures.Count == 0) continue;

                Material mat = new Material(shader);
                mat.name = textures[0].name;

                for (int i = 0; i < textures.Count; i++)
                {
                    string property = "";
                    string propertyToEnable = "";
                    if (textures[i].name.Contains("BaseColor"))
                    {
                        property = "_MainTex";
                        propertyToEnable = "_UseColorMap";
                    }
                    else if (textures[i].name.Contains("Normal"))
                    {
                        property = "_BumpMap";
                        propertyToEnable = "_UseNormalMap";
                    }
                    else if (textures[i].name.Contains("Metallic"))
                    {
                        property = "_MetallicGlossMap";
                        propertyToEnable = "_UseMetallicMap";
                    }
                    else if (textures[i].name.Contains("Roughness"))
                    {
                        property = "_SpecGlossMap";
                        propertyToEnable = "_UseRoughnessMap";
                    }
                    else
                    {
                        continue;
                    }

                    mat.SetTexture(property, textures[i]);
                    mat.SetInt(propertyToEnable, 1);

                }

                SaveMaterial(mat, dir);
            }

        }
        AssetDatabase.SaveAssets();
    }
    
    public void SavePrefab(GameObject prefab, string dir)
    {
        Directory.CreateDirectory("Assets/Prefabs/" + modelOutputPath + "/" + dir);
        string localPath = "Assets/Prefabs/" + modelOutputPath + "/" + dir + "/" + prefab.name + ".prefab";

        var a = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
        if (a !=null)
        {
            a.GetComponent<MeshFilter>().sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            return;
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, localPath, InteractionMode.UserAction);
    }

    public void SaveMaterial(Material mat, string dir)
    {
        Directory.CreateDirectory("Assets/Materials/" + textureOutputPath + "/" + dir);
        string localPath = "Assets/Materials/" + textureOutputPath + "/" + dir + "/" + mat.name + ".mat";

        var a = AssetDatabase.LoadAssetAtPath<Material>(localPath);
        if (a != null)
        {
            a.SetTexture("_MainTex", mat.GetTexture("_MainTex"));
            a.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
            a.SetTexture("_MetallicGlossMap", mat.GetTexture("_MetallicGlossMap"));
            a.SetTexture("_SpecGlossMap", mat.GetTexture("_SpecGlossMap"));
            return;
        }

        AssetDatabase.CreateAsset(mat, localPath);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Models"))
        {
            CreateModelPrefabs();
        }
        if (GUILayout.Button("Generate Textures"))
        {
            CreateMaterials();
        }

        modelInputPath = GUILayout.TextField(modelInputPath, 25);
        modelOutputPath = GUILayout.TextField(modelOutputPath, 25);
        textureInputPath = GUILayout.TextField(textureInputPath, 25);
        textureOutputPath = GUILayout.TextField(textureOutputPath, 25);
    }
}
