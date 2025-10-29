using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using System.IO;
public static class LevelDesignerTool
{
    static AI_Client client;
    static Dictionary<string, string> prefabDictionary = new Dictionary<string, string>();
    private static void SetupAIClient()
    {
        if (EditorPrefs.HasKey("AI_API_KEY") == false)
        {
            Debug.LogWarning("AI API Key not found in EditorPrefs under 'AI_API_KEY'. Please set it before using the LevelDesignerTool.");
            return;
        }
        string api = EditorPrefs.GetString("AI_API_KEY", "");
        client = new AI_Client(api);
    }
    public static void SpawnPrefabAtPosition(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (!prefabDictionary.ContainsKey(prefabName))
        {
            Debug.LogError($"[WorldDesignGenerationTool] Prefab '{prefabName}' not found in dictionary.");
            return;
        }

        string prefabPath = prefabDictionary[prefabName];
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[WorldDesignGenerationTool] Could not load prefab at path: {prefabPath}");
            return;
        }

        EditorApplication.delayCall += () =>
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError("[WorldDesignGenerationTool] Failed to instantiate prefab.");
                return;
            }

            instance.transform.position = position;
            instance.transform.rotation = rotation;

            // Register Undo for editor safety
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Prefab");
        };
    }
    private static string GetAllPrefabsAsString()
    {
        List<string> prefabDetails = new List<string>();

        foreach (var kvp in prefabDictionary)
        {
            string prefabName = kvp.Key;
            string prefabPath = kvp.Value;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                prefabDetails.Add($"{prefabName}: [Could not load prefab]");
                continue;
            }

            // Calculate prefab size (renderer bounds)
            Bounds totalBounds = new Bounds(prefab.transform.position, Vector3.zero);
            bool hasRenderer = false;

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (!hasRenderer)
                {
                    totalBounds = r.bounds;
                    hasRenderer = true;
                }
                else
                {
                    totalBounds.Encapsulate(r.bounds);
                }
            }

            Vector3 size = hasRenderer ? totalBounds.size : Vector3.zero;
            prefabDetails.Add($"{prefabName} ({size.x:F2}, {size.y:F2}, {size.z:F2})");
        }

        return string.Join(", ", prefabDetails);
    }
    public static void LoadPrefabDictionary(string assetPath)
    {
        prefabDictionary.Clear();
        string[] paths = Directory.GetFiles(Path.Combine(Application.dataPath, assetPath), "*.prefab", SearchOption.AllDirectories);

        foreach (var path in paths)
        {
            string prefabName = Path.GetFileNameWithoutExtension(path);
            prefabDictionary[prefabName] = "Assets" + path.Replace(Application.dataPath, "").Replace("\\", "/");
        }
        Debug.Log($"📂 Loaded {paths.Length} prefabs into the dictionary from '{assetPath}'.");
    }
    public static async Task GenerateLevelLayout(string designPrompt)
    {
        SetupAIClient();
        if (client == null) return;
        Debug.Log("Generating level layout with prompt: " + designPrompt);
        string assetContent = GetAllPrefabsAsString();
        List<ToolListToSend> tools = new List<ToolListToSend>()
        {
            new ToolListToSend
            {
                toolName = "SpawnPrefabAtPosition",
                description = "Spawns a prefab at a specified position and rotation.",
                parameters = new List<string> { "prefabName: string", "position: Vector3", "rotation: Quaternion" },
                returnType = "GameObject"
            },
            new ToolListToSend
            {
                toolName = "CreateCuboid",
                description = "Creates a cuboid primitive in the scene.",
                parameters = new List<string> { "name: string", "position: Vector3", "scale: Vector3" },
                returnType = "void"
            },
            new ToolListToSend
            {
                toolName = "CreateSphere",
                description = "Creates a sphere primitive in the scene.",
                parameters = new List<string> { "name: string", "position: Vector3", "radius: float" },
                returnType = "void"
            },
            new ToolListToSend
            {
                toolName = "CreateCapsule",
                description = "Creates a capsule primitive in the scene.",
                parameters = new List<string> { "name: string", "position: Vector3", "height: float", "radius: float" },
                returnType = "void"
            },
            new ToolListToSend
            {
                toolName = "CreatePlane",
                description = "Creates a plane primitive in the scene.",
                parameters = new List<string> { "name: string", "position: Vector3", "size: Vector2" },
                returnType = "void"
            },
            new ToolListToSend
            {
                toolName = "CreateCylinder",
                description = "Creates a cylinder primitive in the scene.",
                parameters = new List<string> { "name: string", "position: Vector3", "height: float", "radius: float" },
                returnType = "void"
            }
        };
        string prompt = $"Generate a level layout based on the following design prompt:\n{designPrompt}\n" +
                        "You have access to the following assets:\n" +
                        $"{assetContent}\n";
        string responseFormat = @"{
            ""prompt"": ""Your Answer Here"",
            ""actions"": [{{ ""tool"": ""<toolName>"", ""parameters"": {{ ... }} }}]
        }";

        string response = await client.SendRequestAsync(prompt, responseFormat, tools);
        AI_Client.ProcessResponse(response);

        // Debug.Log("Level Layout Response: " + response);
    }
}










