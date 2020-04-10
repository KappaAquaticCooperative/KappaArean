using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class ChildServerButton : MonoBehaviour {
	public Text text;

	private ChildServerInformation information;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Setup (ChildServerInformation info) {
		information = info;

		text.text = info.Name;
	}

	public void OnClick () {
		NetworkInterface.Instance.OnChildServerButtonClick (information);
	}
}
