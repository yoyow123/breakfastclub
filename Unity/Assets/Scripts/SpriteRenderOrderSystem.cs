using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderOrderSystem : MonoBehaviour
{
    SpriteRenderer[] renderers;
    // Start is called before the first frame update
    void Start()
    {
       renderers = FindObjectsOfType<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SpriteRenderer renderer in renderers) {
            renderer.sortingOrder = (int)renderer.transform.position.y * -100;
        }
    }
}
