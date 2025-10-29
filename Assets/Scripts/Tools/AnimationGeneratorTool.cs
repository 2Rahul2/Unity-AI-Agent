using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class ExtractRigData
{
    public static string GetJsonRigData(GameObject target)
    {
        if (target == null)
        {
            return "{}";
        }
        List<string> boneData = new List<string>();
        foreach (Transform bone in target.GetComponentsInChildren<Transform>())
        {
            string parentName = bone.parent ? bone.parent.name : "None";
            boneData.Add($"{{\"name\":\"{bone.name}\", \"parent\":\"{parentName}\"}}");
        }

        string json = "{ \"bones\": [" + string.Join(",", boneData) + "] }";
        return json;
    }
}
public static class AnimationGeneratorTool
{
    static AI_Client client;
    static GameObject targetObject;
    static string animationFilePath = "Assets/AgentGeneratedAnimations/";
    private static void SetupAIClient()
    {
        if (EditorPrefs.HasKey("AI_API_KEY") == false)
        {
            Debug.LogWarning("AI API Key not found in EditorPrefs under 'AI_API_KEY'. Please set it before using the AnimationGeneratorTool.");
            return;
        }
        string api = EditorPrefs.GetString("AI_API_KEY", "");
        client = new AI_Client(api);
    }
    private static void EnsureAnimationDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder(animationFilePath.TrimEnd('/')))
        {
            AssetDatabase.CreateFolder("Assets", "AgentGeneratedAnimations");
            AssetDatabase.Refresh();
        }
    }
    private static AnimationClip CreateAnimationFile()
    {
        EnsureAnimationDirectoryExists();
        if (targetObject == null)
        {
            Debug.LogError("Target object is null. Cannot create animation file.");
            return null;
        }
        string filePath = System.IO.Path.Combine(animationFilePath, $"GeneratedAnimation_{targetObject.name}.anim");
        // Create the animation file (this is just a placeholder, implement your own logic)
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        AssetDatabase.CreateAsset(clip, filePath);
        AssetDatabase.Refresh();
        return clip;
    }
    public static async Task GenerateAnimation(GameObject targetObject, string prompt, float duration = 2f)
    {
        SetupAIClient();
        if (client == null)
        {
            Debug.LogError("AI Client is not initialized. Cannot generate animation.");
            return;
        }
        AnimationGeneratorTool.targetObject = targetObject;
        // CreateAnimationFile();
        string finalPrompt = @$"
        Generate an 24 FPS animation of {duration} seconds for the given GameObject based on the following prompt: {prompt}
        The GameObject has the following rig structure: {ExtractRigData.GetJsonRigData(targetObject)}";

        string responseFormat = @"
        {
            ""frame_rate"": 24,
            ""duration"": 1.0,
            ""animation"": [
                {
                    ""time"": <float_seconds>,
                    ""bones"": {
                        ""<BoneName>"": {
                            ""position"": [<float_x>, <float_y>, <float_z>],
                            ""rotation"": [<float_x>, <float_y>, <float_z>, <float_w>]
                        }
                    }
                }
            ]
        }";


        string response = await client.SendRequestAsync(finalPrompt, responseFormat);
        // AI_Client.ProcessResponse(response);

        // string res = @"
        // {
        // ""candidates"": [
        //     {
        //     ""content"": {
        //         ""parts"": [
        //         {
        //             ""text"": ""```json\n{\n  \""frame_rate\"": 24,\n  \""duration\"": 1.0,\n  \""animation\"": [\n    {\n      \""time\"": 0.0,\n      \""bones\"": {\n        \""root\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.0, 0.0, 0.0, 1.0]\n        },\n        \""pelvis\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.0, -0.043, 0.0, 0.999]\n        },\n        \""spine_01\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.0, 0.043, 0.0, 0.999]\n        },\n        \""upperarm_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.259, 0.0, 0.0, 0.966]\n        },\n        \""lowerarm_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.087, 0.0, 0.0, 0.996]\n        },\n        \""hand_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.043, 0.0, 0.0, 0.999]\n        },\n        \""upperarm_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [-0.259, 0.0, 0.0, 0.966]\n        },\n        \""lowerarm_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.087, 0.0, 0.0, 0.996]\n        },\n        \""hand_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.043, 0.0, 0.0, 0.999]\n        },\n        \""thigh_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [-0.259, 0.0, 0.0, 0.966]\n        },\n        \""calf_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.0, 0.0, 0.0, 1.0]\n        },\n        \""foot_l\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.087, 0.0, 0.0, 0.996]\n        },\n        \""thigh_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.259, 0.0, 0.0, 0.966]\n        },\n        \""calf_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [0.173, 0.0, 0.0, 0.985]\n        },\n        \""foot_r\"": {\n          \""position\"": [0.0, 0.0, 0.0],\n          \""rotation\"": [-0.259, 0.0, 0.0, 0.966]\n        }\n      }\n    }\n  ]\n}\n```""
        //         }
        //         ],
        //         ""role"": ""model""
        //     },
        //     ""finishReason"": ""STOP"",
        //     ""index"": 0
        //     }
        // ],
        // ""usageMetadata"": {
        //     ""promptTokenCount"": 1029,
        //     ""candidatesTokenCount"": 4607,
        //     ""totalTokenCount"": 13727,
        //     ""promptTokensDetails"": [
        //     {
        //         ""modality"": ""TEXT"",
        //         ""tokenCount"": 1029
        //     }
        //     ],
        //     ""thoughtsTokenCount"": 8091
        // },
        // ""modelVersion"": ""gemini-2.5-flash"",
        // ""responseId"": ""xbX_aLPVJ_P71e8P1r7OoQY""
        // }";

        FetchAnimationData(response);
    }
    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    private static void FetchAnimationData(string response)
    {
        JObject jsonResponse = JObject.Parse(response);
        // UnityEngine.Debug.Log("Animation Generation Raw Response: " + jsonResponse.ToString());
        string text = (string)jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];
        text = text.Replace("```json", "").Replace("```", "").Trim();
        Debug.Log("Extracted Animation JSON: " + text);
        AnimationData animationData = JsonConvert.DeserializeObject<AnimationData>(text);
        AnimationClip clip = CreateAnimationFile();
        if (clip == null)
        {
            Debug.LogError("Failed to create animation clip.");
            return;
        }
        // Process animationData to create keyframes and apply to the targetObject
        foreach (var frame in animationData.animation)
        {
            float time = frame.time;
            Debug.Log($"bones length: {frame.bones.Count}");
            foreach (var boneEntry in frame.bones)
            {
                string boneName = boneEntry.Key;
                BoneTransform boneTransform = boneEntry.Value;
                // Find the bone transform in the targetObject
                Transform bone = FindChildRecursive(targetObject.transform, boneName);
                if (bone != null)
                {
                    Vector3 position = new Vector3(boneTransform.position[0], boneTransform.position[1], boneTransform.position[2]);
                    Quaternion rotation = new Quaternion(boneTransform.rotation[0], boneTransform.rotation[1], boneTransform.rotation[2], boneTransform.rotation[3]);
                    // Here you would create keyframes for position and rotation at the specified time
                    AddKeyframe(clip, targetObject, bone, "localPosition.x", time, position.x);
                    AddKeyframe(clip, targetObject, bone, "localPosition.y", time, position.y);
                    AddKeyframe(clip, targetObject, bone, "localPosition.z", time, position.z);
                    // This is a placeholder for actual keyframe creation logic

                    Quaternion rot = new Quaternion(
                        boneTransform.rotation[0],
                        boneTransform.rotation[1],
                        boneTransform.rotation[2],
                        boneTransform.rotation[3]
                    );
                    Vector3 euler = rot.eulerAngles;
                    AddKeyframe(clip, targetObject, bone, "localRotation.x", time, euler.x);
                    AddKeyframe(clip, targetObject, bone, "localRotation.y", time, euler.y);
                    AddKeyframe(clip, targetObject, bone, "localRotation.z", time, euler.z);
                    Debug.Log($"At time {time}s, Bone: {boneName}, Position: {position}, Rotation: {rotation}");
                }
            }
        }
    }
    private static void AddKeyframe(AnimationClip clip, GameObject root, Transform bone, string property, float time, float value)
    {
        string relativePath = AnimationUtility.CalculateTransformPath(bone, root.transform);
        Debug.Log($"Binding path: {relativePath}, Property: {property}");

        var binding = EditorCurveBinding.FloatCurve(relativePath, typeof(Transform), property);
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding) ?? new AnimationCurve();

        curve.AddKey(new Keyframe(time, value));
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }


    private static string GetRelativePath(Transform root, Transform target)
    {
        if (target == root) return "";
        return AnimationUtility.CalculateTransformPath(target, root);
    }
}

public class BoneTransform
{
    public float[] position;
    public float[] rotation;
}

public class Frame
{
    public float time;
    public Dictionary<string, BoneTransform> bones;
}

public class AnimationData
{
    public int frame_rate;
    public float duration;
    public List<Frame> animation;
}










