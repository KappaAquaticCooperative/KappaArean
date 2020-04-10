using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : ClientSingletion<AudioManager> {
	public AudioClip HitFeedbackSound;

	private AudioSource MusicAudioSource;
	private AudioSource SoundFXSource;
	// Use this for initialization
	void Start () {
		MusicAudioSource = gameObject.AddComponent<AudioSource> ();
		SoundFXSource = gameObject.AddComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayHitFeedbackSound () {
		SoundFXSource.PlayOneShot (HitFeedbackSound);
	}
}
