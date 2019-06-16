﻿using System;

public class Chat : AgentBehavior
{
    /*
    • requirements: free spot at shared table, other student at the table
    • effect: regenerate energy, increase noise, will increase happiness(amount is a function of extraversion)
    */

    private const float NOISE_INC = 0.1f;
    private const float HAPPINESS_INCREASE = 0.05f;
    private const float ENERGY_INCREASE = 0.05f;

    private const float ENERGY_BIAS = -0.4f; // Negative Values incourage work, positive to take a break
    private const float SCORE_SCALE = 100.0f;
    private const float EXTRAVERSION_WEIGHT = 0.3f;

    //private const int MISSING_PARTNER_COST = -30;

    private const int RETRY_THRESHOLD = 3;
    private int retry_cnter;

    private Agent otherAgent;

    public Chat(Agent agent) : base(agent, AgentBehavior.Actions.Chat, "Chat", NOISE_INC) { }

    // An Agent can chat if there is another Agent disponible
    public override bool possible()
    {

        switch (state)
        {
            // Start to engage another agent
            case ActionState.INACTIVE:
                if (engageOtherAgent())
                {
                    state = ActionState.WAITING;
                    return true;
                }
                return false;

            // Either Change to active if the other agent is responing, or try to interact again
            // If we tried long enough, change to another target.
            case ActionState.WAITING:
                if ((otherAgent.Desire is Chat) || (otherAgent.currentAction is Chat))
                {
                    agent.LogInfo(String.Format("Agent {0} is ready to chat, lets go ...", otherAgent));
                    state = ActionState.EXECUTING;
                }
                else
                {
                    // We have someone we want to quarrel with but they have not responded 'yet', so try to convince them
                    if (retry_cnter >= RETRY_THRESHOLD)
                    {
                        agent.LogInfo(String.Format("Giving up to try to chat with {0}. Will try another agent ...", otherAgent));
                        engageOtherAgent();
                    }
                    else
                    {
                        retry_cnter++;
                        otherAgent.Interact(agent, this);
                        agent.navagent.destination = otherAgent.transform.position;
                        agent.LogInfo(String.Format("Trying again {0} to chat with {1}", retry_cnter, otherAgent));
                    }
                }
                return true;
            case ActionState.EXECUTING:
                if ((otherAgent.Desire is Chat) || (otherAgent.currentAction is Chat))
                {
                    agent.LogInfo(String.Format("Still chatting with {0} ...", otherAgent));
                    return true;
                }
                else
                {
                    // The other left; Execution will return false
                    agent.LogInfo(String.Format("Other agent {0} has left the chat ...", otherAgent));
                    otherAgent = null;
                    state = ActionState.INACTIVE;
                }
                return false;
        }

        return false;
    }

    /*
    • requirements: free spot on individual table
    • effect: regenerate energy, will increase happiness(amount is a function of extraversion)
    */
    public override int rate()
    {
        // The score is defined by the vale of extraversion and the energy of the agent
        // High values of extraversion and low values of energy increase the score (make this action more likely)

        // Agents low on extraversion prefare break (over chat)
        float extra = agent.personality.extraversion;
        float energy = boundValue(0.0f, 1.0f + ENERGY_BIAS - agent.energy, 1.0f);
        float t = (extra * EXTRAVERSION_WEIGHT) + (energy * (1.0f - EXTRAVERSION_WEIGHT));

        int score = (int)(boundValue(0.0f, t, 1.0f) * SCORE_SCALE);

        return score;
    }

    public override bool execute()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                agent.LogError(String.Format("This should not happen!"));
                throw new NotImplementedException();

            case ActionState.WAITING:
                (float energy, float happiness) = calculateWaitingEffect();
                agent.energy = energy;
                agent.happiness = happiness;
                return true;

            case ActionState.EXECUTING:
                agent.energy = boundValue(0.0f, agent.energy + ENERGY_INCREASE, 1.0f);
                agent.happiness = boundValue(-1.0f, agent.happiness + HAPPINESS_INCREASE, 1.0f);
                agent.navagent.destination = otherAgent.transform.position;
                return true;
        }
        return false;
    }

    // Find another agent to chat with
    private bool engageOtherAgent()
    {
        // Reset retry counter for all conditions
        retry_cnter = 0;

        if (agent.classroom.agents.Length == 1)
        {
            agent.LogInfo(String.Format("No other Agent to chat with!"));
            return false;
        }

        // Select a random other agent
        int idx;
        do
        {
            idx = agent.random.Next(agent.classroom.agents.Length);
            otherAgent = agent.classroom.agents[idx];
        } while (otherAgent == agent);

        agent.LogInfo(String.Format("Agent tries to chat with agent {0}!", otherAgent));
        otherAgent.Interact(agent, this);
        agent.navagent.destination = otherAgent.transform.position;

        return true;
    }

    public override void end()
    {
        agent.LogInfo(String.Format("Stop chatting with {0}!", otherAgent));
        otherAgent = null;

        switch (state)
        {
            case ActionState.INACTIVE:
                agent.LogError(String.Format("This should not happen!"));
                throw new NotImplementedException();

            case ActionState.WAITING:
                agent.LogDebug(String.Format("Giving up to wait for {0}!", otherAgent));
                retry_cnter = 0;
                break;

            case ActionState.EXECUTING:
                agent.LogDebug(String.Format("Ending Chatting with {0}!", otherAgent));
                otherAgent = null;
                retry_cnter = 0;
                break;
        }
        state = ActionState.INACTIVE;
    }

    public void acceptInviation(Agent otherAgent)
    {
        agent.LogInfo(String.Format("{0} is accepting invitation to chat with {1}!", agent, otherAgent));
        this.otherAgent = otherAgent;
        state = ActionState.EXECUTING;
    }

    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format("{0} ({1})", name, state);
            case ActionState.WAITING:
                return String.Format("{0} ({1}) waiting for {2} retrying {3}", name, state, otherAgent.studentname, retry_cnter);
            case ActionState.EXECUTING:
                return String.Format("{0} ({1}) with {2}", name, state, otherAgent.studentname);
        }
        return "Invalid State!";
    }
}
