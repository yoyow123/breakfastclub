using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System;
using Spriter2UnityDX;

public class AgentUI : MonoBehaviour
{
    // SoloTime 0
    // Rest 1
    // Walk 2
    // Diagreement 3
    // Communication 4
    // InteractionTime 5 
    public enum AnimationState : int { SoloTime=0, Walking, Rest,  Diagreement, Communication, InteractionTime};

    public bool isFront;
    public AnimationState animationstate;
    public float distanceMoved;

    private NavMeshAgent navAgent;
    private Camera cam;
    private Agent agent;
    private Canvas UICanvas;
    [SerializeField] private AgentStatsTooltip statsTooltip;
    [SerializeField] private AgentStatsTags statsTags;
    [SerializeField] private TextureController textureController;
    private Animator agentAnimator;
    private Animator bubbleAnimator;

    //private bool showStats = false;
    private TMPro.TextMeshPro AgentNameText;

    private Vector3 prevPosition;

    // Start is called before the first frame update
    void Start()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        cam = FindObjectOfType<Camera>();
        agent = gameObject.GetComponent<Agent>();
        agentAnimator = transform.Find("AgentAnimation").gameObject.GetComponent<Animator>();
        bubbleAnimator = transform.Find("BubbleAnimation").gameObject.GetComponent<Animator>();

        UICanvas = FindObjectOfType<Canvas>();
        statsTooltip = UICanvas.transform.Find("AgentStatsTooltip").GetComponent<AgentStatsTooltip>();
        statsTags = statsTooltip.GetComponentInChildren<AgentStatsTags>();
        AgentNameText = transform.Find("NameText").GetComponent<TMPro.TextMeshPro>();

        AgentNameText.SetText(agent.studentname);
    }

    // Update is called once per frame
    void Update()
    {
        SetAnimationState();
    }

    // Decide animation based on agent.currentAction
    // If agents are too far away from their navAgent destination their animation will be walking
    private void SetAnimationState()
    {
        distanceMoved = Vector3.Distance(transform.position, prevPosition);
        if(distanceMoved > 0.02)
        {
            textureController.SetTexture(0);
            animationstate = AnimationState.Walking;
            isFront = !((transform.position - prevPosition).x < 0.02);
        }
        else
        {
            isFront = true;
            if ((agent.currentAction is Disagreement) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION))
            {
                textureController.SetTexture(2);
                animationstate = AnimationState.Diagreement;
            }
            else
            if (agent.currentAction is Rest)
            {
                textureController.SetTexture(1);
                //animationstate = AnimationState.Walking;
                animationstate = AnimationState.Rest;
            }
            else
            if (((agent.currentAction is SoloTime) || (agent.currentAction is InteractionTime)) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION))
            {
                textureController.SetTexture(0);
                animationstate = AnimationState.SoloTime;
            }
            else
            if ((agent.currentAction is Communication) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION))
            {
                textureController.SetTexture(0);
                animationstate = AnimationState.Communication;
            }
            else
            {
                textureController.SetTexture(0);
                animationstate = AnimationState.SoloTime;
            }
        }
        //isFront = (navAgent.destination - transform.position).z < 0.5;
        //isFront = !((transform.position - prevPosition).z > 0.01) || ((transform.position - prevPosition).x > 0.01);


        //isFront = true;
        prevPosition = transform.position;


        agentAnimator.SetInteger("AgentAnimationState", (int)animationstate);
        agentAnimator.SetBool("IsFront", isFront);
        bubbleAnimator.SetInteger("AgentAnimationState", (int)animationstate);

        /*
        if ((agent.currentAction is Disagreement) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION))
        {
            if (distanceToDestination < 2.0)
            {
                animationstate = AnimationState.Disagreement;
            } else { animationstate = AnimationState.Walking; }
        } else 
        if (agent.currentAction is Break)
        {
            animationstate = AnimationState.Walking;
            //animationstate = AnimationState.Idle;
        } else 
        if (((agent.currentAction is StudyAlone) || (agent.currentAction is StudyGroup)) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION))
        {
            if (distanceToDestination < 1.0)
            {
                animationstate = AnimationState.Study;
            }
            else { animationstate = AnimationState.Walking; }
        } else 
        if ((agent.currentAction is Communication) && (agent.currentAction.state == AgentBehavior.ActionState.ACTION) )
        {
            if (distanceToDestination < 2.0)
            {
                animationstate = AnimationState.Communication;
            }
            else { animationstate = AnimationState.Walking; }
        }
        else
        {
            if (distanceToDestination < 1.0)
            { animationstate = AnimationState.Idle; }
            else { animationstate = AnimationState.Walking; }
        }*/
    }

    void OnMouseDown()
    {
        //return;
        //If your mouse hovers over the GameObject with the script attached, output this message
        //Debug.Log(string.Format("OnMouseDown GameObject {0}.", this.name));
        if(statsTooltip.agent == agent)
        {
            //Debug.Log(string.Format("Dissable stats."));
            statsTooltip.SetAgent(null);
            statsTags.SetAgent(null);
        }
        else
        {
            //Debug.Log(string.Format("Enable stats."));
            statsTooltip.SetAgent(agent);
            statsTags.SetAgent(agent);

        }   
    }
}
