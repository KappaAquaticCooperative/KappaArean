using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KappaSolider : InfantryBasement {

	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
		if(!IsMoving)
			anim.SetInteger ("speed",0);
		base.Update ();
	}

	protected override void MoveToTarget () {
		base.MoveToTarget ();
		if (IsMoving)
			anim.SetInteger ("speed",2);
	}
}
