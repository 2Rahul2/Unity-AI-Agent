using UnityEngine;
using UnityEditor;
public class AgentToolWindow : EditorWindow
{
    // ==== API Key ====
    private string apiKey = "";
    private bool hasKey = false;
    private bool isEditing = false;
    private const string PREF_KEY = "AI_API_KEY";

    // ==== Foldout Toggles ====
    private bool showTools = true;
    private bool showLevelDesignTool = true;
    private bool showAnimationTool = true;

    // ==== Level Designing Tool ====
    private string assetsDirectory = "";
    private string levelPrompt = "";

    // ==== Animation Tool ====
    private GameObject animationTarget;
    private string animationPrompt = "";
    private float animationDuration = 2f; // default duration


    // ==== AI Client Setup ====
    private AI_Client aiClient;

    [MenuItem("Tools/AI Agent/Setup & Tools")]
    public static void ShowWindow()
    {
        GetWindow<AgentToolWindow>("AI Agent");
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey(PREF_KEY))
        {
            apiKey = EditorPrefs.GetString(PREF_KEY);
            hasKey = !string.IsNullOrEmpty(apiKey);
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("🤖 AI Agent Configuration", EditorStyles.boldLabel);
        GUILayout.Space(5);

        DrawAPIKeySection();
        GUILayout.Space(10);
        DrawToolsSection();
    }

    // ========== API KEY SECTION ==========
    private void DrawAPIKeySection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔐 API Key", EditorStyles.boldLabel);
        GUILayout.Space(3);

        if (!hasKey || isEditing)
        {
            EditorGUILayout.LabelField("Enter your AI API key:", EditorStyles.wordWrappedLabel);
            apiKey = EditorGUILayout.TextField("API Key:", apiKey);

            GUILayout.Space(5);
            if (GUILayout.Button("Submit", GUILayout.Height(25)))
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    EditorPrefs.SetString(PREF_KEY, apiKey);
                    hasKey = true;
                    isEditing = false;
                    Debug.Log("✅ API Key saved successfully!");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "API Key cannot be empty!", "OK");
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("Agent is ready ✅", EditorStyles.helpBox);
            GUILayout.Space(3);
            if (GUILayout.Button("Edit Key", GUILayout.Height(25)))
            {
                isEditing = true;
            }

        }

        GUILayout.Space(5);
        if (hasKey && GUILayout.Button("Clear Saved Key"))
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            apiKey = "";
            hasKey = false;
            isEditing = false;
            Debug.Log("🗑️ API Key cleared.");
        }

        EditorGUILayout.EndVertical();
    }

    // ========== TOOLS SECTION ==========
    private void DrawToolsSection()
    {
        EditorGUILayout.BeginVertical("box");
        showTools = EditorGUILayout.Foldout(showTools, "🧰 Tools", true, EditorStyles.foldoutHeader);

        if (showTools)
        {
            GUILayout.Space(5);
            DrawLevelDesignTool();
            GUILayout.Space(10);
            DrawAnimationTool();
        }

        EditorGUILayout.EndVertical();
    }

    // ========== LEVEL DESIGN TOOL ==========
    private void DrawLevelDesignTool()
    {
        EditorGUILayout.BeginVertical("helpBox");
        showLevelDesignTool = EditorGUILayout.Foldout(showLevelDesignTool, "🎨 Level Designing", true);

        if (showLevelDesignTool)
        {
            // Asset directory field
            EditorGUILayout.BeginHorizontal();
            assetsDirectory = EditorGUILayout.TextField("Assets Directory:", assetsDirectory);
            if (GUILayout.Button("Select", GUILayout.Width(70)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Asset Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    // Normalize path separators
                    selected = selected.Replace("\\", "/");
                    LevelDesignerTool.LoadPrefabDictionary(selected);

                    // Convert to relative path (Assets/...)
                    // if (selected.StartsWith(Application.dataPath))
                    // {
                    //     selected = "Assets" + selected.Substring(Application.dataPath.Length);
                    // }

                    assetsDirectory = selected;
                    Debug.Log($"📂 Selected folder: {assetsDirectory}");

                }
            }
            EditorGUILayout.EndHorizontal();

            // Prompt
            levelPrompt = EditorGUILayout.TextField("Prompt:", levelPrompt);

            GUILayout.Space(5);
            if (GUILayout.Button("Generate Level", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(assetsDirectory))
                {
                    EditorUtility.DisplayDialog("Error", "Please select an assets directory first!", "OK");
                }
                else if (string.IsNullOrEmpty(levelPrompt))
                {
                    EditorUtility.DisplayDialog("Error", "Please enter a level prompt!", "OK");
                }
                else
                {
                    Debug.Log($"🚀 Level generation started using assets at '{assetsDirectory}' with prompt: '{levelPrompt}'");
                    LevelDesignerTool.LoadPrefabDictionary(assetsDirectory);
                    _ = LevelDesignerTool.GenerateLevelLayout(levelPrompt);
                    // TODO: Connect this to your AI logic
                }
            }
        }

        EditorGUILayout.EndVertical();
    }

    // ========== ANIMATION TOOL ==========
    private void DrawAnimationTool()
    {
        EditorGUILayout.BeginVertical("helpBox");
        showAnimationTool = EditorGUILayout.Foldout(showAnimationTool, "🎬 Animation Generation", true);

        if (showAnimationTool)
        {
            // Object field (Prefab or Transform)
            animationTarget = (GameObject)EditorGUILayout.ObjectField("Target Prefab / Object:", animationTarget, typeof(GameObject), true);

            // Prompt field
            animationPrompt = EditorGUILayout.TextField("Prompt:", animationPrompt);

            // Duration input (float)
            animationDuration = EditorGUILayout.FloatField("Animation Duration (sec):", animationDuration);

            GUILayout.Space(5);
            if (GUILayout.Button("Generate Animation", GUILayout.Height(30)))
            {
                if (animationTarget == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a prefab or transform target!", "OK");
                }
                else if (string.IsNullOrEmpty(animationPrompt))
                {
                    EditorUtility.DisplayDialog("Error", "Please enter an animation prompt!", "OK");
                }
                else if (animationDuration <= 0)
                {
                    EditorUtility.DisplayDialog("Error", "Please enter a valid animation duration greater than 0!", "OK");
                }
                else
                {
                    Debug.Log($"🎞️ Generating animation for '{animationTarget.name}' with prompt: '{animationPrompt}' (Duration: {animationDuration} sec)");
                    _ = AnimationGeneratorTool.GenerateAnimation(animationTarget, animationPrompt, animationDuration);
                    // TODO: Add animation generation logic using AI
                }
            }
        }

        EditorGUILayout.EndVertical();
    }

}
