﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTime : AgentBehavior
{
    private Table lastTable;
    private Vector3 destination;
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

                // Get a new table
                (Table table, Transform seat) = getTable();
                if (table != null)
                {
                    lastTable = table;
                    destination = seat.position;
                    agent.navagent.destination = destination;
                    //Debug.Log(String.Format("Taking seat at {0}", seat.position));
                    state = ActionState.MOVING;
                    retry_cnter = 0;
                    transition_cnter = 2;
                    agent.LogDebug(String.Format("Got a hub {0}!", lastTable.name));
                    return true;
                }
                else
                {
                    agent.LogDebug(String.Format("Unable to get a hub!"));
                    return false;
                }
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
                if (table_ready())
                {
                    state = ActionState.ACTION;
                }
                else
                {
                    //agent.navagent.destination = destination;
                    retry_cnter++;

                    // If we waited long enough, thats it, stop trying and stop studying
                    //if(retry_cnter > (int)(config["MAX_RETRIES"]* agent.personality.conscientousness))
                    if (retry_cnter > (int)(config["MAX_RETRIES"]))
                    {
                        agent.LogDebug(String.Format("Fed up with waiting will stop trying!"));
                        agent.LogDebug(String.Format("Fed up with AWAITING will stop trying!"));
                        state = ActionState.INACTIVE;
                        return false;
                    }
                    agent.LogDebug(String.Format("Table not ready. Waiting for {0} turns!", retry_cnter));
                    agent.LogDebug(String.Format("Table not ready. AWAITING for {0} turns!", retry_cnter));
                }
                return true;
            case ActionState.ACTION:
                if (table_ready())
                {
                    if (agent.classroom.noise >= agent.personality.conscientousness * config["NOISE_THRESHOLD"])
                    {
                        agent.LogDebug(String.Format("Cant learn its too noisy {0} > {1}", agent.classroom.noise, agent.personality.conscientousness * config["NOISE_THRESHOLD"]));
                        state = ActionState.AWAITING;
                        //return false;
                    }
                    //return true;
                }
                else
                {
                    // If table not ready wait
                    state = ActionState.AWAITING;
                }
                return true;
        }
        return false;
    }

    private bool table_ready()
    {
        //agent.LogDebug("Check if there are still other agents on the table ...");
        // So we sit on the table do we have someone to study with?
        List<Agent> others = lastTable.getOtherAgents(agent);
        foreach (Agent other in others)
        {
            if (other.currentAction is InteractionTime)
            {
                if ((other.currentAction.state is ActionState.AWAITING) || (other.currentAction.state is ActionState.ACTION))
                {
                    agent.LogDebug(String.Format("Found at least one other agent {0} on hub!", other));
                    otherAgent = other;
                    return true;
                }
            }
        }
        agent.LogDebug(String.Format("Could not find anyone at the table ready to study!"));
        state = ActionState.AWAITING;
        return false;
    }

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
                agent.LogError(String.Format("This should not happen!"));
                throw new NotImplementedException();
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

                agent.navagent.destination = destination;
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
    private (Table, Transform) getTable()
    {

        (Table table, Transform seat) = _getTable(true);
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
                agent.LogDebug(String.Format("Agent takes seat on hub {0}", table.name));
                lastTable = table;
                return (table, seat);
            }
        }
        return (null, null);
    }

    public override void end()
    {

        switch (state)
        {
            case ActionState.INACTIVE:
                //agent.LogError(String.Format("This should not happen!"));
                //throw new NotImplementedException();
                break;

            case ActionState.MOVING:
                agent.LogDebug(String.Format("Stopping before reaching the Hub!"));
                break;

            case ActionState.AWAITING:
                agent.LogDebug(String.Format("Stopping to wait for a social group at {0}!", lastTable.name));
                break;

            case ActionState.ACTION:
                agent.LogDebug(String.Format("Stop interacting at {0}!", lastTable.name));

                agent.AddFriends(otherAgent);


                break;
        }
        if (lastTable)
        {
            lastTable.releaseSeat(agent);
            lastTable = null;
        }

        state = ActionState.INACTIVE;
        retry_cnter = 0;
    }

    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format($"{name}({state})");
            case ActionState.MOVING:
                return String.Format($"{name}({state}) moving towards {lastTable} to join a lively social group");
            case ActionState.AWAITING:
                return String.Format($"{name}({state})  awaiting at {lastTable} for creating social simulation circles ");
            case ActionState.ACTION:
                return String.Format($"{name}({state}) actively forming social alliances at {lastTable} with {lastTable.nAgents() - 1} others");
        }
        return "Invalid State!";
    }
}
