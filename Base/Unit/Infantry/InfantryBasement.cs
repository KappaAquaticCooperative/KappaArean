using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryBasement : Unit {
	public Transform Hand;//仅限于非Player的单位

	private InventoryBasement Gun;
	protected Unit InteractivingObject;
	// Use this for initialization
	public virtual void Start () {
		base.Start ();
		ChangingWeapon (0);
	}
	
	// Update is called once per frame
	protected void Update () {
		base.Update ();
	}

	public override void Attack (Vector3 TargetPosition, Vector3 StartPosition, string Combo) {
		Gun.ApplyCombo (Combo,StartPosition,TargetPosition);
		anim.SetTrigger ("attack");
	}
		
	public void ChangingWeapon (int WeaponSlots) {
		Debug.Log ("Changing!");
		if (Gun != null)
			Destroy (Gun.gameObject);

		States.CurrentWeaponID = States.AvaliableWeapons [WeaponSlots];
		GameObject go = GameObject.Instantiate (InventoryManagement.Instance.GetTPSWeaponByID (States.CurrentWeaponID), Hand);
		Gun = go.GetComponent<InventoryBasement> ();
		CurrentWeapon = Gun.dev;
		Gun.User = this;
	}

	public void InteractiveStart(Unit u){
		InteractivingObject = u.gameObject.GetComponent<IInteractable> ().OnInteractiveStart (this);
	}

	public void InteractiveExit(Unit u){
		InteractivingObject.GetComponent<IInteractable>().OnInteractiveExit (this);
		InteractivingObject = null;
	}
}
