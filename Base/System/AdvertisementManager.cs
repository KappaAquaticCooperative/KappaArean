using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertisementManager : ClientSingletion<AdvertisementManager> {
	private string MyID = "3544137";

	// Use this for initialization
	void Start () {
		Debug.Log("Running precheck for Unity Ads initialization...");  

		if (!Advertisement.isSupported) {  
			Debug.LogError ("Unity Ads is not supported on the current runtime platform.");  
		} else if (Advertisement.isInitialized) {
			Debug.Log ("广告加载完毕");
		}
			
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
