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
    public bool isFirstPageTag = false;

    // Start is called before the first frame update
    void Start()
    {
        //agentsManager = FindObjectOfType<AgentsManager>();
        agentStatsTags = FindObjectOfType<AgentStatsTags>();
       // btn.onClick.AddListener(() => SetHighlight(!isHighLight));
        btn.onClick.AddListener(() => OnSelect());
    }


    public void SetHighlight(bool isTrue)
    {
        isHighLight = isTrue;
        highlightImg.enabled = isTrue;
    }

    public void OnSelect()
    {
        Debug.Log("---On Selection----");
        if (agentStatsTags.tempTags.Count < 6)
        {
            if (!isSelected)
            {
                agentStatsTags.AddTempTags(text);
                isSelected = true;
            }
            else
            {
                agentStatsTags.RemoveTempTags(text);
                isSelected = false;
            }
        }
        else {
            if (isSelected)
            {
                agentStatsTags.RemoveTempTags(text);
                isSelected = false;
         
            }
        }
        SetHighlight(isSelected);

    }

    public void Init()
    {
        tmp.text = text;
        if (agentsManager != null && agentsManager.currentMatchedTags.Contains(text))
        {
            img.color = matchedColor;
            isMatched = true;
        }
        else {
            img.color = defaultColor;
            isMatched = false;
        }
        if (agentStatsTags != null && agentStatsTags.selectedTags.Contains(text)) {
            isSelected = true;
            if (!isFirstPageTag)
                SetHighlight(true);

        }

    }


}
