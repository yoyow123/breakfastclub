using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AgentStatsTooltip : MonoBehaviour
{

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI AgentText;
    public TextMeshProUGUI typeNameText;
    public TextMeshProUGUI MotivationText;
    public TextMeshProUGUI HappinessText;
    public TextMeshProUGUI AttentionText;
    public TextMeshProUGUI PersonalityText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI LogText;
    public TextMeshProUGUI ActionText;
    public TextMeshProUGUI currentActionText;
    public TextMeshProUGUI prevActionText;
    public TextMeshProUGUI DesireText;
    public Vector3 position_offset;
    public Agent agent { protected set; get; }

    public bool isActived = false;
    
    // Start is called before the first frame update
    void Start()
    {
        /*
        TitleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        NameText = transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        AgentText = transform.Find("AgentText").GetComponent<TextMeshProUGUI>();
        MotivationText = transform.Find("MotivationText").GetComponent<TextMeshProUGUI>();
        HappinessText = transform.Find("HappinessText").GetComponent<TextMeshProUGUI>();
        AttentionText = transform.Find("AttentionText").GetComponent<TextMeshProUGUI>();
        PersonalityText = transform.Find("PersonalityText").GetComponent<TextMeshProUGUI>();
        ScoreText = transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        LogText = transform.Find("LogText").GetComponent<TextMeshProUGUI>();
        ActionText = transform.Find("ActionText").GetComponent<TextMeshProUGUI>();
        DesireText = transform.Find("DesireText").GetComponent<TextMeshProUGUI>();*/
    }

    public void SetAgent(Agent newAgent)
    {
        agent = newAgent;
        if (agent)
        {
            gameObject.SetActive(true);
            isActived = true;
        }
        else { 
            gameObject.SetActive(false);
            isActived = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (agent)
        {
            transform.position = agent.transform.position + position_offset;
            //Debug.Log("Plotting" + transform.position);
            NameText.text = agent.studentname;
            AgentText.text = agent.name;
            typeNameText.text = string.Format("{0} & {1}", agent.personality.typeName1, agent.personality.typeName2);
            MotivationText.text = GetLevelText(agent.motivation);
            HappinessText.text = GetLevelText(agent.happiness);
            AttentionText.text = String.Format("{0} - {1}", GetLevelText(agent.attention), agent.personality.roleDescription);
            //PersonalityText.text = agent.personality.ToString();
            ScoreText.text = agent.GetScores();
            LogText.text = agent.GetLastMessage();
            ActionText.text = agent.currentAction.ToString();
            currentActionText.text = agent.currentAction.ToString();
            prevActionText.text = agent.GetPreviousActionLists();
           // DesireText.text = agent.Desire.ToString();
        }
    }
    string GetLevelText(double value) {
        string level = "";
        if (value < 0.3)
            level = "Low";
        else if (value >= 0.3 && value < 0.6)
        {
            level = "Medium";
        }
        else if (value >= 0.6)
            level = "High";

        return level;
    }
}
