using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class KappaScoutPlayer : PlayerBase {
	public float Speed = 10;
	public float FlyingTime = 5;
	public float CoolingTime = 10;
	public float Sensitive = 0.2f;
	public AudioClip EngineClip;
	public AudioClip FullSpeedEngineClip;

	private Rigidbody rigid;
	private float FlyingTimer = 0;
	private float CoolingTimer = 0;
	private AudioSource EngineAudiosourse;
	// Use this for initialization
	public override void Start () {
		base.Start ();
		rigid = GetComponent<Rigidbody> ();
		CoolingTimer = CoolingTime;
		EngineAudiosourse = GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	protected void Update () {
		base.Update ();
		CoolingTimer += Time.deltaTime;
		CoolingTimer = Mathf.Clamp (CoolingTimer, 0, CoolingTime);
		if (Input.GetKey (KeyCode.Space) && CoolingTimer >= CoolingTime) {
			Flying ();
			ModifyEngineSound (3);
		} else {
			if (controller.velocity.magnitude >= Mathf.Epsilon) {
				if (fpscontroller.m_IsWalking)
					ModifyEngineSound (1);
				else
					ModifyEngineSound (2);
			} else {
				ModifyEngineSound (0);
			}
		}
	}

	void Flying () {
		controller.Move(Camera.main.transform.forward * Speed);

		FlyingTimer += Time.deltaTime;
		if (FlyingTimer > FlyingTime) {
			FlyingTimer = 0;
			CoolingTimer = 0;
		}
	}

	void ModifyEngineSound(int Geer){
		switch (Geer) {
		case 0:
			EngineAudiosourse.clip = EngineClip;
			EngineAudiosourse.volume = Random.Range(0.1f,0.3f);
			if (!EngineAudiosourse.isPlaying)
				EngineAudiosourse.Play ();
			break;
		case 1:
			EngineAudiosourse.clip = EngineClip;
			EngineAudiosourse.volume = Random.Range(0.3f,0.6f);
			if (!EngineAudiosourse.isPlaying)
				EngineAudiosourse.Play ();
			break;
		case 2:
			EngineAudiosourse.clip = EngineClip;
			EngineAudiosourse.volume = 1.0f;
			if (!EngineAudiosourse.isPlaying)
				EngineAudiosourse.Play ();
			break;
		case 3:
			EngineAudiosourse.clip = FullSpeedEngineClip;
			EngineAudiosourse.volume = 1.0f;
			if (!EngineAudiosourse.isPlaying)
				EngineAudiosourse.Play ();
			break;
		}
	}
}
