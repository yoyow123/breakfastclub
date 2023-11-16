using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomHat : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private SpriteRenderer sp;
    [SerializeField] private Color defaultColor;
    public Color neonColor;
    public Color blueColor;
    public Color greenColor;
    public Color orangeColor;
    public Color yellowColor;
    public Color redColor;
    public Color tanColor;
    public Color mintColor;


    // Start is called before the first frame update
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        agent = GetComponentInParent<Agent>();

        if (agent)
        {
            string colorName = agent.personality.color;
           // Debug.Log("Agent " + agent.name + "Color : " + colorName);
            SetColorHat(colorName);

        }
    }

    public void SetColorHat(string colorName) { 
        switch(colorName){
            case "Neon":
                sp.color = neonColor;
                break;
            case "Blue":
                sp.color = blueColor;
                break;
            case "Green":
                sp.color = greenColor;
                break;
            case "Orange":
                sp.color = orangeColor;
                break;
            case "Yellow":
                sp.color = yellowColor;
                break;
            case "Red":
                sp.color = redColor;
                break;
            case "Tan":
                sp.color = tanColor;
                break;
            case "Mint":
                sp.color = mintColor;
                break;
        }
    }
}
