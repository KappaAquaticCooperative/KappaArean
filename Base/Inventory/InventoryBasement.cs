using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBasement : MonoBehaviour {
	public Transform CameraPosition;
	public bool IsPlayerWeapon = true;

	[HideInInspector]
	public Unit User;
	[HideInInspector]
	public WeaponDevelopment dev;
	protected string Combo;
	protected float ComboRestTimer = 1;
	protected Animator anim;
	// Use this for initialization
	protected void Awake () {
		dev = GetComponent<WeaponDevelopment> ();
		anim = GetComponent<Animator> ();
	}

	protected void Start () {
		User.CurrentWeapon = dev;
		try{
			User.anim.SetInteger ("mode", dev.Property.WeaponHandMode);
		}catch{
			Debug.Log ("无animator,怀疑是玩家角色");
		}
	}
	
	// Update is called once per frame
	protected void Update () {
		if (!IsPlayerWeapon)
			return;
		#region 键位判断
		bool ResetCombo = false;
		if (Input.GetButtonDown (InputmentManagement.MeleeButton)) {
			OnMeleeAttackDown ();
		} else if (Input.GetMouseButtonDown (InputmentManagement.ShootButton)) {
			MouseLeftDown ();
		} else if (Input.GetMouseButton (InputmentManagement.ShootButton)) {
			MouseLeftClicking ();
		} else if (Input.GetMouseButtonDown (InputmentManagement.AimingButton)) {
			MouseRightDown ();
		} else if (Input.GetButtonDown (InputmentManagement.SkillButton)) {
			OnSkillAttackDown ();
		} else if (Input.GetButtonDown (InputmentManagement.SpellCardButton)) {
			OnSpellCardAttackDown ();
		} else if (Input.GetButtonDown (InputmentManagement.ReloadButton)) {
			OnReloadButtonDown ();
		} else {
			ResetCombo = true;
		}

		if (ResetCombo) {
			ComboRestTimer -= Time.deltaTime;
			if(ComboRestTimer <= 0){
				ComboRestTimer = 1;
			}
		}else {
			ComboRestTimer = 1;
		}
		#endregion
	}

	public virtual void OnMeleeAttackDown () {//E键

	}

	public virtual void MouseLeftDown () {
		
	}

	public virtual void MouseLeftClicking () {
		
	}

	public virtual void OnClickCallback (Vector3 point,Vector3 StartPoint,string combo) {
		PopComboInputment (combo);
		ClientManager.Instance.AttackRequest (point,StartPoint,Combo);
		if (dev.Property.AmmoInClip != 0) {
			anim.SetTrigger ("attack");
		} else if(dev.Property.MaxAmmo != 0) {
			anim.SetTrigger ("reload");
		}
	}
		
	public virtual void MouseRightDown () {
		
	}

	public virtual void OnSkillAttackDown () {//Q键
		
	}

	public virtual void OnSpellCardAttackDown () {//G键
		
	}

	public virtual void OnReloadButtonDown () {

	}

	public virtual void ApplyCombo(string combo){
		Combo = combo;
	}

	public virtual void ApplyCombo(string combo,Vector3 StartPos,Vector3 TargetPos){
		Combo = combo;

		switch (combo) {
		case "0":
			dev.FireOneShoot (TargetPos,User,StartPos);
			break;
		}
	}

	public virtual void PopComboInputment (string input) {
		Combo = input;
		ApplyCombo (Combo);
	}
}
