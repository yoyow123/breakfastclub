using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AgentStatsTags : MonoBehaviour
{
    public Button matchButton, selectButton;
    public Transform tagGroup;
    public Transform selectGroup;
    public Transform additionalTagGroup;
    public GameObject tagPrefab, additionalTagPrefab;
    public GameObject selectionPage;

    public List<string> allTags = new List<string>();
    public List<string> selectedTags = new List<string>();
    [SerializeField] private int maxTags = 6;

    public Agent agent { protected set; get; }

    public Vector3 position_offset;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color matchColor;
    // Start is called before the first frame update
    void Start()
    {
        matchButton.onClick.AddListener(() => FindMatch());
        selectButton.onClick.AddListener(() => EnableSelectionPage());
        selectionPage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LoadTags() {
        if (!agent) return;

        //Additional Tags
        additionalTagGroup.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = agent.personality.additionalTags[0];
        additionalTagGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = agent.gptName;
        
        //Tags
        allTags.AddRange(agent.personality.tags);

        //select the tags randomly
        for (int i = 0; i < maxTags; i++) {
            int rand = Random.Range(0, allTags.Count);
            string tag = allTags[rand];
            if (!selectedTags.Contains(tag))
                selectedTags.Add(tag);

        }

        //selected tags group

        if (selectedTags.Count ==0)
        {

            for (int i = 0; i < tagGroup.childCount; i++)
            {
                GameObject tagObj = tagGroup.GetChild(i).gameObject;
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = selectedTags[i];
            }
        }
        if (allTags.Count > 0)
        {
            //unselected tags
            for (int i = 0; i < allTags.Count; i++)
            {
                GameObject tagObj = Instantiate(tagPrefab, selectGroup);
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = allTags[i];
            }
        }
    }

    public void ResetTags() {
        allTags.Clear();
        selectedTags.Clear();
        if (selectGroup.childCount > 0) {
            for (int i = 0; i < selectGroup.childCount; i++) {
                Destroy(selectGroup.GetChild(i).gameObject);
            }            
        }
    }

    public void SetAgent(Agent newAgent)
    {
        ResetTags();
        agent = newAgent;
        if (agent)
        {
            gameObject.SetActive(true);
            LoadTags();
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

    public void EnableSelectionPage() {
        selectionPage.SetActive(true);
    }

}
