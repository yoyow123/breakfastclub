﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class StudyGroup : AgentBehavior
{

    /*
    • requirements: no quarrel, free spot at shared table, another learning students at the table, attention
    • effects: learning in group, reduces energy every turn, increase noise slightly,
    */
    private const float NOISE_INC = 0.0f;
    private const float HAPPINESS_INCREASE = 0.00f;
    private const float ENERGY_INCREASE = -0.01f;
    private const float NOISE_SCALE = 2.0f;

    private const float ENERGY_THRESHOLD = 0.5f; // As of when an Agent will start Learning
    private const float SCORE_SCALE = 100.0f;
    private const float EXTRAVERSION_WEIGHT = 0.3f;

    private Table lastTable;
    private Vector3 destination;

    private const int RETRY_THRESHOLD = 3;
    private int retry_cnter;

    public StudyGroup(Agent agent) : base(agent, AgentBehavior.Actions.StudyGroup, "StudyGroup", NOISE_INC) { }
    /*
     *  • requirements: no quarrel, free individual table, attention
     *  • effects: learning, reduces energy every turn
    */
    public override bool possible()
    {

        /*
        switch (state)
        {
            case ActionState.INACTIVE:
                break;
            case ActionState.WAITING:
                break;
            case ActionState.EXECUTING:
                break;
        }      
         */

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

                    state = ActionState.WAITING;
                    retry_cnter = 0;
                    //agent.LogDebug(String.Format("Got a table {0}!", lastTable));
                    return true;
                }
                return false;

            case ActionState.WAITING:
                if (table_ready())
                {
                    state = ActionState.EXECUTING;
                }
                else
                {
                    agent.navagent.destination = destination;
                    retry_cnter++;
                    agent.LogDebug(String.Format("Table not ready, waiting for {0} turns!", retry_cnter));
                }
                return true;
            case ActionState.EXECUTING:
                return table_ready();
        }
        return false;
    }

    private bool table_ready()
    {
        agent.LogDebug("Check if there are still other agents on the table ...");
        // So we sit on the table do we have someone to study with?
        List<Agent> others = lastTable.getOtherAgents(agent);
        foreach (Agent other in others)
        {
            if (other.Desire is StudyGroup)
            {
                if (agent.classroom.noise >= agent.personality.conscientousness * NOISE_SCALE)
                {
                    agent.LogInfo(String.Format("Cant learn its too noisy {0} > {1}", agent.classroom.noise, agent.personality.conscientousness * NOISE_SCALE));
                    state = ActionState.WAITING;
                    return false;
                }
                agent.LogDebug(String.Format("Found other agent {0} on table!", other));
                return true;
            }
        }
        agent.LogDebug(String.Format("Could not find anyone at the table!"));
        state = ActionState.WAITING;
        return false;
    }

    public override int rate()
    {
        // The score is defined by the vale of extraversion and the energy of the agent
        // Low values of extraversion and low values of energy increase the score (make this action more likely)

        // Agents low on extraversion prefare break (over chat)
        float extra = agent.personality.extraversion;
        float energy = boundValue(0.0f, agent.energy - ENERGY_THRESHOLD, 1.0f);
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
                agent.LogInfo(String.Format("Agent takes seat on table {0}", table));
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
                agent.LogError(String.Format("This should not happen!"));
                throw new NotImplementedException();

            case ActionState.WAITING:
                agent.LogDebug(String.Format("Stopping to wait for a study group at {0}!", lastTable));
                lastTable.releaseSeat(agent);
                lastTable = null;
                break;

            case ActionState.EXECUTING:
                agent.LogDebug(String.Format("Stop studying at {0}!", lastTable));
                lastTable.releaseSeat(agent);
                lastTable = null; 
                break;
        }
        state = ActionState.INACTIVE;
        retry_cnter = 0;
    }

    public override string ToString()
    {
        switch (state)
        {
            case ActionState.INACTIVE:
                return String.Format("{0} ({1})", name, state);
            case ActionState.WAITING:
                return String.Format("{0} ({1}) waiting at {2} to study with someone for {3} turns", name, state, lastTable, retry_cnter);
            case ActionState.EXECUTING:
                return String.Format("{0} ({1}) studying at {2} with {3} others", name, state, lastTable, lastTable.nAgents()-1);
        }
        return "Invalid State!";
    }
}
