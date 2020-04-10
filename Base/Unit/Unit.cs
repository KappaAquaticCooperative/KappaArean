using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Net;
using System;
using System.IO;

public class Unit : MonoBehaviour
{
	public UnitStates States;
	public bool IsMoving;

	public WeaponDevelopment[] Weapons;
	[HideInInspector]public WeaponDevelopment CurrentWeapon;
	private Vector3 MovingPos;
	private Quaternion MovingRot;
	[HideInInspector]
	public Animator anim;
    // Start is called before the first frame update
	public virtual void Awake()
    {
		anim = GetComponent<Animator> ();
    }

	public virtual void Start()
	{
		
	}

    // Update is called once per frame
	protected virtual void Update()
    {
		MoveToTarget ();
    }
		
	public virtual void TakeDamage (DamageProperty WeaponPro,Unit Attacker) {
		float Total = 0;
		//免疫伤害处理
		States.DefenseLevel.APPower -= WeaponPro.APPower;
		if (States.DefenseLevel.APPower > 0)
			return;
		if (States.DefenseLevel.ToxinPower != 0) {
			WeaponPro.SlashPower = 0;
		}

		//伤害叠加
		if (WeaponPro.KineticPower != 0) {
			Total += WeaponPro.KineticPower - States.DefenseLevel.KineticPower - States.DefenseLevel.KineticPower - States.DefenseLevel.ToxinPower;
			Total = Mathf.Clamp (Total, 0, Mathf.Infinity);
		}
		if (WeaponPro.ChemicalPower  != 0) {
			Total += WeaponPro.ChemicalPower - States.DefenseLevel.ChemicalPower - States.DefenseLevel.ToxinPower;
			Total = Mathf.Clamp (Total, 0, Mathf.Infinity);
		}
		if (WeaponPro.ToxinPower  != 0){
			Total += WeaponPro.ToxinPower - States.DefenseLevel.ChemicalPower;
			Total = Mathf.Clamp (Total, 0, Mathf.Infinity);
		}
		if (WeaponPro.SlashPower  != 0) {
			Total += WeaponPro.SlashPower - States.DefenseLevel.KineticPower;
			Total = Mathf.Clamp (Total, 0, Mathf.Infinity);
		}
		if (Attacker.tag == Tags.player)
			NetworkInterface.Instance.HitFeedback (this,Total);

		States.PerHP -= Total;
		if (ServerManager.Instance != null) {
			int id = ClientManager.Instance.GetNetworkIDByUnit (this);
			ServerManager.Instance.SynchronizingUnitStates (id, States);
			if (States.PerHP <= 0) {
				ServerManager.Instance.KillPlayerRequest (id);
			}
		}
	}

	private void OnParticleCollision(GameObject Object) {
		if (Object.tag == Tags.BulletFlames) {
			Bullet bullet = Object.GetComponent<Bullet> ();
			if (GameFactionManager.Instance.CheckFaction (States.FactionID, bullet.User.States.FactionID) && ServerManager.Instance != null) {
				ServerManager.Instance.DamageRequest (bullet.User, bullet.property, this);
			}
		}
	}

	public void Moving (Vector3 Position,Quaternion Rotation) {
		MovingPos = Position;
		MovingRot = Rotation;
		IsMoving = true;
	}

	public virtual void Attack(Vector3 TargetPosition,Vector3 StartPosition) {
		CurrentWeapon.FireOneShoot (TargetPosition,this,StartPosition);
		anim.SetTrigger ("attack");
	}

	public virtual void Attack(Vector3 TargetPosition,Vector3 StartPosition,string Combo) {
		CurrentWeapon.GetComponent<InventoryBasement> ().ApplyCombo (Combo, StartPosition, TargetPosition);
		anim.SetTrigger ("attack");
	}

	public virtual void Healing (float Amount,Unit Target){
		States.PerHP += Amount;
	}

	protected virtual void MoveToTarget () {
		if (IsMoving) {
			float distance = Vector3.Distance (transform.position, MovingPos);
			transform.position = Vector3.SmoothDamp (transform.position, MovingPos, ref States.Velocity, 0.1f);
			transform.rotation = Quaternion.Lerp (transform.rotation, MovingRot, 0.1f);
			if (distance > 0.1f) {

			} else {
				IsMoving = false;
			}
		}
	}

	public void Death () {
		Destroy (gameObject);
	}
}

[System.Serializable]
public class UnitStates
{
    //基本属性
	public int ID;
	public bool IsBoss = false;

	[Header("用于游戏进行时实时通讯")]
	public Vector3 Position;
	public Quaternion Rotation;

	public int FactionID;
    public int MaxLife;
    public float MaxHP;
	public int[] AvaliableWeapons;//需要在unitsmanagement里面注册
	public Dictionary<int,WeaponProperty> Weapons = new Dictionary<int, WeaponProperty>();
	public int[] OwnedBuilding;
	public int CurrentWeaponID;
	public DamageProperty DefenseLevel;

    //实时属性
	public Vector3 Velocity = new Vector3(1,0,0)*10;
    public int PerLife;
    public float PerHP;
}
