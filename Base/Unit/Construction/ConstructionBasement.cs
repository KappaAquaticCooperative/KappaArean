using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionBasement : Unit,IVehicle {
	public Unit Initializator;//谁造的建筑
	public override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		
	}

	public void Repair (float RepairHP) {
		ClientManager.Instance.HealingRequest (this, RepairHP);
	}

//	public void InitializatingBuilding (int ID) {
//		Building = GameObject.Instantiate(ConstructionList.Instance.GetConstructionsByID (1),transform).GetComponent<ConstructionBasement>();
//	}
}
