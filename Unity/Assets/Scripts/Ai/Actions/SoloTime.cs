using System;
using UnityEngine;
using System.Collections.Generic;

public class SoloTime : AgentBehavior
{
    private Table lastTable;
    private Vector3 destination;

    public SoloTime(Agent agent) : base(agent, AgentBehavior.Actions.SoloTime, "SoloTime", agent.SC.SoloTime) { }
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
                    state = ActionState.MOVING;
                    transition_cnter = 2;
                    return true;
                }
                else
                {
                    agent.LogDebug("No free single table to study!");
                    return false;
                }

            case ActionState.MOVING:
                transition_cnter--;
                if (transition_cnter > 0)
                {

                }
                else
                {
                    state = ActionState.ACTION;
                }
                return true;

            case ActionState.AWAITING:
                retry_cnter++;
                if (retry_cnter > (int)(config["MAX_RETRIES"]))
                {
                    agent.LogDebug(String.Format("Fed up with waiting will stop trying!"));
                    state = ActionState.INACTIVE;
                    return false;
                }
                state = ActionState.ACTION;
                return true;
            case ActionState.ACTION:
                if (agent.classroom.noise >= agent.personality.conscientousness * config["NOISE_THRESHOLD"])
                {
                    agent.LogDebug($"Its too loud! Cannot learn! {agent.classroom.noise} > {agent.personality.conscientousness * config["NOISE_THRESHOLD"]}. Will Wait!");
                    state = ActionState.AWAITING;
                }
                else
                {
                    agent.LogDebug($"Continou to study alone!");
                }
                return true;
        }
        return false;
    }

    public override double rate()
    {
        double score = CalculateScore(1.0 - agent.personality.extraversion, config["PERSONALITY_WEIGHT"], ExpGrowth(agent.motivation), config["MOTIVATION_WEIGHT"], ExpGrowth(agent.happiness), config["HAPPINESS_WEIGHT"]);
        return score;
    }

    public override bool execute()
    {

        switch (state)
        {
            case ActionState.INACTIVE:
                agent.LogError(String.Format("This should not happen!"));
                //throw new NotImplementedException();
                return false;

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
        foreach (Table table in agent.classroom.individualTables)
        {
            if (table.freeSpot())
            {
                return true;
            }
        }
        return false;
    }

    // Find a free Table and 
    private (Table, Transform) getTable()
    {
        List<int> indices = GetPermutedIndices(agent.classroom.individualTables.Length);
        foreach (int idx in indices)
        {
            Table table = agent.classroom.individualTables[idx];
            Transform seat = table.takeSeat(agent);
            if (seat != null)
            {
                //Debug.Log(String.Format("Getting table {0}", idx));
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
            case ActionState.MOVING:
                agent.LogDebug(String.Format("Stopping before reaching the hub!"));
                break;

            case ActionState.AWAITING:
                agent.LogError(String.Format("Waiting in order to study alone!"));
                break;

            case ActionState.ACTION:
                agent.LogDebug(String.Format("Ending study alone on hub {0}!", lastTable.name));
                break;
        }
        if (lastTable)
        {
            lastTable.releaseSeat(agent);
            lastTable = null;
        }
        retry_cnter = 0;
        state = ActionState.INACTIVE;
    }

    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format($"{name}({state})");
            case ActionState.MOVING:
                return String.Format($"{name}({state}) walking towards {lastTable.name} for some solo time.");
            case ActionState.AWAITING:
                return String.Format($"{name}({state}) peacefully wandering at  {lastTable.name}");
            case ActionState.ACTION:
                return String.Format($"{name}({state}) immersing in solo activities at  {lastTable.name}");
        }
        return "Invalid State!";
    }
}
