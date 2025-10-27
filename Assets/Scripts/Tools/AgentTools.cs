using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class ToolListToSend
{
    public string toolName { get; set; }
    public string description { get; set; }
    public List<string> parameters { get; set; }
    public string returnType { get; set; }
}
public static class AgentTools
{
    public static void ExecuteAgentTools(string toolName , JObject parameters)
    {
        MethodInfo methodInfo = typeof(AgentTools).GetMethod(toolName, BindingFlags.Public | BindingFlags.Static);
        if (methodInfo == null)
        {
            Debug.LogError($"Tool not found: {toolName}");
            return;
        }
        ParameterInfo[] paramsInfo = methodInfo.GetParameters();
        object[] parsedParams = new object[paramsInfo.Length];
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            var param = paramsInfo[i];
            string paramName = param.Name;
            if (parameters.TryGetValue(paramName, out JToken token))
            {
                parsedParams[i] = token.ToObject(param.ParameterType);
            }
            else
            {
                parsedParams[i] = Type.Missing;
            }
        }
        methodInfo.Invoke(null, parsedParams);

    }
    public static void CreateCuboid(string name, Vector3 position, Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
    }
    public static void CreateSphere(string name, Vector3 position, float radius)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(radius, radius, radius);
    }
    public static void CreateCapsule(string name, Vector3 position, float height, float radius)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.position = position;
        capsule.transform.localScale = new Vector3(radius, height, radius);
    }
    public static void CreatePlane(string name, Vector3 position, Vector2 size)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = name;
        plane.transform.position = position;
        plane.transform.localScale = new Vector3(size.x / 10f, 1, size.y / 10f); // Plane default size is 10x10
    }
    public static void CreateCylinder(string name, Vector3 position, float height, float radius)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.position = position;
        cylinder.transform.localScale = new Vector3(radius, height / 2f, radius); // Height is along Y-axis
    }
    public static void SpawnPrefabAtPosition(string prefabName, Vector3 position, Quaternion rotation)
    {
        LevelDesignerTool.SpawnPrefabAtPosition(prefabName, position, rotation);
    }
}











