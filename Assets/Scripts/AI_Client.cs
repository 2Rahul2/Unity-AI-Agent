using UnityEngine;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
public class AI_Client
{
    private readonly HttpClient httpClient;
    public AI_Client(string apiKey)
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }
    public async Task<string> SendRequestAsync(string prompt, string responseFormat, List<ToolListToSend> tools = default) 
    {
        string finalPrompt = @$"
        You are a game assistant. You can choose tools from the list, you can use tools multiple times and respond strictly in JSON format.
        ### Available Tools:
        {JsonConvert.SerializeObject(tools, Formatting.Indented)}
        ### Rules:
        - Always respond in JSON format.
        - Format:
        {responseFormat}
        ### User Prompt:
        {prompt}
        ";
        Debug.Log("Final Prompt Sent to AI: " + finalPrompt);
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new {text = finalPrompt}
                    }
                }
            }
        };
        StringContent content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await httpClient.PostAsync(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent", content
        );
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();

        return responseString;
    }
    public static void ProcessResponse(string response)
    {
        JObject jsonResponse = JObject.Parse(response);
        UnityEngine.Debug.Log("Agent Raw Response: " + jsonResponse.ToString());
        string text = (string)jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];
        text = text.Replace("```json", "").Replace("```", "").Trim();

        Root root = JsonConvert.DeserializeObject<Root>(text);
        UnityEngine.Debug.Log("Agent Prompt Summary: " + root.prompt);
        foreach (var action in root.actions)
        {
            AgentTools.ExecuteAgentTools(action.tool, action.parameters);
        }
        Debug.Log("Agent Response : " + text);
        // OnAgentResponseTextMessage?.Invoke(text);
    }
    public class Root
    {
        public string prompt { get; set; }
        public List<Actions> actions { get; set; }
    }
    public class Actions
    {
        public string tool { get; set; }
        public JObject parameters { get; set; }
    }
}
public class Rig
{
    public Transform rootBone;
    void ExportRigInfo()
    {
        List<string> boneData = new List<string>();
        foreach (Transform bone in rootBone.GetComponentsInChildren<Transform>())
        {
            string parentName = bone.parent ? bone.parent.name : "None";
            boneData.Add($"{{\"name\":\"{bone.name}\", \"parent\":\"{parentName}\"}}");
        }

        string json = "{ \"bones\": [" + string.Join(",", boneData) + "] }";
        Debug.Log(json);
    }

}










