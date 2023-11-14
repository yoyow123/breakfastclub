using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AgentStatsTags : MonoBehaviour
{
    public Transform tagGroup;

    public GameObject tagsPrefab;
    public Agent agent { protected set; get; }

    public Vector3 position_offset;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (agent)
        {       
            for (int i = 0; i < tagGroup.childCount; i++) {
                GameObject tagObj = tagGroup.GetChild(i).gameObject;
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = agent.personality.tags[i];
            }
        }
    }
    public void SetAgent(Agent newAgent)
    {
        agent = newAgent;
        if (agent)
        {
            gameObject.SetActive(true);
        }
        else { gameObject.SetActive(false); }
    }


}
