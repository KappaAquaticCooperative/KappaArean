using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryL01 : InventoryWeapon {

	// Use this for initialization
	void Start () {
		base.Start ();
	}

	// Update is called once per frame
	void Update () {
		base.Update ();
	}

//	public override void ApplyCombo (string combo)
//	{
//		base.ApplyCombo (combo);
//
//		switch (combo) {
//		case "1":
//			GrenadeLaunch ();
//			break;
//		case "e":
//			ShotGunLaunch ();
//			break;
//		}
//	}

	public override void MouseLeftClicking () {
		dev.ChangeBullet (1);
		base.MouseLeftClicking ();
	}

	public override void MouseRightDown () {
		GrenadeLaunch ();
	}

	public override void OnMeleeAttackDown () {
		ShotGunLaunch ();
	}

	public void GrenadeLaunch () {
		if (dev.Property.AmmoInClip >= 5) {
			dev.Property.AmmoInClip -= 4;
			dev.ChangeBullet (2);
			dev.ForceFiring (GetFocusingPosition(), User, OnClickCallback,"1");
		}
	}

	public void GrenadeLaunch (Vector3 TargetPosition,Vector3 StartPosition) {
		if (dev.Property.AmmoInClip >= 5) {
			dev.Property.AmmoInClip -= 4;
			dev.ChangeBullet (2);
			dev.FireOneShoot (TargetPosition,User,StartPosition);
		}
	}

	public void ShotGunLaunch () {
		if (dev.Property.AmmoInClip >= 5) {
			dev.ChangeBullet (3);
			float InitiateDispersion = dev.Property.BulletDispersion;

			dev.Property.BulletPerShot = 5;
			dev.Property.BulletDispersion = 1.5f;
			dev.Property.AmmoInClip -= 4;
			dev.ForceFiring (GetFocusingPosition (), User, OnClickCallback,"e");

			dev.Property.BulletPerShot = 1;
			dev.Property.BulletDispersion = InitiateDispersion;
		}
	}

	public void ShotGunLaunch (Vector3 TargetPosition,Vector3 StartPosition) {
		if (dev.Property.AmmoInClip >= 5) {
			dev.ChangeBullet (3);
			float InitiateDispersion = dev.Property.BulletDispersion;

			dev.Property.BulletPerShot = 5;
			dev.Property.BulletDispersion = 1.5f;
			dev.Property.AmmoInClip -= 4;
			dev.FireOneShoot (TargetPosition,User,StartPosition);

			dev.Property.BulletPerShot = 1;
			dev.Property.BulletDispersion = InitiateDispersion;
		}
	}

	private Vector3 GetFocusingPosition () {
		Transform point = Camera.main.transform;
		Vector3 AttackVector = Vector3.zero;
		RaycastHit hitinfo;
		if(Physics.Raycast (point.position,point.forward,out hitinfo)){
			AttackVector = hitinfo.point;
		} else {
			AttackVector = point.forward*1000+point.position;
		}

		return AttackVector;
	}

	/// <summary>
	/// 服务器通讯
	/// </summary>
	/// <param name="combo">Combo.</param>
	/// <param name="StartPos">Start position.</param>
	/// <param name="TargetPos">Target position.</param>
	public override void ApplyCombo (string combo, Vector3 StartPos, Vector3 TargetPos) {
		if (IsPlayerWeapon)
			return;
		Combo = combo;

		switch (combo) {
		case "e":
			ShotGunLaunch (TargetPos,StartPos);
			break;
		case "1":
			GrenadeLaunch (TargetPos,StartPos);
			break;
		case "0":
			dev.ChangeBullet (1);
			dev.FireOneShoot (TargetPos, User, StartPos);
			break;
		}
	}
}
