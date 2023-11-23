using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tag : MonoBehaviour
{
    [SerializeField] private AgentStatsTags agentStatsTags;
    public TextMeshProUGUI tmp;
    public Image highlightImg;
    public Button btn;
    public string text = "";
    public bool isInit = false;
    public bool isHighLight = false;


    // Start is called before the first frame update
    void Start()
    {
        text = tmp.text;
        agentStatsTags = FindObjectOfType<AgentStatsTags>();
        SetHighlight(false);
        btn.onClick.AddListener(() => SetHighlight(!isHighLight));
        btn.onClick.AddListener(() => OnSelect());
        StartCoroutine(Init());
    }


	private void Update()
	{

    }

    public void SetHighlight(bool isTrue)
    {
        if (isInit)
        {
            if (agentStatsTags.tempTags.Count >= 6)
            {
                if (isHighLight)
                {
                    highlightImg.enabled = false;
                    isHighLight = false;
                }
            }
            else {
                highlightImg.enabled = isTrue;
                isHighLight = isTrue;
            }

        }
        else
        {
            highlightImg.enabled = isTrue;
            isHighLight = isTrue;
        }
    }

    public void OnSelect() {
        if (isInit)
        {
            if (isHighLight)
                agentStatsTags.AddTempTags(text);
            else
                agentStatsTags.RemoveTempTags(text);

        }
    }


    IEnumerator Init() {
        yield return new WaitForEndOfFrame();
        if (!isInit)
        {
            if (agentStatsTags.tempTags.Contains(text))
            {
                if (!isHighLight)
                    SetHighlight(true);
            }
            else
            {
                SetHighlight(false);
            }
            isInit = true;
        }
    }
}
