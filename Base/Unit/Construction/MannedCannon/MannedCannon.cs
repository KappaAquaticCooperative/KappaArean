using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannedCannon : CannonBasement,IInteractable{
	public Transform OperatorPosition;

	public bool IsInteractiving = false;
	public Vector3 PlayerFocusingPoint = Vector3.zero;
	[HideInInspector]
	public Unit User;

	private float XAngle;
	// Use this for initialization
	void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		OnInteractiving ();
	}

	void OnInteractiving () {
		try {
			if (!IsInteractiving || User.tag != Tags.player)
				return;
		} catch {
			User = null;
			return;
		}

		//检查玩家焦点
		Transform PlayerTransform = Camera.main.transform;
		RaycastHit hitinfo;
		Vector3 point;//为了避免流量爆炸,我这边弄一个判断
		if (Physics.Raycast (PlayerTransform.position, PlayerTransform.forward, out hitinfo)) {
			point = hitinfo.point;
		} else {
			point = PlayerTransform.forward * 1000 + PlayerTransform.position;
		}
		if (point != PlayerFocusingPoint) {
			PlayerFocusingPoint = point;

			ClientManager.Instance.BuildingFocusRequest (this, PlayerFocusingPoint);
		}

		//控制视角旋转
		XAngle = XAngle + Input.GetAxis ("Mouse Y") * 1.5f;
		User.transform.Rotate (0, Input.GetAxis ("Mouse X") * 5, 0);
		XAngle = Mathf.Clamp (XAngle, MaxDownAngle, MaxUpAngle);
		Camera.main.transform.localEulerAngles = new Vector3 (-XAngle + transform.localEulerAngles.x, 0, 0);

		//开火
		if (Input.GetMouseButton (InputmentManagement.ShootButton)) {
			Firing (PlayerFocusingPoint);
		}
	}

	public Unit OnInteractiveStart (Unit user) {
		if (user == Initializator || Initializator == null) {
			IsInteractiving = true;
			User = user;
			try {
				Camera.main.transform.rotation = Quaternion.identity;
			} catch {

			}
			User.transform.SetPositionAndRotation (OperatorPosition.position, OperatorPosition.rotation);
			return this;
		} else {
			return null;
		}
	}

	public void OnInteractiveExit (Unit user) {
		IsInteractiving = false;
		User.transform.SetParent(null);
	}
}
