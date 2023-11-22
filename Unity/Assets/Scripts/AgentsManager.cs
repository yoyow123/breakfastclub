using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class AgentsManager : MonoBehaviour
{
    public string saveFilePath;
    public List<Agent> agents = new List<Agent>();
    public List<Agent> topAgents = new List<Agent>();
    public int maxResults = 5;

    public bool IsInitCoroutine = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Init());
    }

    // Update is called once per frame
    void Update()
    {  
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetTheTopResult();

            Save(saveFilePath);
        }
    }

    void GetTheTopResult() {

        Debug.Log("---Top Result----");

        topAgents = agents.OrderBy(t => t.actionCount.GetTotalCount()).Take(maxResults).ToList();

        foreach (Agent a in topAgents)
        {
            Debug.Log(string.Format("Name:{0} , Total Count:{1}", a.name, a.actionCount.GetTotalCount())); 
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

    IEnumerator Init() {

        IsInitCoroutine = true;
        yield return new WaitForEndOfFrame();
        Agent[] list = FindObjectsOfType<Agent>();
        agents.AddRange(list);

        IsInitCoroutine = false;
    }
}
