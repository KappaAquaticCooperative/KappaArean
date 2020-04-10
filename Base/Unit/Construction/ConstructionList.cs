using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionList : ClientSingletion<ConstructionList> {
	public List<ConstructionProperty> Constructions = new List<ConstructionProperty>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public GameObject GetConstructionsByID (int ID){
		foreach (ConstructionProperty pro in Constructions) {
			if (pro.ID == ID) {
				return pro.Construction;
			}
		}

		return null;
	}
}

[System.Serializable]
public class ConstructionProperty {
	public int ID;
	public GameObject Construction;
}
