using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AgentResult
{
	public string name;
	public string gptName;
	public ActionCount actionCount;

	public AgentResult() { }

	public AgentResult(string name, string gptName, ActionCount actionCount) {
		this.name = name;
		this.gptName = gptName;
		this.actionCount = actionCount;
	}
}
