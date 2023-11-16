using System;

public class Communication : AgentBehavior
{
    private Agent otherAgent;

    public Communication(Agent agent) : base(agent, AgentBehavior.Actions.Communication, "Communication", agent.SC.Communication) { }

    // An Agent can Communication if there is another Agent disponible
    public override bool possible()
    {

        switch (state)
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
                if (transition_cnter > 0)
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
                if ((otherAgent.Desire is Communication) || (otherAgent.currentAction is Communication))
                {
                    agent.LogDebug(String.Format("Agent {0} is ready to Communication, lets go ...", otherAgent));
                    state = ActionState.ACTION;
                }
                else
                {
                    // We have someone we want to quarrel with but they have not responded 'yet', so try to convince them
                    //if (retry_cnter >= (int)(config["MAX_RETRIES"]*agent.personality.conscientousness))
                    if (retry_cnter >= (int)config["MAX_RETRIES"])
                    {
                        agent.LogDebug(String.Format("Giving up to try to Communication with {0}. Will try another agent ...", otherAgent));
                        //engageOtherAgent();
                        state = ActionState.INACTIVE;
                    }
                    else
                    {
                        retry_cnter++;
                        otherAgent.Interact(agent, this);
                        agent.navagent.destination = otherAgent.transform.position;
                        agent.LogDebug(String.Format("Trying again {0} to Communication with {1}", retry_cnter, otherAgent));
                    }
                }
                return true;
            case ActionState.ACTION:
                if ((otherAgent.Desire is Communication) || (otherAgent.currentAction is Communication))
                {
                    agent.LogDebug(String.Format("Still Communicationting with {0} ...", otherAgent));
                    return true;
                }
                else
                {
                    // The other left; Execution will return false
                    agent.LogDebug(String.Format("Other agent {0} has left the Communication ...", otherAgent));
                    otherAgent = null;
                    state = ActionState.INACTIVE;
                    return false;
                }
        }

        return false;
    }

    // High values of extroversion and low values of energy increase the score
    public override double rate()
    {
        //double score = CalculateScore(agent.personality.extraversion, 0.5, ExpDecay(agent.motivation), 0.25, ExpGrowth(agent.happiness), 0.25);
        double score = CalculateScore(agent.personality.extraversion, config["PERSONALITY_WEIGHT"], ExpDecay(agent.motivation), config["MOTIVATION_WEIGHT"], ExpGrowth(agent.happiness), config["HAPPINESS_WEIGHT"]);
        return score;
    }

    public override bool execute()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                if (engageOtherAgent())
                    state = ActionState.MOVING;
                return true;

            case ActionState.MOVING:
                {
                    (double energy, double happiness) = calculateTransitionEffect();
                    agent.motivation = energy;
                    agent.happiness = happiness;
                    return true;
                }

            case ActionState.AWAITING:
                {
                    (double energy, double happiness) = calculateWaitingEffect();
                    agent.motivation = energy;
                    agent.happiness = happiness;
                    return true;
                }

            case ActionState.ACTION:
                agent.motivation = boundValue(0.0, agent.motivation + config["MOTIVATION_INCREASE"], 1.0);
                agent.happiness = boundValue(0.0, agent.happiness + config["HAPPINESS_INCREASE"], 1.0);
                agent.navagent.destination = otherAgent.transform.position;
                return true;
        }
        return false;
    }

    // Find another agent to Communication with
    private bool engageOtherAgent()
    {
        // Reset retry counter for all conditions
        retry_cnter = 0;

        if (agent.classroom.agents.Length == 1)
        {
            agent.LogDebug(String.Format("No other Agent to Communication with!"));
            return false;
        }

        // Select a random other agent
        int idx;
        do
        {
            idx = agent.random.Next(agent.classroom.agents.Length);
            otherAgent = agent.classroom.agents[idx];

            // Dont try to Communication with agents that are quarreling
            if (otherAgent.currentAction is Disagreement)
                continue;
        } while (otherAgent == agent);

        agent.LogDebug(String.Format("Agent tries to Communication with agent {0}!", otherAgent));
        if (otherAgent.currentAction is Communication)
        {
            agent.LogDebug(String.Format($"{otherAgent} is already Communicationting, join Communication!"));
        }
        else
        {
            otherAgent.Interact(agent, this);
        }
        agent.navagent.destination = otherAgent.transform.position;

        return true;
    }

    public override void end()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                // It can happen if the other one left the Communication, and than we end Communication
                break;

            case ActionState.MOVING:
                // It can happen if the other one left the Communication, and than we end Communication
                break;

            case ActionState.AWAITING:
                agent.LogDebug($"Giving up to wait for {otherAgent}!");
                break;

            case ActionState.ACTION:
                agent.LogDebug($"Ending Communicationting with {otherAgent}!");
                break;
        }
        retry_cnter = 0;
        otherAgent = null;
        state = ActionState.INACTIVE;
    }

    public void acceptInviation(Agent otherAgent)
    {
        agent.LogDebug(String.Format("{0} is accepting invitation to Communication with {1}!", agent, otherAgent));
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
                return String.Format($"{name}({state}) walking to meet {otherAgent.studentname}");
            case ActionState.AWAITING:
                return String.Format($"{name}({state}) waiting to interact with {otherAgent.studentname}");
            case ActionState.ACTION:
                return String.Format($"{name}({state}) actively involving in a conversation with {otherAgent.studentname}");
        }
        return "Invalid State!";
    }
}
