using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Friend {
	public Agent agent;
	public string name;
	public string gptName;
	public ActionCount actionCount;
	public int count=0;

	public Friend() { }

	public Friend(Agent agent, ActionCount actionCount, int count) {
		this.agent = agent;
		this.name = agent.personality.name;
		this.gptName = agent.gptName;
		this.actionCount = actionCount;
		this.count = count;
	}
}
[Serializable]
public class AgentResult
{
	public string name;
	public string gptName;
	public ActionCount actionCount;
	public List<Friend> friends = new List<Friend>();

	public AgentResult() { }

	public AgentResult(string name, string gptName, ActionCount actionCount, List<Friend> friends) {
		this.name = name;
		this.gptName = gptName;
		this.actionCount = actionCount;
		this.friends = friends;
	}
}
