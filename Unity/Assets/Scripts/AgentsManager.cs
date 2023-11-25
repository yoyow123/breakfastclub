using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;



[System.Serializable]
public class FriendListsInfo  {
    public List<AgentResult> datas;
}

[Serializable]
public class AgentTagInfo
{
    public string name = "";
    public Agent agent;
    public List<string> tags;

    public AgentTagInfo() { }

    public AgentTagInfo(Agent agent) {
        this.agent = agent;
        name = agent.personality.name;
        this.tags = agent.personality.tags.ToList();
    }
}


public class AgentsManager : MonoBehaviour
{
    [SerializeField] private AgentStatsTags agentStatsTags;

    public string saveFilePath;
    public List<Agent> agents = new List<Agent>();
    //public List<Friend> topFriends = new List<Agent>();

    public List<AgentTagInfo> agentTagsLists = new List<AgentTagInfo>();
    public List<string> allComparedTags = new List<string>();
    public List<string> currentMatchedTags = new List<string>();
    public FriendListsInfo friendListsInfo;

    public AgentTagInfo currentAgentTagInfo;
    public Agent currentAgent;

    public int maxResults = 5;
    public int maxMatchedTags = 6;

    public bool IsInitCoroutine = false;
    public bool isMatched = false;
    // Start is called before the first frame update
    void Start()
    {
        agentStatsTags = FindObjectOfType<AgentStatsTags>();
        StartCoroutine(Init());
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAgentTagInfo != null && !String.IsNullOrEmpty(currentAgentTagInfo.name))
        {
            if (!isMatched)
            {
                GetAllTags();
                SelectMatchedTags();
                agentStatsTags.SetAgent(currentAgent);
                ShowMatchedInfo();
                isMatched = true;
            }
        }



        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Save(saveFilePath);
        }
    }

    void GetTheTopResult() {

/*        Debug.Log("---Get Result----");
        for (int i = 0; i < agents.Count; i++) {
            var friends = agents[i].friendLists.OrderByDescending(t => t.count).Take(maxResults).ToList();
            AgentResult result = new AgentResult(agents[i].personality.name, friends);}
        */
       
       // topAgents = agents.OrderByDescending(t => t.actionCount.totalCount).Take(maxResults).ToList();
    }
    public void Save(string configPath) {
        friendListsInfo.datas.Clear();
        for (int i = 0; i < agents.Count; i++) {
            var friends = agents[i].friendLists.OrderByDescending(t => t.count).Take(maxResults).ToList();
            AgentResult result = new AgentResult(agents[i].personality.name, agents[i].gptName,agents[i].actionCount, friends);
            if (!friendListsInfo.datas.Contains(result))
                friendListsInfo.datas.Add(result);
        }

        if (friendListsInfo.datas.Count > 0)
        {
            string str = JsonUtility.ToJson(friendListsInfo,true);
            File.WriteAllText(saveFilePath, str);
            Debug.Log("Save data");
        }


    }
    public void SetAgent(Agent agent) {
        currentAgent = agent;
        if(currentAgent !=null)
        currentAgentTagInfo = new AgentTagInfo(currentAgent);

    }

    public void GetAllTags()
    {
        for (int i = 0; i < agentTagsLists.Count; i++)
        {
            if (currentAgentTagInfo.name != agentTagsLists[i].name)
            {
                //Debug.Log("Current is: " + currentAgentTagInfo.name + ", Add tags from:" + agentTagsLists[i].name);
                allComparedTags.AddRange(agentTagsLists[i].tags);
            }
        }
    }
    public void SelectMatchedTags() {
        Debug.Log("---Matched Tags-------");
        var result = currentAgentTagInfo.tags.Intersect(allComparedTags);
        foreach (string str in result)
        {
            if(!currentMatchedTags.Contains(str))
            currentMatchedTags.Add(str);
        }
       // Debug.Log("Matched tags " + currentMatchedTags.Count);
    }

    public void ShowMatchedInfo() {
        for (int i = 0; i < agentTagsLists.Count; i++)
        {
            if (currentAgentTagInfo.name != agentTagsLists[i].name)
            {
                //Debug.Log("Current is: " + currentAgentTagInfo.name + ", Add tags from:" + agentTagsLists[i].name);
                var result = currentAgentTagInfo.tags.Intersect(agentTagsLists[i].tags);
                Debug.Log(string.Format("{0} has {1} matched tags with current Agent {2}", agentTagsLists[i].name, result.Count(), currentAgentTagInfo.name));
                GameObject agent = agentTagsLists[i].agent.gameObject;
                agent.GetComponent<AgentUI>().EnableHeart(result.Count());
            }
            else {
                GameObject agent = currentAgentTagInfo.agent.gameObject;
                agent.GetComponent<AgentUI>().DisableHeart();
            }
        }
    }

    public void ResetState() {
        allComparedTags.Clear();
        currentMatchedTags.Clear();
        isMatched = false;

    }
    IEnumerator Init() {

        IsInitCoroutine = true;
        yield return new WaitForEndOfFrame();
        Agent[] list = FindObjectsOfType<Agent>();
        agents.AddRange(list);

        for (int i = 0; i < agents.Count; i++) {
            AgentTagInfo agentTagInfo = new AgentTagInfo(agents[i]);          
            if(!agentTagsLists.Contains(agentTagInfo))
            agentTagsLists.Add(agentTagInfo);
        
        }

        IsInitCoroutine = false;
    }
}
