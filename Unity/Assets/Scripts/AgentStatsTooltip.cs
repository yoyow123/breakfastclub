using System;
using UnityEngine;
using UnityEngine.UI;


public class AgentStatsTooltip : MonoBehaviour
{

    public Text TitleText;
    public Text NameText;
    public Text AgentText;
    public Text MotivationText;
    public Text HappinessText;
    public Text AttentionText;
    public Text PersonalityText;
    public Text ScoreText;
    public Text LogText;
    public Text ActionText;
    public Text DesireText;

    public Vector3 position_offset;

    public Agent agent { protected set; get; }
    
    // Start is called before the first frame update
    void Start()
    {
/*        TitleText = transform.Find("TitleText").GetComponent<Text>();
        NameText = transform.Find("NameText").GetComponent<Text>();
        AgentText = transform.Find("AgentText").GetComponent<Text>();
        MotivationText = transform.Find("MotivationText").GetComponent<Text>();
        HappinessText = transform.Find("HappinessText").GetComponent<Text>();
        AttentionText = transform.Find("AttentionText").GetComponent<Text>();
        PersonalityText = transform.Find("PersonalityText").GetComponent<Text>();
        ScoreText = transform.Find("ScoreText").GetComponent<Text>();
        LogText = transform.Find("LogText").GetComponent<Text>();
        ActionText = transform.Find("ActionText").GetComponent<Text>();
        DesireText = transform.Find("DesireText").GetComponent<Text>();*/
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

    // Update is called once per frame
    void Update()
    {
        if (agent)
        {
            transform.position = agent.transform.position + position_offset;
            //Debug.Log("Plotting" + transform.position);
            NameText.text = agent.studentname;
            AgentText.text = agent.name;
            MotivationText.text = agent.motivation.ToString("0.00");
            HappinessText.text = agent.happiness.ToString("0.00");
            AttentionText.text = agent.attention.ToString("0.00");
            PersonalityText.text = agent.personality.ToString();
            ScoreText.text = agent.GetScores();
            LogText.text = agent.GetLastMessage();
            ActionText.text = agent.currentAction.ToString();
            DesireText.text = agent.Desire.ToString();

            Debug.Log(String.Format("Rest x {0}", 0)) ;
        }
    }
}
