using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CIWSDevelopment : CannonBasement {
	public bool CanJudging = true;
	private Unit Enemy;
	public override void Start () {
		base.Start ();
		if (NetworkInterface.Instance.IsServer) {
			if (CanJudging)
				StartCoroutine (Judging ());
		}
	}

	protected override void Update () {
		base.Update ();

		if (Enemy) {
			ClientManager.Instance.BuildingFocusRequest (this,Enemy.transform.position);
			Firing (Enemy.transform.position);
		} else {
			GunSet = false;
			TurretSet = false;
		}
	}

	IEnumerator Judging () {
		Debug.Log ("Show!");
		while (CanJudging) {
			Collider[] colliders = Physics.OverlapSphere (transform.position, AttackRange);
			foreach (Collider c in colliders) {
				try {
					if (GameFactionManager.Instance.CheckFaction (States.FactionID, c.GetComponent<Unit> ().States.FactionID) && Enemy == null) {
						Enemy = c.GetComponent<Unit>();
					} else if (Enemy != null) {
						break;
					}
				} catch {

				}
			}

			yield return new WaitForSeconds(0.25f);
		}
	}
}
