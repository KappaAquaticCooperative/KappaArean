using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;


public class PlayerBase : InfantryBasement {
	private CameraFilterPack_AAA_Blood_Hit screen_hit;
	private CameraFilterPack_FX_EarthQuake CameraShakeFX;
	private InventoryBasement Weapon;

	protected FirstPersonController fpscontroller;
	protected CharacterController controller;

	float RefreshTimer = 0.1f;
	// Use this for initialization
	public override void Start () {
		screen_hit = Camera.main.gameObject.AddComponent<CameraFilterPack_AAA_Blood_Hit> ();
		screen_hit.Hit_Left = 0;
		CameraShakeFX = Camera.main.gameObject.AddComponent<CameraFilterPack_FX_EarthQuake> ();
		CameraShakeFX.Speed = 0;
		CameraShakeFX.X = 0.01f;
		CameraShakeFX.Y = 0.01f;

		StartCoroutine (MovingRequest ());
		ChangingWeapon (0);

		fpscontroller = GetComponent<FirstPersonController> ();
		controller = GetComponent<CharacterController> ();
	}
	
	// Update is called once per frame
	protected void Update () {
		screen_hit.Hit_Full -= Time.deltaTime;
		screen_hit.Hit_Full = Mathf.Clamp01 (screen_hit.Hit_Full);

		if (CameraShakeFX.Speed >= Mathf.Epsilon) {
			CameraShakeFX.enabled = true;
			CameraShakeFX.Speed -= 400 * Time.deltaTime;
			CameraShakeFX.Speed = Mathf.Clamp (CameraShakeFX.Speed, 0, 100);
		} else {
			CameraShakeFX.enabled = false;
		}

		if (ClientManager.Instance.CanPlay) {
			CheckInteractive ();
			CheckWeapon ();
			ChangeWeapon ();
		} else {
			
		}
	}

	public void Shaking () {
		CameraShakeFX.Speed = 100;
	}

	void CheckInteractive () {
		if (Input.GetKeyDown (KeyCode.F) && InteractivingObject == null) {
			RaycastHit hitinfo;
			Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out hitinfo, 3);
			if (hitinfo.collider.GetComponent<IInteractable> () != null) {
				ClientManager.Instance.OnInteractiveStartRequest (this,  hitinfo.collider.GetComponent<Unit> ());
			}
		} else if (Input.GetKeyDown (KeyCode.F) && InteractivingObject != null) {
			ClientManager.Instance.OnInteractiveExitRequest (this, InteractivingObject);
//			InteractivingObject.OnInteractiveExit ();
		} else if (InteractivingObject != null) {
			fpscontroller.enabled = false;
			controller.enabled = false;
		} else if (InteractivingObject == null) {
			fpscontroller.enabled = true;
			controller.enabled = true;
		}
	}

	void CheckWeapon () {
		if (Weapon != null && Weapon.dev.Property.MaxAmmo != 0 && Weapon.dev.Property.AmmoInClip <= 0) {
			return;
		}
	}

	void ChangeWeapon () {
		try {
			if (Input.GetKey (KeyCode.Alpha1) && States.AvaliableWeapons [0] != States.CurrentWeaponID)
				ChangingWeapon (0);
			if (Input.GetKey (KeyCode.Alpha2) && States.AvaliableWeapons [1] != States.CurrentWeaponID)
				ChangingWeapon (1);
			if (Input.GetKey (KeyCode.Alpha3) && States.AvaliableWeapons [2] != States.CurrentWeaponID)
				ChangingWeapon (2);
			if (Input.GetKey (KeyCode.Alpha4) && States.AvaliableWeapons [3] != States.CurrentWeaponID)
				ChangingWeapon (3);
			if (Input.GetKey (KeyCode.Alpha5) && States.AvaliableWeapons [4] != States.CurrentWeaponID)
				ChangingWeapon (4);
			if (Input.GetKey (KeyCode.Alpha6) && States.AvaliableWeapons [5] != States.CurrentWeaponID)
				ChangingWeapon (5);
			if (Input.GetKey (KeyCode.Alpha7) && States.AvaliableWeapons [6] != States.CurrentWeaponID)
				ChangingWeapon (6);
			if (Input.GetKey (KeyCode.Alpha8) && States.AvaliableWeapons [7] != States.CurrentWeaponID)
				ChangingWeapon (7);
			if (Input.GetKey (KeyCode.Alpha9) && States.AvaliableWeapons [8] != States.CurrentWeaponID)
				ChangingWeapon (8);
		} catch {

		}
	}

	public override void TakeDamage (DamageProperty WeaponPro, Unit Attacker) {
		base.TakeDamage (WeaponPro, Attacker);
		try {
			Camera.main.transform.Rotate (-5, 0, 0);
			screen_hit.Hit_Full = 1;
		} catch {

		}
	}

	public void ChangingWeapon (int WeaponSlots) {
		if (InventoryManagement.Instance.GetFPSWeaponByID (States.AvaliableWeapons [WeaponSlots]) == null)
			return;
		if (Weapon != null)
			Destroy (Weapon.gameObject);
		
		States.CurrentWeaponID = States.AvaliableWeapons [WeaponSlots];
		GameObject go = GameObject.Instantiate (InventoryManagement.Instance.GetFPSWeaponByID (States.CurrentWeaponID), Camera.main.transform);
		Weapon = go.GetComponent<InventoryBasement> ();
		if (States.Weapons.ContainsKey (States.CurrentWeaponID))
			Weapon.dev.Property = States.Weapons [States.CurrentWeaponID];
		else
			States.Weapons.Add (States.CurrentWeaponID, Weapon.dev.Property);
		
		Weapon.User = this;
		ClientManager.Instance.ChangingWeaponRequest (WeaponSlots);
		//TODO 告诉服务器切换武器
	}

	public IEnumerator MovingRequest () {
		while (true) {
			Vector3 OldPosition = transform.position;
			Quaternion OldRotation = transform.rotation;
			yield return new WaitForSeconds (RefreshTimer);
			Vector3 CurrentPosition = transform.position;
			Quaternion CurrentRotation = transform.rotation;
			if (CurrentPosition != OldPosition || CurrentRotation != OldRotation) {
				ClientManager.Instance.MovingRequest (CurrentPosition, transform.rotation);
			}
		}
	}
}
