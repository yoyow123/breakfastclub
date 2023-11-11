using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentStatsTags : MonoBehaviour
{
    public Agent agent { protected set; get; }

    public Vector3 position_offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent) {
            transform.position = agent.transform.position;
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
