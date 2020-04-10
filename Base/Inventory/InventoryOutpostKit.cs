using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryOutpostKit : InventoryWeapon {
	public int Buildings;
	// Use this for initialization
	void Start () {
		base.Start ();
	}

	// Update is called once per frame
	void Update () {
		base.Update ();
	}

	public override void MouseLeftClicking ()
	{
		
	}

	public override void MouseRightDown () {
		base.MouseRightDown ();
		if (dev.Property.AmmoInClip > 0) {
			RaycastHit hitinfo;
			if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out hitinfo, 5) && IsPlayerWeapon) {
				ClientManager.Instance.PutoutSentryPostRequest (hitinfo.point, Buildings);
				dev.Property.AmmoInClip -= 1;
			}
		}
	}
}
