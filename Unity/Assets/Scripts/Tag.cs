using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tag : MonoBehaviour
{
    public AgentsManager agentsManager;
    public AgentStatsTags agentStatsTags;
    [SerializeField] private Image img;
    public TextMeshProUGUI tmp;
    public Image highlightImg;
    public Button btn;
    public string text = "";
    public bool isInit = false;
    public bool isSelected = false;
    public bool isHighLight = false;
    public bool isMatched = false;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color matchedColor;

    // Start is called before the first frame update
    void OnEnable()
    {
        //agentsManager = FindObjectOfType<AgentsManager>();
        agentStatsTags = FindObjectOfType<AgentStatsTags>();
        SetHighlight(false);
        btn.onClick.AddListener(() => SetHighlight(!isHighLight));
        btn.onClick.AddListener(() => OnSelect());
    }


    private void Update()
    {
        
    }

    public void SetHighlight(bool isTrue)
    {
        Debug.Log("Highlight :" + isTrue);
        isHighLight = isTrue;
        highlightImg.enabled = isTrue;
    }

    public void OnSelect()
    {
        if (agentStatsTags.tempTags.Count < 6)
        {
            if (isHighLight)
                agentStatsTags.AddTempTags(text);
            else
                agentStatsTags.RemoveTempTags(text);
        }
        else {
            agentStatsTags.RemoveTempTags(text);
        }

    }

    public void Init()
    {
        tmp.text = text;
        if (agentsManager!=null && agentsManager.currentMatchedTags.Contains(text))
        {
            img.color = matchedColor;
            isMatched = true;
        }
        if (agentStatsTags!=null && agentStatsTags.selectedTags.Contains(text))
            isSelected = true;
    }


}
