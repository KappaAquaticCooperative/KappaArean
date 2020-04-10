using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFactionManager : ClientSingletion<GameFactionManager> {
	public List<FactionCollection> Factions = new List<FactionCollection>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	/// <summary>
	/// 检查阵营.
	/// </summary>
	/// <returns><c>true</c>, 当目标为敌对时返回true, <c>false</c> 否则返回false.</returns>
	/// <param name="我的阵营">My faction.</param>
	/// <param name="目标阵营">Target faction.</param>
	public bool CheckFaction (int MyID,int TargetID){
		foreach (FactionCollection collection in Factions) {
			if (collection.MyID == MyID) {
				if (collection.EnemyID.Contains (TargetID))
					return true;
			}
		}
		return false;
	}

	[System.Serializable]
	public class FactionCollection {
		public int MyID;
		public List<int> AlliesID = new List<int> ();
		public List<int> EnemyID = new List<int>();
	}
}
