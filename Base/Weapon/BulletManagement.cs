using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManagement : ClientSingletion<BulletManagement> {
	public List<BulletProperty> Bullets = new List<BulletProperty> ();

	private Dictionary<int,BulletProperty> BulletDictionary = new Dictionary<int, BulletProperty>();

	void Awake () {
		foreach (BulletProperty property in Bullets) {
			BulletDictionary.Add (property.BulletID, property);
		}
	}

	public GameObject GetBulletByID (int ID) {
		if (BulletDictionary.ContainsKey (ID)) {
			return BulletDictionary [ID].Bullet;
		} else {
			Debug.LogError ("找不到ID为"+ID+"的子弹!");
			return null;
		}
	}

	public AudioClip GetClipByID(int ID){
		if (BulletDictionary.ContainsKey (ID)) {
			return BulletDictionary [ID].ShootingClips[Random.Range(0,BulletDictionary [ID].ShootingClips.Length-1)];
		} else {
			Debug.LogError ("找不到ID为"+ID+"的子弹音效!");
			return null;
		}
	}
}

[System.Serializable]
public class BulletProperty {
	public int BulletID;
	public GameObject Bullet;
	public AudioClip[] ShootingClips;
}
