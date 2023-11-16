using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTime : AgentBehavior
{
    private Agent otherAgent;
    public InteractionTime(Agent agent) : base(agent, AgentBehavior.Actions.InteractionTime, "InteractionTime", agent.SC.InteractionTime) { }
    /*
     *  • requirements: no quarrel, free individual table, attention
     *  • effects: learning, reduces energy every turn
    */
    public override bool possible()
    {
        switch (state)
        {
            // Start to engage another agent
            case ActionState.INACTIVE:

                if (engageOtherAgent())
                {
                    state = ActionState.MOVING;
                    retry_cnter = 0;
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

            case ActionState.AWAITING:
                if ((otherAgent.Desire is InteractionTime) || (otherAgent.currentAction is InteractionTime))
                {
                    agent.LogDebug($"{otherAgent} is ready, just join!");
                    state = ActionState.ACTION;
                }
                else
                {
                    // We have someone we want to Disagreement with but they have not responded 'yet', so try to convince them
                    //if (retry_cnter >= (int)(config["MAX_RETRIES"]* agent.personality.conscientousness))
                    if (retry_cnter >= (int)config["MAX_RETRIES"])
                    {
                        agent.LogDebug(String.Format("Giving up to interact with {0}. Will try another agent ...", otherAgent));
                        //engageOtherAgent();
                        state = ActionState.INACTIVE;
                    }
                    else
                    {
                        retry_cnter++;
                        otherAgent.Interact(agent, this);
                        agent.navagent.destination = otherAgent.transform.position;
                        agent.LogDebug(String.Format("Trying again {0} to interact with {1}", retry_cnter, otherAgent));
                    }
                }
                return true;
            case ActionState.ACTION:
                if ((otherAgent.Desire is InteractionTime) || (otherAgent.currentAction is InteractionTime))
                {
                    agent.LogDebug($"Continou to Interacte with {otherAgent} ...");
                    return true;
                }
                else
                {
                    // The other left; Execution will return false
                    agent.LogDebug(String.Format("Other agent {0} has left the Interaction time ...", otherAgent));
                    otherAgent = null;
                    state = ActionState.INACTIVE;
                    return false;
                }
                return true;
        }
        return false;
    }

/*    private bool table_ready()
    {
        //agent.LogDebug("Check if there are still other agents on the table ...");
        // So we sit on the table do we have someone to study with?
        List<Agent> others = lastTable.getOtherAgents(agent);
        foreach (Agent other in others)
        {
            if (other.currentAction is InteractionTime)
            {
                if((other.currentAction.state is ActionState.AWAITING) || (other.currentAction.state is ActionState.ACTION))
                {
                    agent.LogDebug(String.Format("Found at least one other agent {0} on table!", other));
                    return true;
                }
            }
        }
        agent.LogDebug(String.Format("Could not find anyone at the table ready to study!"));
        state = ActionState.AWAITING;
        return false;
    }*/

    public override double rate()
    {
        double score = CalculateScore(agent.personality.extraversion, config["PERSONALITY_WEIGHT"], ExpGrowth(agent.motivation), config["MOTIVATION_WEIGHT"], ExpGrowth(agent.happiness), config["HAPPINESS_WEIGHT"]);
        return score;
    }

    public override bool execute()
    {
        switch (state)
        {
            case ActionState.INACTIVE:

                agent.LogError(String.Format("Trying to find someone to interact with!"));
                if (engageOtherAgent())
                {
                    state = ActionState.MOVING;
                }
                return true;

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
                agent.LogDebug($"Continue to Interact with {otherAgent} ...");
                agent.motivation = boundValue(0.0, agent.motivation + config["MOTIVATION_INCREASE"], 1.0);
                agent.happiness = boundValue(0.0, agent.happiness + config["HAPPINESS_INCREASE"], 1.0);
                agent.navagent.destination = otherAgent.transform.position;
                return true;
        }
        return false;
    }


    private bool freeTableAvailable()
    {
        foreach (Table table in agent.classroom.groupTables)
        {
            if (table.freeSpot())
            {
                return true;
            }
        }
        return false;
    }

    // Find a group Table, prefare tables with other agents
 /*   private (Table, Transform) getTable()
    {

        (Table table, Transform seat)  = _getTable(true);
        if (table)
            return (table, seat);
        return _getTable(false);
    }

    // Find a grouop Table
    private (Table, Transform) _getTable(bool hasAgents)
    {

        List<int> indices = GetPermutedIndices(agent.classroom.groupTables.Length);
        foreach (int idx in indices)
        {
            Table table = agent.classroom.groupTables[idx];

            if (hasAgents && table.nAgents() == 0)
                continue;

            Transform seat = table.takeSeat(agent);
            if (seat != null)
            {
                agent.LogDebug(String.Format("Agent takes seat on table {0}", table));
                lastTable = table;
                return (table, seat);
            }
        }
        return (null, null);
    }*/

    public override void end()
    {

        switch (state)
        {
            case ActionState.INACTIVE:
                //agent.LogError(String.Format("This should not happen!"));
                //throw new NotImplementedException();
                break;

            case ActionState.MOVING:
                agent.LogDebug(String.Format("Stopping before reaching the Table!"));
                break;

            case ActionState.AWAITING:
                agent.LogDebug(String.Format("Stopping to wait for a study group at {0}!", otherAgent));
                break;

            case ActionState.ACTION:
                agent.LogDebug(String.Format("Stop studying at {0}!", otherAgent));
                break;
        }

        state = ActionState.INACTIVE;
        retry_cnter = 0;
    }

    private bool engageOtherAgent()
    {
        // Reset retry counter for all conditions
        retry_cnter = 0;

        if (agent.classroom.agents.Length == 1)
        {
            agent.LogDebug(String.Format("No other Agent to interact with!"));
            return false;
        }

        // Select a random other agent
        int idx;
        do
        {
            idx = agent.random.Next(agent.classroom.agents.Length);
            otherAgent = agent.classroom.agents[idx];
        } while (otherAgent == agent);

        agent.LogDebug(String.Format("Agent tries to Interact with agent {0}!", otherAgent));
        otherAgent.Interact(agent, this);
        agent.navagent.destination = otherAgent.transform.position;

        return true;
    }
    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format($"{name}({state})");
            case ActionState.MOVING:
                return String.Format($"{name}({state}) moving towards {otherAgent.studentname} to join a lively social group");
            case ActionState.AWAITING:
                return String.Format($"{name}({state}) awaiting for creating social simulation circles ");
            case ActionState.ACTION:
                return String.Format($"{name}({state}) actively forming social alliances at with {otherAgent.studentname}");
        }
        return "Invalid State!";
    }
}
