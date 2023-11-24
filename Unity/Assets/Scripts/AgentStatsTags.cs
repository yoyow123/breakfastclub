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
    private bool isRandomCoroutine = false;
    // Start is called before the first frame update
    void Start()
    {
        agentsManager = FindObjectOfType<AgentsManager>();
        matchButton.onClick.AddListener(() => FindMatch());
        selectButton.onClick.AddListener(() => EnableSelectionPage());
        nextButton.onClick.AddListener(() => NextSelectionPage());
        confirmButton.onClick.AddListener(() => ConfirmTags());
        selectionPage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateTagObject()
    {

        //   if (selectedTags.Count != 0) return;

        // update the info of selected tags

        //prepare enough selection groups
        if (allTags.Count > 0)
        {
            //17/9 = 1
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

            int tagIndex = 0;
            
            while (tagIndex < allTags.Count)
            {
                GameObject tagObj = Instantiate(tagPrefab, selectGroups[GetGroupNum(tagIndex)].transform);
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = allTags[tagIndex];
                tagIndex++;
            }
        }
    }
    private int GetGroupNum(int tagIndex) {
        return tagIndex / maxTagPerGroup;
    
    }
    public void LoadTags()
    {
        if (!agent) return;

        Debug.Log("---Load Tags---");
        //Additional Tags
        additionalTagGroup.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = agent.personality.additionalTags[0];
        additionalTagGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = agent.gptName;

        //Tags
        if (!isRandomCoroutine)
            StartCoroutine(SelectTagsCoroutine());

        if (selectedTags.Count == selectTagsGroup.childCount)
        {
            for (int i = 0; i < selectTagsGroup.childCount; i++)
            {
                GameObject tagObj = selectTagsGroup.GetChild(i).gameObject;
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = selectedTags[i];

            }
        }
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

    public void SetAgent(Agent newAgent)
    {
        agent = newAgent;
        if (agent)
        {
            gameObject.SetActive(true);
            LoadTags();
            UpdateTagObject();
        }
        else { gameObject.SetActive(false); }
    }



    public void FindMatch()
    {
     /*   RefreshTags();

        int randNum = Random.Range(0, additionalTagGroup.childCount);
        GameObject g = additionalTagGroup.GetChild(randNum).gameObject;
        g.transform.Find("TagsInColor").GetComponent<Image>().color = matchColor;

        for (int i = 0; i < 3; i++)
        {
            int num = Random.Range(0, tagGroup.childCount);
            GameObject tag = tagGroup.GetChild(randNum).gameObject;
            tag.transform.Find("TagsInColor").GetComponent<Image>().color = matchColor;
        }*/
    }

    public void RefreshTags() {

     /*   for (int i = 0; i < additionalTagGroup.childCount; i++)
        {
            GameObject tagObj = additionalTagGroup.GetChild(i).gameObject;
            tagObj.transform.Find("TagsInColor").GetComponent<Image>().color = defaultColor;
        }

        for (int i = 0; i < tagGroup.childCount; i++)
        {
            GameObject tagObj = tagGroup.GetChild(i).gameObject;
            tagObj.transform.Find("TagsInColor").GetComponent<Image>().color = defaultColor;
        }*/
    }

    public void EnableSelectionPage() {
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
        selectedTags.Clear();
        if (selectedTags.Count == 0)
            selectedTags.AddRange(tempTags);

        if (selectedTags.Count != selectTagsGroup.childCount)
        {
            int r = selectTagsGroup.childCount - selectedTags.Count;

            for (int j = 6 - r - 1; j < selectTagsGroup.childCount; j++)
            {
                GameObject tagObj = selectTagsGroup.GetChild(j).gameObject;
                tagObj.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }

        for (int i = 0; i < selectedTags.Count; i++)
        {
            GameObject tagObj = selectTagsGroup.GetChild(i).gameObject;
            tagObj.GetComponentInChildren<TextMeshProUGUI>().text = selectedTags[i];
        }


        // when user just highlight 4,then it show 

        selectionPage.gameObject.SetActive(false);

    }


    private IEnumerator SelectTagsCoroutine() {

        isRandomCoroutine = true;
        allTags.AddRange(agent.personality.tags);
       // Debug.Log("**All Matched tags: " + agentsManager.currentMatchedTags.Count);
        //select the tags randomly
        while (selectedTags.Count  != maxTags)
        {
            int rand = Random.Range(0, agentsManager.currentMatchedTags.Count);
            string tag = agentsManager.currentMatchedTags[rand];
            // selected tags randomly
            if (!selectedTags.Contains(tag))
                selectedTags.Add(tag);

        }
        if (tempTags.Count ==0)
            tempTags.AddRange(selectedTags);
        yield return null;
        isRandomCoroutine = false;
    }

}
