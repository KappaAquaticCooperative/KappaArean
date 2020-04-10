using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitFeedbackNumberUI : MonoBehaviour {
	public Text text;
	private Vector2 velocity;
	// Use this for initialization
	void Awale () {
		
	}

	void Start () {
		velocity = new Vector2(Vector2.right.x,Random.Range(-0.5f,0.5f))*2;
		Destroy (gameObject, 1f);
	}
	
	// Update is called once per frame
	void Update () {
		text.color = new Color(text.color.r,text.color.g,text.color.b,text.color.a-0.05f);
		text.rectTransform.anchoredPosition += velocity;
	}

	public void Setup (float Damage) {
		text = GetComponent<Text> ();
		text.text = "-" + Damage.ToString();
	}
}
