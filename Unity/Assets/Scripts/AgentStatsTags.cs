using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AgentStatsTags : MonoBehaviour
{
    public Button matchButton;
    public Transform tagGroup;
    public Transform additionalTagGroup;
    public GameObject tagsPrefab, additionalTagsPrefab;
    public Agent agent { protected set; get; }

    public Vector3 position_offset;

    [SerializeField] private int maxTags = 5;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color matchColor;
    // Start is called before the first frame update
    void Start()
    {
        matchButton.onClick.AddListener(() => FindMatch());
    }

    // Update is called once per frame
    void Update()
    {
        if (agent)
        {
            for (int i = 0; i < additionalTagGroup.childCount; i++)
            {
                GameObject tagObj = additionalTagGroup.GetChild(i).gameObject;
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = agent.personality.additionalTags[i];
            }

            for (int i = 0; i < tagGroup.childCount; i++)
            {
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

    public void FindMatch()
    {
        RefreshTags();

        int randNum = Random.Range(0, additionalTagGroup.childCount);
        GameObject g = additionalTagGroup.GetChild(randNum).gameObject;
        g.transform.Find("TagsInColor").GetComponent<Image>().color = matchColor;

        for (int i = 0; i < 3; i++)
        {
            int num = Random.Range(0, tagGroup.childCount);
            GameObject tag = tagGroup.GetChild(randNum).gameObject;
            tag.transform.Find("TagsInColor").GetComponent<Image>().color = matchColor;
        }
    }

    public void RefreshTags() {

        for (int i = 0; i < additionalTagGroup.childCount; i++)
        {
            GameObject tagObj = additionalTagGroup.GetChild(i).gameObject;
            tagObj.transform.Find("TagsInColor").GetComponent<Image>().color = defaultColor;
        }

        for (int i = 0; i < tagGroup.childCount; i++)
        {
            GameObject tagObj = tagGroup.GetChild(i).gameObject;
            tagObj.transform.Find("TagsInColor").GetComponent<Image>().color = defaultColor;
        }
    }

}
