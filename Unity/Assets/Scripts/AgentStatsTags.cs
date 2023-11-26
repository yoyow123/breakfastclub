using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AgentStatsTags : MonoBehaviour
{
    [SerializeField] private AgentsManager agentsManager;
    public Button matchButton, selectButton;
    public Button nextButton, confirmButton;

    public Transform additionalTagGroup;
    public Transform selectTagsGroup;


    public GameObject tagPrefab;
    public GameObject selectGroupPrefab;
    public List<GameObject> selectGroups = new List<GameObject>();
    public Transform firstPage;
    public Transform selectionPage;

    public int currentPageIndex = 0;

    [HideInInspector] public List<string> allTags = new List<string>();
    public List<string> selectedTags = new List<string>();
    public List<string> tempTags = new List<string>();
    [SerializeField] private int maxTags = 6;
    [SerializeField] private int maxTagPerGroup = 9;

    public Agent agent { protected set; get; }

    public Vector3 position_offset;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color matchColor;

    [SerializeField] private bool isInit = false;
    private bool isTagCoroutine = false;
    // Start is called before the first frame update
    void Start()
    {
        agentsManager = FindObjectOfType<AgentsManager>();
        selectButton.onClick.AddListener(() => EnableSelectionPage());
        nextButton.onClick.AddListener(() => NextSelectionPage());
        confirmButton.onClick.AddListener(() => ConfirmTags());
        selectionPage.gameObject.SetActive(false);
        firstPage.gameObject.SetActive(true);
    }
    public void SetAgent(Agent newAgent)
    {
        agent = newAgent;
        if (agent)
        {
            gameObject.SetActive(true);
            firstPage.gameObject.SetActive(true);
            LoadDefaultTags();
            UpdateTagObject();
            LoadAllTags();

        }
        else { gameObject.SetActive(false); }
    }


    public void UpdateTagObject()
    {
        Debug.Log("-------Generate Tag Group----");

        //prepare enough selection groups
        if (allTags.Count > 0)
        {
            //17/9 = 2
            int division = allTags.Count / maxTagPerGroup;
            int remainder = allTags.Count % maxTagPerGroup;

            if (remainder > 0)
                division += 1;

            Debug.Log("***ALL TAG:" + allTags.Count + ",Need : " + division);

            int num = 0;

            while (num != division)
            {
                GameObject groupObj = Instantiate(selectGroupPrefab, selectionPage);
                if (!selectGroups.Contains(groupObj))
                    selectGroups.Add(groupObj);
                num++;
            }
            // return if insufficient group count
            if (selectGroups.Count != num)
                return;


        }
    }
    public void LoadAllTags() {
        Debug.Log("-------Load All tags---");
        int tagIndex = 0;

        while (tagIndex < allTags.Count)
        {
            GameObject tagObj = Instantiate(tagPrefab, selectGroups[GetGroupNum(tagIndex)].transform);
            Tag tag = tagObj.GetComponent<Tag>();
            tag.text = allTags[tagIndex];
            tag.agentsManager = agentsManager;
            tag.agentStatsTags = this;
            tag.Init();
            tagIndex++;
        }
    }
    private int GetGroupNum(int tagIndex) {
        return tagIndex / maxTagPerGroup;
    
    }
    public void LoadDefaultTags()
    {
        if (!agent) return;

        Debug.Log("---Load Default Tags---");
        //Additional Tags
        additionalTagGroup.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = agent.personality.additionalTags[0];
        additionalTagGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = agent.gptName;

        // Get all tags of the current agent
        if (allTags.Count == 0)
            allTags.AddRange(agent.personality.tags);

        //Get all matched tags and show in ui by default
        if (!isTagCoroutine)
            StartCoroutine(SelectTagsCoroutine());

        Debug.Log("**Select Tags :" + selectedTags.Count + ", TagsCount : " + selectTagsGroup.childCount);
    }

    public void ResetState()
    {
        allTags.Clear();
        selectedTags.Clear();
        tempTags.Clear();

        for (int i = 0; i < selectGroups.Count; i++)
            Destroy(selectGroups[i]);

        selectGroups.Clear(); 

        selectionPage.gameObject.SetActive(false);
    }

    public void EnableFirstPage() {
        firstPage.gameObject.SetActive(true);
    }


    public void EnableSelectionPage() {
        firstPage.gameObject.SetActive(false);
        selectionPage.gameObject.SetActive(true);
        selectGroups[0].SetActive(true);
        for (int i = 1; i < selectGroups.Count; i++) {
            selectGroups[i].SetActive(false);
        }
    }

    public void NextSelectionPage() {
        for (int i = 0; i < selectGroups.Count; i++)
        {
            selectGroups[i].SetActive(false);
        }
        if (currentPageIndex + 1 < selectGroups.Count)
            currentPageIndex++;
        else
            currentPageIndex = 0;


        selectGroups[currentPageIndex].SetActive(true);

    }

    public void AddTempTags(string text) {
        if (tempTags.Count < maxTags) {
            if (!tempTags.Contains(text))
                tempTags.Add(text);
        }
    }

    public void RemoveTempTags(string text) {
        if (tempTags.Count > 0)
        {
            if (tempTags.Contains(text))
                tempTags.Remove(text);
        }
    }

    public void ConfirmTags() {

        Debug.Log("---Confirm Tags---");
        selectedTags.Clear();
        if (selectedTags.Count == 0)
            selectedTags.AddRange(tempTags);

        if (selectedTags.Count != selectTagsGroup.childCount)
        {
            int r = selectTagsGroup.childCount - selectedTags.Count;
            // when the number of selected tag is not 6
            for (int j = 6 - r - 1; j < selectTagsGroup.childCount; j++)
            {
                GameObject tagObj = selectTagsGroup.GetChild(j).gameObject;
                Tag tag = tagObj.GetComponent<Tag>();
                tag.text = "";
                tag.agentsManager = agentsManager;
                tag.agentStatsTags = this; 
                tag.Init();
            }
        }

        for (int i = 0; i < selectedTags.Count; i++)
        {
            GameObject tagObj = selectTagsGroup.GetChild(i).gameObject;
            Tag tag = tagObj.GetComponent<Tag>();
            tag.text = selectedTags[i];
            tag.agentsManager = agentsManager;
            tag.agentStatsTags = this; 
            tag.Init();
        }


        // when user just highlight 4,then it show 

        selectionPage.gameObject.SetActive(false);
        firstPage.gameObject.SetActive(true);

    }


    private IEnumerator SelectTagsCoroutine() {

        isTagCoroutine = true;

        Debug.Log("**All Matched tags: " + agentsManager.currentMatchedTags.Count);
        foreach (string s in agentsManager.currentMatchedTags) {
            //Debug.Log("Tag: " + s);
        }

        //  add the matched tags to list
        for (int i = 0; i < agentsManager.currentMatchedTags.Count; i++)
        {
            if (!selectedTags.Contains(agentsManager.currentMatchedTags[i]) && selectedTags.Count !=maxTags)
                selectedTags.Add(agentsManager.currentMatchedTags[i]);
        }

        //if less than 6,  then add other unmatched tags to list
        if (selectedTags.Count < maxTags) {
            for (int i = 0; i < allTags.Count; i++) 
            {
                if (!selectedTags.Contains(allTags[i]) && selectedTags.Count != maxTags)
                    selectedTags.Add(allTags[i]);
            }
        
        }


        if (selectedTags.Count==maxTags)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject tagObj = selectTagsGroup.GetChild(i).gameObject;
                Tag tag = tagObj.GetComponent<Tag>();
                tag.text = selectedTags[i];
                tag.agentsManager = agentsManager;
                tag.agentStatsTags = this;;
                tag.Init();

            }
        }
        if (tempTags.Count ==0)
            tempTags.AddRange(selectedTags);

        yield return null;
        isTagCoroutine = false;
    }

}
