using System;

public class Disagreement : AgentBehavior
{
    private Agent otherAgent;

    public Disagreement(Agent agent) : base(agent, AgentBehavior.Actions.Disagreement, "Disagreement", agent.SC.Disagreement) { }  

    public override bool possible()
    {
    
        switch(state)
        {
            // Start to engage another agent
            case ActionState.INACTIVE:
                if (engageOtherAgent())
                {
                    state = ActionState.MOVING;
                    transition_cnter = 2;
                    return true;
                }
                return false;

            case ActionState.MOVING:
                transition_cnter--;
                if(transition_cnter > 0)
                {

                }
                else
                {
                    state = ActionState.AWAITING;
                }
                return true;

            // Either Change to active if the other agent is responing, or try to interact again
            // If we tried long enough, change to another target.
            case ActionState.AWAITING:
                if ((otherAgent.Desire is Disagreement) || (otherAgent.currentAction is Disagreement))
                {
                    agent.LogDebug($"{otherAgent} is already Disagreementing, just join!");
                    state = ActionState.ACTION;
                }
                else
                {
                    // We have someone we want to Disagreement with but they have not responded 'yet', so try to convince them
                    //if (retry_cnter >= (int)(config["MAX_RETRIES"]* agent.personality.conscientousness))
                    if (retry_cnter >= (int)config["MAX_RETRIES"])
                    {
                        agent.LogDebug(String.Format("Giving up to quarel with {0}. Will try another agent ...", otherAgent));
                        //engageOtherAgent();
                        state = ActionState.INACTIVE;
                    }
                    else
                    {
                        retry_cnter++;
                        otherAgent.Interact(agent, this);
                        agent.navagent.destination = otherAgent.transform.position;
                        agent.LogDebug(String.Format("Trying again {0} to Disagreement with {1}", retry_cnter, otherAgent));
                    }
                }
                return true;
            case ActionState.ACTION:
                if ((otherAgent.Desire is Disagreement) || (otherAgent.currentAction is Disagreement))
                {
                    agent.LogDebug($"Continou to Disagreement with {otherAgent} ...");
                    return true;
                } else {
                    // The other left; Execution will return false
                    agent.LogDebug(String.Format("Other agent {0} has left the Disagreement ...", otherAgent));
                    otherAgent = null;
                    state = ActionState.INACTIVE;
                    return false;
                }

        }

        return false;
    }

    public override double rate()
    {
        double score = CalculateScore(1.0 - agent.personality.agreeableness, config["PERSONALITY_WEIGHT"], ExpGrowth(agent.motivation), config["MOTIVATION_WEIGHT"], ExpDecay(agent.happiness, power: 4), config["HAPPINESS_WEIGHT"]);
        return score;
    }

    public override bool execute()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
            {
                agent.LogError(String.Format("Trying to find someone to Disagreement with!"));
                if (engageOtherAgent())
                {
                    state = ActionState.MOVING;
                }
                return true;
            }

            case ActionState.MOVING:
            {
                (double energy, double happiness) = calculateTransitionEffect();
                agent.motivation = energy;
                agent.happiness = happiness;
                agent.navagent.destination = otherAgent.transform.position;
                return true;
            }

            case ActionState.AWAITING:
            {
                (double energy, double happiness) = calculateWaitingEffect();
                agent.motivation = energy;
                agent.happiness = happiness;
                agent.navagent.destination = otherAgent.transform.position;
                return true;
            }

            case ActionState.ACTION:
                agent.LogDebug($"Continue to Disagreement with {otherAgent} ...");
                agent.motivation = boundValue(0.0, agent.motivation + config["MOTIVATION_INCREASE"], 1.0);
                agent.happiness = boundValue(0.0, agent.happiness + config["HAPPINESS_INCREASE"], 1.0);
                agent.navagent.destination = otherAgent.transform.position;
                return true;
        }
        return false;
    }

    public override void end()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                // It can happen if the other one left the Disagreement, and than we end Disagreement
                //agent.LogError(String.Format("This should not happen!"));
                //throw new NotImplementedException();
                break;

            case ActionState.MOVING:
                // It can happen if the other one left the chat, and than we end chat
                break;

            case ActionState.AWAITING:
                agent.LogDebug(String.Format("Giving up to wait for {0}!", otherAgent));
                break;

            case ActionState.ACTION:
                agent.LogDebug(String.Format("Ending Disagreement with {0}!", otherAgent));
                otherAgent = null;

                // Give the agent an happiness boost in order to not start Disagreement again imediately
                agent.happiness += config["HAPPINESS_BOOST"];
                break;
        }
        retry_cnter = 0;
        state = ActionState.INACTIVE;
    }

    // Find another agent to chat with
    private bool engageOtherAgent()
    {
        // Reset retry counter for all conditions
        retry_cnter = 0;

        if (agent.classroom.agents.Length == 1)
        {
            agent.LogDebug(String.Format("No other Agent to Disagreement with!"));
            return false;
        }

        // Select a random other agent
        int idx;
        do
        {
            idx = agent.random.Next(agent.classroom.agents.Length);
            otherAgent = agent.classroom.agents[idx];
        } while (otherAgent == agent);

        agent.LogDebug(String.Format("Agent tries to Disagreement with agent {0}!", otherAgent));
        otherAgent.Interact(agent, this);
        agent.navagent.destination = otherAgent.transform.position;

        return true;
    }

    public void acceptInviation(Agent otherAgent)
    {
        agent.LogDebug(String.Format("{0} is accepting invitation to Disagreement with {1}!", agent, otherAgent));
        this.otherAgent = otherAgent;
        state = ActionState.MOVING;
    }

    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format($"{name}({state})");
            case ActionState.MOVING:
                return String.Format($"{name}({state}) moving towards {otherAgent.studentname} to resolve differences");
            case ActionState.AWAITING:
                return String.Format($"{name}({state}) awaiting reconciliation with {otherAgent.studentname}");
            case ActionState.ACTION:
                return String.Format($"{name}({state}) finding common ground with {otherAgent.studentname}");
        }
        return "Invalid State!";
    }
}
