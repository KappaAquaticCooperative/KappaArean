using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KappaSapper : InfantryBasement {

	// Use this for initialization
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
