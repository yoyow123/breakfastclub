using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Friend {
	public string name;
	public string gptName;
	public ActionCount actionCount;
	public int count = 0;

	public Friend() { }

	public Friend(string name, string gptName, ActionCount actionCount) {
		this.name = name;
		this.gptName = gptName;
		this.actionCount = actionCount;
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
