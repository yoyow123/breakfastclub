using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OpenURL : MonoBehaviour
{
    [SerializeField] private AgentStatsTooltip tooltip;
    public string url;
    // Start is called before the first frame update

    public void OnMouseDown() {
        if(!tooltip.isActived)
        Application.OpenURL(url);
    }
}
