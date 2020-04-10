using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryWeapon : InventoryBasement {
	// Use this for initialization
	void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
	}

	public override void MouseLeftDown ()
	{
		base.MouseLeftDown ();

		Transform point = Camera.main.transform;
		Vector3 AttackVector = Vector3.zero;
		RaycastHit hitinfo;
		if(Physics.Raycast (point.position,point.forward,out hitinfo)){
			AttackVector = hitinfo.point;
		} else {
			AttackVector = point.forward*1000+point.position;
		}
		if (dev.Property.Action == WeaponAction.Semi)
			dev.DoSemiRoot (AttackVector, User, OnClickCallback,"0");
		
		base.MouseLeftDown ();
	}

	public override void MouseLeftClicking ()
	{
		if (dev.Property.Action == WeaponAction.Semi)
			return;
		base.MouseLeftClicking ();

		Transform point = Camera.main.transform;
		Vector3 AttackVector = Vector3.zero;
		RaycastHit hitinfo;
		if(Physics.Raycast (point.position,point.forward,out hitinfo)){
			AttackVector = hitinfo.point;
		} else {
			AttackVector = point.forward*1000+point.position;
		}

		dev.DoFiringRoot (AttackVector, User, OnClickCallback,"0");
	}

	public override void ApplyCombo (string combo, Vector3 StartPos, Vector3 TargetPos) {
		base.ApplyCombo (combo, StartPos, TargetPos);
		switch (combo) {
		case "0":
			dev.FireOneShoot (TargetPos, User, StartPos);
			break;
		}
	}

	public override void OnReloadButtonDown () {
		dev.ForceReload = true;
		anim.SetTrigger ("reload");
	}
}
