using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionCount 
{
	public int restCount;
	public int communicationCount;
	public int disagreementCount;
	public int soloCount;
	public int interactionCount;

	public ActionCount() { }
	public ActionCount(int restCount, int communicationCount, int disagreementCount,	
									int soloCount, int interactionCount) {
		this.restCount = restCount;
		this.communicationCount = communicationCount;
		this.disagreementCount = disagreementCount;
		this.soloCount = soloCount;
		this.interactionCount = interactionCount;
	}


}

