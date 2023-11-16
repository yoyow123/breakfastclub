using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderOrderSystem : MonoBehaviour
{
   [SerializeField]  private SpriteRenderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SpriteRenderer renderer in renderers) {
            int defaultOrder = renderer.sortingOrder;
            if (transform.position.z < -0.75 && renderer.sortingOrder -1 == defaultOrder )
                renderer.sortingOrder++;
            else
                renderer.sortingOrder = defaultOrder;
        }   
    }
}
