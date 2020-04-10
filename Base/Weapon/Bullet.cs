using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public class Bullet : MonoBehaviour {
	public BulletStatement state = BulletStatement.Normal;
	public DamageProperty property;

	public Vector3 EnemyPOS;
	public float DestoiedTime = 5;
	public float speed = 800;
	public float BulletHigh;
	public GameObject[] BulletFX;

	[Header("声效")]
	public AudioClip[] MetalCrash;
	public AudioClip[] Crack;
	public AudioClip[] Ground;


	private bool isMoving = false;
	private Unit EnemyUnit;
	public Unit User;
	// Use this for initialization
	void Start () {
		if (EnemyPOS == null)
			Destroy (this.gameObject);
		Destroy(this.gameObject,DestoiedTime);
		if (state == BulletStatement.Tracking) {
			Collider[] colliders = Physics.OverlapSphere (EnemyPOS, 1);
			foreach (Collider c in colliders) {
				try {
					if (GameFactionManager.Instance.CheckFaction (User.States.FactionID, c.GetComponent<Unit> ().States.FactionID)) {
						EnemyUnit = c.GetComponent<Unit> ();
						break;
					}
				} catch {
					continue;
				}
			}
		} else if (state == BulletStatement.Grenade) {
			GetComponent<Rigidbody> ().velocity = transform.forward * speed;
		}
	}

	// Update is called once per frame
	void Update () {
		switch (state) {
		case BulletStatement.Normal:
			Vector3 oriPos = transform.position;
			transform.Translate (Vector3.forward * speed * Time.deltaTime);
			Vector3 direction = transform.position - oriPos;
			float length = (transform.position - oriPos).magnitude;

			RaycastHit hitinfo;
			bool isCollider = Physics.Raycast (oriPos, direction, out hitinfo, length);
			if (isCollider && hitinfo.collider.tag != Tags.barrier) {
				try {
					if (GameFactionManager.Instance.CheckFaction (User.States.FactionID, hitinfo.collider.GetComponent<Unit> ().States.FactionID))
						ApplyDamage (hitinfo.collider.GetComponent<Unit> ());
				} catch {}
				if (hitinfo.collider.GetComponent<Unit> () == User)
					return;
				Destroy (this.gameObject);
				int Index = Random.Range (0, BulletFX.Length);
				GeneratingExplosion (BulletFX [Index], hitinfo);
			}
			break;

		case BulletStatement.Cycle:
			if (!isMoving) {
				Vector3 center = (this.transform.position + EnemyPOS) * 0.5f;
				center += new Vector3 (0,BulletHigh, 0);
				List<Vector3> path = new List<Vector3> ();
				if (path.Count == 0) {
					path.Add (transform.position);
					path.Add (center);
					path.Add (EnemyPOS);
				}
				iTween.MoveTo (this.gameObject, iTween.Hash ("path", path.ToArray (), 
					"speed", speed,
					"looktarget", EnemyPOS,
					"looktime", 1f,
					"easetype", iTween.EaseType.easeOutCirc,
					"oncomplete","CycleEnd",
					"oncompleteparams",BulletFX[Random.Range(0,BulletFX.Length)]));
				isMoving = true;
			}
			break;

		case BulletStatement.Cycle_Tracking:
			if (EnemyUnit == null) {
				EnemyUnit = NearestEnemy();
			}
			if (!isMoving) {
				if (EnemyUnit != null) {
					EnemyPOS = EnemyUnit.transform.position;
				}
				Vector3 center = (this.transform.position + EnemyPOS) * 0.5f;
				center += new Vector3 (0,BulletHigh, 0);
				List<Vector3> path = new List<Vector3> ();
				if (path.Count == 0) {
					path.Add (transform.position);
					path.Add (center);
					path.Add (EnemyPOS);
				}
				iTween.MoveTo (this.gameObject, iTween.Hash ("path", path.ToArray (), 
					"speed", speed,
					"looktarget", EnemyPOS,
					"looktime", 1f,
					"easetype", iTween.EaseType.easeInCubic,
					"oncomplete","CycleEnd",
					"oncompleteparams",BulletFX[Random.Range(0,BulletFX.Length)]));
				isMoving = true;
			}
			break;

		case BulletStatement.Tracking:
			if (EnemyUnit)
				transform.LookAt (EnemyUnit.transform.position);
			else
				transform.LookAt (EnemyPOS);
			
			oriPos = transform.position;
			transform.Translate (Vector3.forward * speed * Time.deltaTime);
			direction = transform.position - oriPos;
			length = (transform.position - oriPos).magnitude;

			isCollider = Physics.Raycast (oriPos, direction, out hitinfo, length);
			if (isCollider && hitinfo.collider.tag != Tags.barrier) {
				try {
					if (GameFactionManager.Instance.CheckFaction (User.States.FactionID, hitinfo.collider.GetComponent<Unit>().States.FactionID)) {
						ApplyDamage (hitinfo.collider.GetComponent<Unit> ());
					}
				} catch {}
				Destroy (this.gameObject);
				int Index = Random.Range (0, BulletFX.Length);
				GeneratingExplosion (BulletFX [Index], hitinfo);
			}
		
			if (EnemyUnit == null) {
				EnemyUnit = NearestEnemy ();
			}
			break;
		}
	}

	void GeneratingExplosion (GameObject Explosion,RaycastHit Hitinfo) {
		GameObject go = GameObject.Instantiate (Explosion, Hitinfo.point, Quaternion.identity) as GameObject;
		go.transform.rotation = Quaternion.FromToRotation (go.transform.up, Hitinfo.normal);
		go.transform.Translate (Vector3.back * 0.001f);
		try {
			go.GetComponent<RangeDamage> ().Setup (User);
		} catch {

		}

		AudioSource audio = go.AddComponent<AudioSource> ();
		audio.minDistance = 3;
		audio.spatialBlend = 1;
		if (Hitinfo.collider.GetComponent<IVehicle> () != null) {
			int i = Random.Range (0, MetalCrash.Length);
			audio.clip = MetalCrash [i];
			audio.Play ();
		} else if (Hitinfo.collider.GetComponent<InfantryBasement> ()) {
			int i = Random.Range (0, Crack.Length);
			audio.clip = Crack [i];
			audio.Play ();
		} else if (Hitinfo.collider.tag == Tags.ground) {
			int i = Random.Range (0, Ground.Length);
			audio.clip = Ground [i];
			audio.Play ();
		}
	}

	void GeneratingExplosion (GameObject Explosion,Collision collision) {
		GameObject go = GameObject.Instantiate (Explosion, transform.position, Quaternion.identity) as GameObject;
		try {
			go.GetComponent<RangeDamage> ().Setup (User);
		} catch {

		}

		AudioSource audio = go.AddComponent<AudioSource> ();
		audio.minDistance = 3;
		audio.spatialBlend = 1;
		if (collision.collider.GetComponent<IVehicle> () != null) {
			int i = Random.Range (0, MetalCrash.Length);
			audio.clip = MetalCrash [i];
			audio.Play ();
		} else if (collision.collider.GetComponent<InfantryBasement> ()) {
			int i = Random.Range (0, Crack.Length);
			audio.clip = Crack [i];
			audio.Play ();
		} else if (collision.collider.tag == Tags.ground) {
			int i = Random.Range (0, Ground.Length);
			audio.clip = Ground [i];
			audio.Play ();
		}
	}

	void ApplyDamage (Unit Target) {
		if (ServerManager.Instance != null) {
			ServerManager.Instance.DamageRequest (User, property, Target);
		}
	}

	void CycleEnd (GameObject Explosion) {
		GameObject go = GameObject.Instantiate (Explosion, transform.position, Quaternion.identity) as GameObject;
		go.tag = User.tag;
		Destroy (this.gameObject);
		iTween.Stop(this.gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag != User.gameObject.tag) {
			Destroy (this.gameObject);
			int Index = Random.Range (0, BulletFX.Length);
			GeneratingExplosion (BulletFX [Index], collision);
		}
	}

	Unit NearestEnemy () {
		float minDist = Mathf.Infinity;
		float dist = 0;
		Unit nearest = null;
		if (User.tag == Tags.allies) {//阵营为人类
			GameObject[] EnemyNeedFind = GameObject.FindGameObjectsWithTag (Tags.enemy);

			if (EnemyNeedFind.Length == 1) {
				nearest = EnemyNeedFind [0].GetComponent<Unit> ();
			}
			for (int i = 0; i < EnemyNeedFind.Length; i++) {
				dist = Vector3.Distance (this.transform.position, EnemyNeedFind [i].transform.position);  

				if (dist < minDist) {
					minDist = dist;  
					nearest = EnemyNeedFind [i].GetComponent<Unit> ();
				}
			}
			return nearest;
		} else if (User.tag == Tags.enemy) {//阵营为月民
			GameObject[] Enemies = GameObject.FindGameObjectsWithTag (Tags.allies);
			GameObject[] Player = GameObject.FindGameObjectsWithTag (Tags.player);
			GameObject[] EnemyNeedFind =new GameObject[Enemies.Length + Player.Length];
			Enemies.CopyTo (EnemyNeedFind, 0);
			Player.CopyTo(EnemyNeedFind,Enemies.Length);
			if (EnemyNeedFind.Length == 1) {
				nearest = EnemyNeedFind [0].GetComponent<Unit> ();
			}
			for (int i = 0; i < EnemyNeedFind.Length; i++) {
				dist = Vector3.Distance (this.transform.position, EnemyNeedFind [i].transform.position);  

				if (dist < minDist) {
					minDist = dist;  
					nearest = EnemyNeedFind [i].GetComponent<Unit> ();
				}
				//Debug.Log (i);
			}
			return nearest;
		} else {
			//中立单位
			return null;
		}
	}

}

public enum BulletStatement {
	Tracking,
	Cycle,
	Cycle_Tracking,
	Normal,
	Grenade
}

[System.Serializable]
public class DamageProperty {
	public float KineticPower = 0;//Bullet
	public float ChemicalPower = 0;//Explosions
	public float ToxinPower = 0;//Fire,Poison,Ice,Water,etc...防御力如果这一栏不为空代表该单位有力场庇佑,免疫刀剑,碾压伤害
	public float SlashPower = 0;//Tank rolling,Sword.凯夫拉不防刀剑
	public float APPower = 0;//额外护盾,无论伤害多少优先扣护盾
}
