using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderOrderSystem : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private List<int> defaultOrders;
    private bool isAdded = false;

    // Start is called before the first frame update
    void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            defaultOrders.Add(renderer.sortingOrder);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder += 3;
            isAdded = true;
        }

        foreach (SpriteRenderer renderer in renderers)
        {
            if (agent.currentAction.action != AgentBehavior.Actions.InteractionTime)
            {
                foreach (int i in defaultOrders)
                    renderer.sortingOrder = i;
            }
            isAdded = false;
        }

    }
}
