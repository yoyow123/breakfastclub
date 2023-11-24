using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;



[Serializable]
public class AgentTagInfo
{
    public string name = "";
    public List<string> tags;

    public AgentTagInfo() { }

    public AgentTagInfo(string name, List<string> tags) {
        this.name = name;
        this.tags = tags;
    }
}


public class AgentsManager : MonoBehaviour
{
    public string saveFilePath;
    public List<Agent> agents = new List<Agent>();
    public List<Agent> topAgents = new List<Agent>();

    public List<AgentTagInfo> agentTagsLists = new List<AgentTagInfo>();
    public List<string> allComparedTags = new List<string>();
    public List<string> currentMatchedTags = new List<string>();

    public AgentTagInfo currentAgentTagInfo;
    public Agent currentAgent;

    public int maxResults = 5;
    public int maxMatchedTags = 6;

    public bool IsInitCoroutine = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Init());
    }

    // Update is called once per frame
    void Update()
    {
        if (!String.IsNullOrEmpty(currentAgentTagInfo.name))
        {
            GetAllTags();
            SelectMatchedTags();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetTheTopResult();

            Save(saveFilePath);
        }
    }

    void GetTheTopResult() {

        Debug.Log("---Top Result----");

        topAgents = agents.OrderByDescending(t => t.actionCount.totalCount).Take(maxResults).ToList();

        foreach (Agent a in topAgents)
        {
            Debug.Log(string.Format("Name:{0} , Total Count:{1}", a.name, a.actionCount.totalCount)); 
        }
    }
    public void Save(string configPath) {
        string[] datas = new string[maxResults];
        int j = 0;
        for (int i = 0; i < topAgents.Count; i++) {
            AgentResult result = new AgentResult(topAgents[i].personality.name ,topAgents[i].gptName, topAgents[i].actionCount);
           // jsonData += JsonUtility.ToJson(result);
            datas[j] = JsonUtility.ToJson(result,true);
            j++;
        }
        File.WriteAllLines(saveFilePath,datas);
        Debug.Log("Save data");
    }
    public void SetCurrentAgent(Agent agent) {
        currentAgent = agent;
        currentAgentTagInfo = new AgentTagInfo(agent.personality.name, agent.personality.tags.ToList());

    }

    public void GetAllTags()
    {
        for (int i = 0; i < agentTagsLists.Count; i++)
        {
            if (currentAgentTagInfo.name != agentTagsLists[i].name)
            {
                Debug.Log("Current is: " + currentAgentTagInfo.name + ", Add tags from:" + agentTagsLists[i].name);
                allComparedTags.AddRange(agentTagsLists[i].tags);
            }
        }
    }
    public void SelectMatchedTags() {
        var result = currentAgentTagInfo.tags.Intersect(allComparedTags);
        currentMatchedTags.AddRange(result);

        Debug.Log("Matched tags " + currentMatchedTags.Count);
    }

    IEnumerator Init() {

        IsInitCoroutine = true;
        yield return new WaitForEndOfFrame();
        Agent[] list = FindObjectsOfType<Agent>();
        agents.AddRange(list);

        for (int i = 0; i < agents.Count; i++) {
            AgentTagInfo agentTagInfo = new AgentTagInfo(agents[i].personality.name, agents[i].personality.tags.ToList());          
            if(!agentTagsLists.Contains(agentTagInfo))
            agentTagsLists.Add(agentTagInfo);
        
        }

        IsInitCoroutine = false;
    }
}
