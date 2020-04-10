using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBasement : ConstructionBasement {
	[Header("火炮基础")]
	public Transform Turret;
	public Transform GunRot;

	[Header("火炮数值")]
	public float AttackRange = 100;
	public float TurnningSpeed = 90;
	public float MaxUpAngle;
	public float MaxDownAngle;

	protected bool TurretSet = false;
	protected bool GunSet = false;
	protected Vector3 FocusingPoint = Vector3.zero;
	public override void Start () {
		base.Start ();
	}

	// Update is called once per frame
	protected override void Update () {
		if(FocusingPoint != Vector3.zero)
			Aiming (FocusingPoint);
	}

	public void FocusPosition (Vector3 Target) {
		FocusingPoint = Target;
	}

	void Aiming (Vector3 Position) {
		RaycastHit[] hitinfo;

		Vector3 dir = Position - transform.position;
		Vector3 euler = Quaternion.LookRotation (dir).eulerAngles;
		Vector3 eulerGun = Vector3.zero;
		if (GunRot != null) {
			eulerGun = Quaternion.LookRotation (Position - GunRot.position).eulerAngles;
		}

		float RotatingSpeed = TurnningSpeed * Time.fixedDeltaTime;
		//开始控制方向
		if (Turret) {
			Turret.localEulerAngles = new Vector3 (0, Turret.localEulerAngles.y, 0);
			float TurrAnglesDiff = euler.y - Turret.eulerAngles.y;
			if (TurrAnglesDiff < 0)
				TurrAnglesDiff += 360;
			if (TurrAnglesDiff >= 180 && RotatingSpeed <= 360 - TurrAnglesDiff) {
				//左转
				Turret.Rotate (0, -RotatingSpeed, 0);
				TurretSet = false;
			} else if (TurrAnglesDiff < 180 && RotatingSpeed <= TurrAnglesDiff) {
				//右转
				Turret.Rotate (0, RotatingSpeed, 0);
				TurretSet = false;
			}

			if (TurrAnglesDiff <= RotatingSpeed || 360 - TurrAnglesDiff <= RotatingSpeed) {
				Turret.eulerAngles = new Vector3 (Turret.eulerAngles.x, euler.y, Turret.eulerAngles.z);
				TurretSet = true;
			}
		} else {
			TurretSet = true;
		}

		if (GunRot) {
			float AimingAngle = 0;
			float CheckAngle = 0;
			float CheckGunAngle = 0;
			//说明:父物体全局欧拉X角有时明明是-1度角结果返回的值是359度,和Clamp结合使用会造成很可怕的结果,这是UNITY欧拉角的特色,不得不品尝
			if (Turret.eulerAngles.x > 180) {
				CheckAngle = Turret.eulerAngles.x - 360;
			} else {
				CheckAngle = Turret.eulerAngles.x;
			}
			if (GunRot.eulerAngles.x > 180) {
				CheckGunAngle = GunRot.eulerAngles.x - 360;
			} else {
				CheckGunAngle = GunRot.eulerAngles.x;
			}

			if (eulerGun.x > 180 ) {
				AimingAngle = eulerGun.x - 360;//往上抬
			} else if (eulerGun.x < 180) {
				AimingAngle = eulerGun.x;//往下降
			}
			AimingAngle = Mathf.Clamp (AimingAngle, -MaxUpAngle + CheckAngle, -MaxDownAngle + CheckAngle);
			float GunAnglesDiff = AimingAngle - CheckGunAngle;
			if (GunAnglesDiff >= 0 && RotatingSpeed < GunAnglesDiff ) {//下扬
				GunRot.Rotate (RotatingSpeed, 0, 0);
			} else if (GunAnglesDiff < 0 &&  RotatingSpeed < -GunAnglesDiff) {//上抬
				GunRot.Rotate (-RotatingSpeed, 0, 0);
			} else  {
				GunSet = true;
			}

		} else {
			GunSet = true;
		}
	}

	public void Firing (Vector3 TargetPoint) {
		if (GunSet && TurretSet) {
			foreach (WeaponDevelopment dev in Weapons) {
				dev.DoFiringRoot (TargetPoint, this, FiringCallBack,"0");
			}
		} else {
			
		}
	}

	public override void Attack (Vector3 TargetPosition, Vector3 StartPosition) {
		foreach (WeaponDevelopment dev in Weapons) {
			dev.FireOneShoot (TargetPosition, this, StartPosition);
		}
	}

	void FiringCallBack (Vector3 TargetPosition,Vector3 StartPoint,string combo){
		ClientManager.Instance.BuildingAttackRequest (this, TargetPosition, StartPoint);

		try {
			float distance = Vector3.Distance (transform.position, NetworkInterface.Instance.Player.transform.position);
			if (distance < 10) {
				NetworkInterface.Instance.Player.GetComponent<PlayerBase>().Shaking();
			}
		} catch {

		}
	}
}
