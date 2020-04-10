using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManagement : ClientSingletion<InventoryManagement> {
	public List<InventoryProperty> InventoryDictionary = new List<InventoryProperty> ();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public GameObject GetFPSWeaponByID (int id) {
		foreach (InventoryProperty p in InventoryDictionary) {
			if (p.ID == id) {
				return p.FirstPersonWeapon;
			}
		}
		Debug.Log ("该ID为" + id + "的FPS武器不存在!请检查脚本");
		return null;
	}

	public GameObject GetTPSWeaponByID (int id) {
		foreach (InventoryProperty p in InventoryDictionary) {
			if (p.ID == id)
				return p.ThirdPersonWeapon;
		}
		Debug.Log ("该ID为"+id+"的TPS武器不存在!请检查脚本");
		return null;
	}

	public GameObject GetDropsByID (int id) {
		foreach (InventoryProperty p in InventoryDictionary) {
			if (p.ID == id)
				return p.Drops;
		}
		Debug.Log ("该ID为"+id+"的武器掉落不存在!请检查脚本");
		return null;
	}
}

[System.Serializable]
public class InventoryProperty {
	[Header("物品属性")]
	public int ID;
	public Texture WeaponTextrue;
	[Header("其他属性")]
	public GameObject ThirdPersonWeapon;
	public GameObject FirstPersonWeapon;
	public GameObject Drops;
}

[System.Serializable]
public class InventoryLocalization {
	public int ID;
	public string Name;
	public string Description;
}
