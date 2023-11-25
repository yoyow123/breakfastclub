using System;
using UnityEngine;

public class Rest : AgentBehavior
{
    private float xS;
    private float zS;

    public Rest(Agent agent) : base(agent, AgentBehavior.Actions.Rest, "Rest", agent.SC.Rest) {
        xS = agent.classroom.groundfloorTransform.GetComponentInChildren<Collider>().bounds.size.x * (float)0.5;
        zS = agent.classroom.groundfloorTransform.GetComponentInChildren<Collider>().bounds.size.z * (float)0.5;
    }

    /*
    • requirements: free spot on individual table
    • effect: regenerate energy, will increase happiness(amount is a function of extraversion)
    */
    public override bool possible()
    {
        return true;
    }

    public override double rate()
    {
        double score = CalculateScore(1.0 - agent.personality.extraversion, config["PERSONALITY_WEIGHT"], ExpDecay(agent.motivation), config["MOTIVATION_WEIGHT"], ExpGrowth(agent.happiness), config["HAPPINESS_WEIGHT"]);
        return score;
    }

    public override bool execute()
    {
        agent.motivation = boundValue(0.0, agent.motivation + config["MOTIVATION_INCREASE"], 1.0);
        agent.happiness = boundValue(0.0, agent.happiness + config["HAPPINESS_INCREASE"], 1.0);


        Vector3 dest;
        // Random draw, decide to walk or stay where put
        if (agent.random.Next(100) > 50)
        {
            // Perform a random walk in the classroom
            dest = agent.classroom.groundfloorTransform.TransformPoint((50 - agent.random.Next(100)) * xS / 100.0f, 0.0f, (50 - agent.random.Next(100)) * zS / 100.0f);
        }
        else
        {
            dest = agent.navagent.transform.position;
        }

        //Debug.Log("Random walk towards " + dest);
        agent.navagent.SetDestination(dest);

        state = ActionState.ACTION;

        return true;
    }

    public override void end()
    {
        agent.AddActionCount(this);
        state = ActionState.INACTIVE;
    }
}
