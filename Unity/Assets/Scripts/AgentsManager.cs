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
    public List<string> tags;

    public AgentTagInfo() { }

    public AgentTagInfo(string name, List<string> tags) {
        this.name = name;
        this.tags = tags;
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
            AgentResult result = new AgentResult(agents[i].personality.name, friends);
            if (!friendListsInfo.datas.Contains(result))
                friendListsInfo.datas.Add(result);
        }

/*        for (int i = 0; i < friendListsInfo.datas.Count; i++) {
            Debug.Log(friendListsInfo.datas[i].name);
        }
*/
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
        currentAgentTagInfo = new AgentTagInfo(currentAgent.personality.name, currentAgent.personality.tags.ToList());

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
            AgentTagInfo agentTagInfo = new AgentTagInfo(agents[i].personality.name, agents[i].personality.tags.ToList());          
            if(!agentTagsLists.Contains(agentTagInfo))
            agentTagsLists.Add(agentTagInfo);
        
        }

        IsInitCoroutine = false;
    }
}
