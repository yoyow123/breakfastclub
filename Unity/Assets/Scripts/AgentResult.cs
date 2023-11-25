using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Friend {
	public Agent agent;
	public string name;
	public ActionCount actionCount;
	public int count=0;

	public Friend() { }

	public Friend(Agent agent, ActionCount actionCount, int count) {
		this.agent = agent;
		this.name = agent.personality.name;
		this.actionCount = actionCount;
		this.count = count;
	}
}
[Serializable]
public class AgentResult
{
	public string name;
	public List<Friend> friends = new List<Friend>();

	public AgentResult() { }

	public AgentResult(string name, List<Friend> friends) {
		this.name = name;
		this.friends = friends;
	}
}
